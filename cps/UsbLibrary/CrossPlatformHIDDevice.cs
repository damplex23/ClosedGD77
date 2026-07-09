/*
 * Copyright (C) 2026 ClosedGD77
 *
 * Cross-platform USB HID device abstraction for CPS and firmware loader.
 * Uses LibUsbDotNet on Linux (libusb-1.0) and Windows (WinUSB/libusb-win32).
 *
 * Keeps same API as existing SpecifiedDevice so callers don't change.
 */

using System;
using System.Threading;

#if LINUX_BUILD
using LibUsbDotNet;
using LibUsbDotNet.Main;
#else
using UsbLibrary;
#endif

namespace UsbLibrary
{
    public class CrossPlatformSpecifiedDevice : IDisposable
    {
        public event DataRecievedEventHandler DataRecieved;
        public event DataSendEventHandler DataSend;

#if LINUX_BUILD
        // ---- LibUsbDotNet implementation ----
        private LibUsbDotNet.UsbDevice _usbDevice;
        private LibUsbDotNet.UsbEndpointReader _usbReader;
        private LibUsbDotNet.UsbEndpointWriter _usbWriter;
        private int _inputReportLength = 64;
        private int _outputReportLength = 64;

        public int OutputReportLength => _outputReportLength;
        public int InputReportLength => _inputReportLength;

        public static CrossPlatformSpecifiedDevice FindSpecifiedDevice(int vendor_id, int product_id)
        {
            var usbFinder = new UsbDeviceFinder(vendor_id, product_id);
            var usbDev = LibUsbDotNet.UsbDevice.OpenUsbDevice(usbFinder);

            if (usbDev == null)
            {
                Console.WriteLine("Device Not Found [0x{0:X4}:0x{1:X4}]", vendor_id, product_id);
                return null;
            }

            var dev = new CrossPlatformSpecifiedDevice();
            dev._usbDevice = usbDev;

            // Find endpoints
            foreach (var configInfo in usbDev.Configs)
            {
                foreach (var ifaceInfo in configInfo.InterfaceInfoList)
                {
                    if (ifaceInfo.Descriptor.EndpointCount >= 2)
                    {
                        foreach (var epInfo in ifaceInfo.EndpointInfoList)
                        {
                            var epId = epInfo.Descriptor.EndpointID;
                            if ((epId & 0x80) != 0)
                                dev._usbReader = usbDev.OpenEndpointReader((ReadEndpointID)epId);
                            else
                                dev._usbWriter = usbDev.OpenEndpointWriter((WriteEndpointID)epId);
                        }

                        // Claim interface
                        var wholeDev = usbDev as IUsbDevice;
                        if (wholeDev != null)
                        {
                            wholeDev.SetConfiguration(configInfo.Descriptor.ConfigID);
                            wholeDev.ClaimInterface(ifaceInfo.Descriptor.InterfaceID);
                        }
                        break;
                    }
                }
                if (dev._usbReader != null && dev._usbWriter != null) break;
            }

            if (dev._usbReader == null || dev._usbWriter == null)
            {
                Console.WriteLine("Could not find endpoints for device");
                usbDev.Close();
                return null;
            }

            return dev;
        }

        public static string ByteArrayToString(byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex.Replace("-", "");
        }

        public bool SendData(byte[] data)
        {
            return SendData(data, 0, data.Length);
        }

        public bool SendData(byte[] data, int index, int length)
        {
            try
            {
                // Format HID report: [report_id=1, 0, len_lo, len_hi, ...data...]
                byte[] sendBuffer = new byte[4 + length];
                sendBuffer[0] = 1;  // Report ID
                sendBuffer[1] = 0;
                sendBuffer[2] = (byte)(length & 0xFF);
                sendBuffer[3] = (byte)((length >> 8) & 0xFF);
                Array.Copy(data, index, sendBuffer, 4, length);

                int transferred;
                var ec = _usbWriter.Write(sendBuffer, 5000, out transferred);

                if (ec == ErrorCode.None)
                {
                    DataSend?.Invoke(this, new DataSendEventArgs(data));
                    return true;
                }

                Console.WriteLine("USB write error: {0}", ec);
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("SendData error: {0}", ex.Message);
                return false;
            }
        }

        public bool ReceiveData(byte[] data)
        {
            try
            {
                byte[] readBuffer = new byte[4096];
                int transferred;

                var ec = _usbReader.Read(readBuffer, 3000, out transferred);

                if (ec == ErrorCode.None && transferred > 4)
                {
                    // Skip 4-byte HID header
                    int copyLen = Math.Min(data.Length, transferred - 4);
                    Array.Copy(readBuffer, 4, data, 0, copyLen);

                    DataRecieved?.Invoke(this, new DataRecievedEventArgs(data));
                    return true;
                }

                if (ec == ErrorCode.IoTimedOut)
                    Console.WriteLine("USB read timeout");
                else if (ec != ErrorCode.None)
                    Console.WriteLine("USB read error: {0}", ec);

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ReceiveData error: {0}", ex.Message);
                return false;
            }
        }

        public void Dispose()
        {
            if (_usbDevice != null)
            {
                if (_usbDevice.IsOpen)
                {
                    var wholeDev = _usbDevice as IUsbDevice;
                    if (wholeDev != null)
                    {
                        wholeDev.ReleaseInterface(0);
                    }
                    _usbDevice.Close();
                }
                _usbDevice = null;
            }
        }
#else
        // ---- Original Windows SpecifiedDevice (passthrough) ----
        private SpecifiedDevice _winDevice;

        public int OutputReportLength => _winDevice?.OutputReportLength ?? 64;
        public int InputReportLength => _winDevice?.InputReportLength ?? 64;

        public static CrossPlatformSpecifiedDevice FindSpecifiedDevice(int vendor_id, int product_id)
        {
            var winDev = SpecifiedDevice.FindSpecifiedDevice(vendor_id, product_id);
            if (winDev == null) return null;

            var dev = new CrossPlatformSpecifiedDevice();
            dev._winDevice = winDev;

            // Wire events
            winDev.DataRecieved += (s, e) => dev.DataRecieved?.Invoke(dev, e);
            winDev.DataSend += (s, e) => dev.DataSend?.Invoke(dev, e);

            return dev;
        }

        public bool SendData(byte[] data)
        {
            return _winDevice?.SendData(data) ?? false;
        }

        public bool SendData(byte[] data, int index, int length)
        {
            return _winDevice?.SendData(data, index, length) ?? false;
        }

        public bool ReceiveData(byte[] data)
        {
            return _winDevice?.ReceiveData(data) ?? false;
        }

        public void Dispose()
        {
            _winDevice?.Dispose();
        }
#endif
    }
}
