/*
 * Copyright (C) 2026 ClosedGD77
 *
 * Cross-platform DfuSe firmware updater for STM32-based radios (DM-1701, UV380, RT-3S).
 * Wraps dfu-util CLI (must be installed: apt install dfu-util / brew install dfu-util).
 *
 * Supports:
 *   - DfuSe file (.dfu) parsing (VID/PID/Version extraction, CRC validation)
 *   - Firmware flash via dfu-util
 *   - Mass erase
 *   - Progress reporting compatible with existing FirmwareUpdateProgressEventHandler
 *
 * dfu-util STM32 device: VID=0x0483, PID=0xDF11 (default STM32 DFU bootloader)
 */

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace DMR
{
    public class FirmwareUpdateCrossPlatform : IFirmwareUpdate
    {
        // STM32 DFU device identifiers
        public const int STM32_DFU_VID = 0x0483;
        public const int STM32_DFU_PID = 0xDF11;

        private string _dfuUtilPath = "dfu-util";
        private Process _currentProcess;
        private bool _cancelRequested;

        public event FirmwareUpdateProgressEventHandler OnFirmwareUpdateProgress;

        public bool CancelComm
        {
            get => _cancelRequested;
            set
            {
                _cancelRequested = value;
                if (value && _currentProcess != null && !_currentProcess.HasExited)
                {
                    try { _currentProcess.Kill(); } catch { }
                }
            }
        }

        public void UpdateFirmware()
        {
            // Called by CPS UI — delegates to FlashFirmware with default STM32 DFU VID/PID
            FlashFirmware(null);
        }

        public void MassErase()
        {
            _cancelRequested = false;
            if (!CheckDfuUtil())
            {
                ReportProgress(100f, "Error: dfu-util not found", true, true);
                return;
            }
            int exitCode = RunDfuUtil("-d 0483:DF11 -s :mass-erase:force", (line) => { });
            if (exitCode == 0)
                ReportProgress(100f, "Mass erase complete", false, true);
            else
                ReportProgress(100f, "Mass erase failed (exit " + exitCode + ")", true, true);
        }

        bool IFirmwareUpdate.ParseDFU_File(string Filepath, out ushort VID, out ushort PID, out ushort Version)
        {
            return ParseDfuFile(Filepath, out VID, out PID, out Version);
        }

        /// <summary>
        /// Parse a DfuSe file header. Returns (vid, pid, version) or throws on error.
        /// Implements same validation as original FirmwareUpdate.ParseDFU_File.
        /// </summary>
        public static bool ParseDfuFile(string filepath, out ushort vid, out ushort pid, out ushort version)
        {
            vid = 0; pid = 0; version = 0;

            try
            {
                byte[] data = File.ReadAllBytes(filepath);

                // DfuSe signature: "DfuSe" at offset 0
                if (Encoding.UTF8.GetString(data, 0, 5) != "DfuSe")
                    throw new Exception("File signature error: not a DfuSe file");

                if (data[5] != 1)
                    throw new Exception("DFU file version must be 1");

                // Suffix: "UFD" at data.Length - 8
                if (data.Length < 16)
                    throw new Exception("File too short for DFU suffix");

                int suffixStart = data.Length - 16;
                if (Encoding.UTF8.GetString(data, suffixStart + 8, 3) != "UFD")
                    throw new Exception("File suffix error: UFD not found");

                if (data[suffixStart + 11] != 16)
                    throw new Exception("Suffix length error");

                version = BitConverter.ToUInt16(data, suffixStart);
                pid = BitConverter.ToUInt16(data, suffixStart + 2);
                vid = BitConverter.ToUInt16(data, suffixStart + 4);

                // CRC validation
                uint fileCrc = BitConverter.ToUInt32(data, suffixStart + 12);
                uint computedCrc = CalculateCrc32(data);
                if (fileCrc != computedCrc)
                    throw new Exception(string.Format("CRC mismatch: file={0:X8} computed={1:X8}", fileCrc, computedCrc));

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ParseDfuFile error: {0}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Flash a DfuSe firmware file to an STM32 radio in DFU mode.
        /// </summary>
        public bool FlashFirmware(string filepath, int vid = STM32_DFU_VID, int pid = STM32_DFU_PID)
        {
            _cancelRequested = false;

            if (!File.Exists(filepath))
            {
                ReportProgress(100f, "Error: Firmware file not found: " + filepath, true, true);
                return false;
            }

            // Validate file
            ushort fwVid, fwPid, fwVersion;
            if (!ParseDfuFile(filepath, out fwVid, out fwPid, out fwVersion))
            {
                ReportProgress(100f, "Error: Invalid or corrupt firmware file", true, true);
                return false;
            }

            ReportProgress(5f, string.Format("Firmware: VID=0x{0:X4} PID=0x{1:X4} v{2}", fwVid, fwPid, fwVersion), false, false);

            // Check dfu-util is available
            if (!CheckDfuUtil())
            {
                ReportProgress(100f, "Error: dfu-util not found. Install: sudo apt install dfu-util", true, true);
                return false;
            }

            // Detect device in DFU mode
            string deviceFilter = string.Format("{0:X4}:{1:X4}", vid, pid);
            ReportProgress(10f, "Checking for STM32 device in DFU mode...", false, false);

            if (!DetectDevice(deviceFilter))
            {
                string msg = string.Format(
                    "No DFU device found (VID=0x{0:X4} PID=0x{1:X4}).\n" +
                    "Put radio in DFU mode: hold PTT+SK1 while powering on, or use 'SK2 + Power' on DM-1701.\n" +
                    "On Linux you may need: sudo dfu-util -l",
                    vid, pid);
                ReportProgress(100f, msg, true, true);
                return false;
            }

            // Flash with dfu-util
            // dfu-util -d vid:pid -D file.dfu -s :leave
            ReportProgress(20f, "Erasing and flashing firmware...", false, false);

            string args = string.Format("-d {0}:{1} -D \"{2}\" -s :leave",
                vid.ToString("X4"), pid.ToString("X4"), filepath);

            try
            {
                int exitCode = RunDfuUtil(args, (line) =>
                {
                    // Parse dfu-util output for progress
                    // dfu-util outputs lines like: "Download	[=========================] 100%        65536 bytes"
                    if (line.Contains("%"))
                    {
                        int pctIdx = line.LastIndexOf('%');
                        if (pctIdx > 0)
                        {
                            // Walk back to find start of number
                            int numStart = pctIdx - 1;
                            while (numStart > 0 && char.IsDigit(line[numStart - 1]))
                                numStart--;

                            if (int.TryParse(line.Substring(numStart, pctIdx - numStart), out int pct))
                            {
                                float progress = 20f + (pct * 0.75f); // 20% to 95%
                                ReportProgress(progress, line.Trim(), false, false);
                            }
                        }
                    }
                });

                if (exitCode == 0)
                {
                    ReportProgress(100f, "Firmware update complete! Radio will restart.", false, true);
                    return true;
                }
                else
                {
                    ReportProgress(100f, string.Format("dfu-util exited with code {0}", exitCode), true, true);
                    return false;
                }
            }
            catch (Exception ex)
            {
                ReportProgress(100f, "Error: " + ex.Message, true, true);
                return false;
            }
        }

        /// <summary>
        /// List DFU devices connected to the system.
        /// </summary>
        public static string[] ListDevices()
        {
            try
            {
                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "dfu-util",
                        Arguments = "-l",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                proc.Start();
                string output = proc.StandardOutput.ReadToEnd() + proc.StandardError.ReadToEnd();
                proc.WaitForExit(5000);
                return output.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            }
            catch
            {
                return new string[0];
            }
        }

        // ---- Private helpers ----

        private bool CheckDfuUtil()
        {
            try
            {
                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = _dfuUtilPath,
                        Arguments = "--version",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                proc.Start();
                proc.WaitForExit(3000);
                return proc.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        private bool DetectDevice(string filter)
        {
            try
            {
                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = _dfuUtilPath,
                        Arguments = "-l",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                proc.Start();
                string output = proc.StandardOutput.ReadToEnd() + proc.StandardError.ReadToEnd();
                proc.WaitForExit(5000);

                // dfu-util -l output: "Found DFU: [vid:pid] ..."
                return output.Contains(filter);
            }
            catch
            {
                return false;
            }
        }

        private int RunDfuUtil(string args, Action<string> outputHandler)
        {
            _currentProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _dfuUtilPath,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            _currentProcess.OutputDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    outputHandler(e.Data);
            };
            _currentProcess.ErrorDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    outputHandler(e.Data);
            };

            _currentProcess.Start();
            _currentProcess.BeginOutputReadLine();
            _currentProcess.BeginErrorReadLine();

            // Wait with cancellation support
            while (!_currentProcess.HasExited)
            {
                if (_cancelRequested)
                {
                    try { _currentProcess.Kill(); } catch { }
                    return -1;
                }
                Thread.Sleep(100);
            }

            _currentProcess.WaitForExit();
            return _currentProcess.ExitCode;
        }

        private void ReportProgress(float percent, string message, bool isError, bool isComplete)
        {
            OnFirmwareUpdateProgress?.Invoke(this, new FirmwareUpdateProgressEventArgs(percent, message, isError, isComplete));
        }

        private static uint[] _crcTable = new uint[256]
        {
            0x00000000, 0x77073096, 0xEE0E612C, 0x990951BA,
            0x076DC419, 0x706AF48F, 0xE963A535, 0x9E6495A3,
            0x0EDB8832, 0x79DCB8A4, 0xE0D5E91E, 0x97D2D988,
            0x09B64C2B, 0x7EB17CBD, 0xE7B82D07, 0x90BF1D91,
            0x1DB71064, 0x6AB020F2, 0xF3B97148, 0x84BE41DE,
            0x1ADAD47D, 0x6DDDE4EB, 0xF4D4B551, 0x83D385C7,
            0x136C9856, 0x646BA8C0, 0xFD62F97A, 0x8A65C9EC,
            0x14015C4F, 0x63066CD9, 0xFA0F3D63, 0x8D080DF5,
            0x3B6E20C8, 0x4C69105E, 0xD56041E4, 0xA2677172,
            0x3C03E4D1, 0x4B04D447, 0xD20D85FD, 0xA50AB56B,
            0x35B5A8FA, 0x42B2986C, 0xDBBBC9D6, 0xACBCF940,
            0x32D86CE3, 0x45DF5C75, 0xDCD60DCF, 0xABD13D59,
            0x26D930AC, 0x51DE003A, 0xC8D75180, 0xBFD06116,
            0x21B4F4B5, 0x56B3C423, 0xCFBA9599, 0xB8BDA50F,
            0x2802B89E, 0x5F058808, 0xC60CD9B2, 0xB10BE924,
            0x2F6F7C87, 0x58684C11, 0xC1611DAB, 0xB6662D3D,
            0x76DC4190, 0x01DB7106, 0x98D220BC, 0xEFD5102A,
            0x71B18589, 0x06B6B51F, 0x9FBFE4A5, 0xE8B8D433,
            0x7807C9A2, 0x0F00F934, 0x9609A88E, 0xE10E9818,
            0x7F6A0DBB, 0x086D3D2D, 0x91646C97, 0xE6635C01,
            0x6B6B51F4, 0x1C6C6162, 0x856530D8, 0xF262004E,
            0x6C0695ED, 0x1B01A57B, 0x8208F4C1, 0xF50FC457,
            0x65B0D9C6, 0x12B7E950, 0x8BBEB8EA, 0xFCB9887C,
            0x62DD1DDF, 0x15DA2D49, 0x8CD37CF3, 0xFBD44C65,
            0x4DB26158, 0x3AB551CE, 0xA3BC0074, 0xD4BB30E2,
            0x4ADFA541, 0x3DD895D7, 0xA4D1C46D, 0xD3D6F4FB,
            0x4369E96A, 0x346ED9FC, 0xAD678846, 0xDA60B8D0,
            0x44042D73, 0x33031DE5, 0xAA0A4C5F, 0xDD0D7CC9,
            0x5005713C, 0x270241AA, 0xBE0B1010, 0xC90C2086,
            0x5768B525, 0x206F85B3, 0xB966D409, 0xCE61E49F,
            0x5EDEF90E, 0x29D9C998, 0xB0D09822, 0xC7D7A8B4,
            0x59B33D17, 0x2EB40D81, 0xB7BD5C3B, 0xC0BA6CAD,
            0xEDB88320, 0x9ABFB3B6, 0x03B6E20C, 0x74B1D29A,
            0xEAD54739, 0x9DD277AF, 0x04DB2615, 0x73DC1683,
            0xE3630B12, 0x94643B84, 0x0D6D6A3E, 0x7A6A5AA8,
            0xE40ECF0B, 0x9309FF9D, 0x0A00AE27, 0x7D079EB1,
            0xF00F9344, 0x8708A3D2, 0x1E01F268, 0x6906C2FE,
            0xF762575D, 0x806567CB, 0x196C3671, 0x6E6B06E7,
            0xFED41B76, 0x89D32BE0, 0x10DA7A5A, 0x67DD4ACC,
            0xF9B9DF6F, 0x8EBEEFF9, 0x17B7BE43, 0x60B08ED5,
            0xD6D6A3E8, 0xA1D1937E, 0x38D8C2C4, 0x4FDFF252,
            0xD1BB67F1, 0xA6BC5767, 0x3FB506DD, 0x48B2364B,
            0xD80D2BDA, 0xAF0A1B4C, 0x36034AF6, 0x41047A60,
            0xDF60EFC3, 0xA867DF55, 0x316E8EEF, 0x4669BE79,
            0xCB61B38C, 0xBC66831A, 0x256FD2A0, 0x5268E236,
            0xCC0C7795, 0xBB0B4703, 0x220216B9, 0x5505262F,
            0xC5BA3BBE, 0xB2BD0B28, 0x2BB45A92, 0x5CB30A04,
            0xC2D7FFA7, 0xB5D0CF31, 0x2CD99E8B, 0x5BDEAE1D,
            0x9B64C2B0, 0xEC63F226, 0x756AA39C, 0x026D930A,
            0x9C0906A9, 0xEB0E363F, 0x72076785, 0x05005713,
            0x95BF4A82, 0xE2B87A14, 0x7BB12BAE, 0x0CB61B38,
            0x92D28E9B, 0xE5D5BE0D, 0x7CDCEFB7, 0x0BDBDF21,
            0x86D3D2D4, 0xF1D4E242, 0x68DDB3F8, 0x1FDA836E,
            0x81BE16CD, 0xF6B9265B, 0x6FB077E1, 0x18B74777,
            0x88085AE6, 0xFF0F6A70, 0x66063BCA, 0x11010B5C,
            0x8F659EFF, 0xF862AE69, 0x616BFFD3, 0x166CCF45,
            0xA00AE278, 0xD70DD2EE, 0x4E048354, 0x3903B3C2,
            0xA7672661, 0xD06016F7, 0x4969474D, 0x3E6E77DB,
            0xAED16A4A, 0xD9D65ADC, 0x40DF0B66, 0x37D83BF0,
            0xA9BCAE53, 0xDEBB9EC5, 0x47B2CF7F, 0x30B5FFE9,
            0xBDBDF21C, 0xCABAC28A, 0x53B39330, 0x24B4A3A6,
            0xBAD03605, 0xCDD70693, 0x54DE5729, 0x23D967BF,
            0xB3667A2E, 0xC4614AB8, 0x5D681B02, 0x2A6F2B94,
            0xB40BBE37, 0xC30C8EA1, 0x5A05DF1B, 0x2D02EF8D
        };

        private static uint CalculateCrc32(byte[] data)
        {
            uint crc = 0xFFFFFFFF;
            for (int i = 0; i < data.Length - 4; i++)
            {
                crc = _crcTable[(crc ^ data[i]) & 0xFF] ^ (crc >> 8);
            }
            return crc;
        }
    }
}
