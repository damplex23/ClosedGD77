using System;
using System.IO;
using System.Runtime.InteropServices;

namespace DMR
{
    /// <summary>
    /// ClosedGD77: Enhanced encryption key storage — separate from the legacy codeplug Encrypt blob.
    /// Stores up to 32 keys with per-key algorithm, up to 32-byte key material (AES-256).
    /// Written to a .keys file alongside the codeplug.
    ///
    /// Firmware key transfer protocol (cpsHandleCommand case 7/8):
    ///   com_requestbuffer[0] = 'C'
    ///   com_requestbuffer[1] = 7 (MK22) or 8 (STM32)
    ///   com_requestbuffer[2] = key_slot
    ///   com_requestbuffer[3] = algorithm
    ///   com_requestbuffer[4] = key_length
    ///   com_requestbuffer[5..20] = name (16 bytes)
    ///   com_requestbuffer[21..52] = key_data (32 bytes)
    /// </summary>

    public enum EncryptionAlgorithm : byte
    {
        None      = 0,
        ARC4      = 1,   // DMR Enhanced Privacy (40-bit DMRA standard)
        AES128    = 2,   // AES-128-CTR (STM32 only)
        AES256    = 3,   // AES-256-CTR (STM32 only)
        Scrambler = 4    // Analog FM frequency inversion
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct EncryptionKeyEntry
    {
        public byte key_id;                     // 0..31
        public byte algorithm;                  // EncryptionAlgorithm cast
        public byte key_length;                 // actual key bytes used (0..32)
        public byte active;                     // 1 = this slot holds a key
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] key_name;                 // UTF-8, null-terminated
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] key_data;                 // raw key material, zero-padded

        public static EncryptionKeyEntry CreateNew(byte id)
        {
            EncryptionKeyEntry e = new EncryptionKeyEntry();
            e.key_id = id;
            e.algorithm = 0;
            e.key_length = 0;
            e.active = 0;
            e.key_name = new byte[16];
            e.key_data = new byte[32];
            return e;
        }

        public bool IsActive
        {
            get { return active != 0; }
            set { active = (byte)(value ? 1 : 0); }
        }

        public string Name
        {
            get
            {
                if (key_name == null) return "";
                int len = 0;
                while (len < 16 && key_name[len] != 0) len++;
                return System.Text.Encoding.UTF8.GetString(key_name, 0, len);
            }
            set
            {
                if (key_name == null) key_name = new byte[16];
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(value ?? "");
                int copyLen = Math.Min(bytes.Length, 15);
                Array.Clear(key_name, 0, 16);
                Buffer.BlockCopy(bytes, 0, key_name, 0, copyLen);
                key_name[copyLen] = 0;
            }
        }

        public string KeyHex
        {
            get
            {
                if (key_data == null || key_length == 0) return "";
                char[] hex = new char[key_length * 2];
                for (int i = 0; i < key_length; i++)
                {
                    byte b = key_data[i];
                    hex[i * 2] = ToHexChar(b >> 4);
                    hex[i * 2 + 1] = ToHexChar(b & 0xF);
                }
                return new string(hex);
            }
            set
            {
                if (key_data == null) key_data = new byte[32];
                Array.Clear(key_data, 0, 32);
                string hex = (value ?? "").Replace(" ", "").Replace("-", "");
                if (hex.Length % 2 != 0) hex = hex.PadRight(hex.Length + 1, '0');
                int byteCount = Math.Min(hex.Length / 2, 32);
                for (int i = 0; i < byteCount; i++)
                {
                    key_data[i] = byte.Parse(hex.Substring(i * 2, 2),
                        System.Globalization.NumberStyles.HexNumber);
                }
                key_length = (byte)byteCount;
            }
        }

        private static char ToHexChar(int nibble)
        {
            return (char)(nibble < 10 ? '0' + nibble : 'A' + nibble - 10);
        }
    }

    /// <summary>
    /// Manages encryption keys for the ClosedGD77 CPS.
    /// Stored in a .keys binary file separate from the codeplug to avoid
    /// changing the legacy codeplug memory layout.
    /// </summary>
    public class EncryptionKeyStore
    {
        public const int MAX_SLOTS = 32;
        private const int FILE_MAGIC = 0x4B444743; // "CGDK"
        private const int FILE_VERSION = 1;

        public EncryptionKeyEntry[] Keys;

        public EncryptionKeyStore()
        {
            Keys = new EncryptionKeyEntry[MAX_SLOTS];
            for (int i = 0; i < MAX_SLOTS; i++)
            {
                Keys[i] = EncryptionKeyEntry.CreateNew((byte)i);
            }
        }

        public int Count
        {
            get
            {
                int n = 0;
                for (int i = 0; i < MAX_SLOTS; i++)
                    if (Keys[i].IsActive) n++;
                return n;
            }
        }

        public EncryptionKeyEntry GetKey(int slot)
        {
            if (slot < 0 || slot >= MAX_SLOTS) return EncryptionKeyEntry.CreateNew(0);
            return Keys[slot];
        }

        public void SetKey(int slot, EncryptionKeyEntry key)
        {
            if (slot < 0 || slot >= MAX_SLOTS) return;
            key.key_id = (byte)slot;
            Keys[slot] = key;
        }

        public void DeleteKey(int slot)
        {
            if (slot < 0 || slot >= MAX_SLOTS) return;
            Keys[slot] = EncryptionKeyEntry.CreateNew((byte)slot);
        }

        /// <summary>
        /// Build the 53-byte packet for firmware key transfer.
        /// Packet: ['C'][cmd7or8][key_slot][algorithm][key_len][name(16)][key_data(32)]
        /// </summary>
        public byte[] BuildTransferPacket(int slot, byte commandByte)
        {
            byte[] packet = new byte[53];
            if (slot < 0 || slot >= MAX_SLOTS) return packet;

            EncryptionKeyEntry k = Keys[slot];
            packet[0] = (byte)'C';
            packet[1] = commandByte;       // 7 = MK22, 8 = STM32
            packet[2] = k.key_id;
            packet[3] = k.algorithm;
            packet[4] = k.key_length;
            if (k.key_name != null)
                Buffer.BlockCopy(k.key_name, 0, packet, 5, Math.Min(k.key_name.Length, 16));
            if (k.key_data != null)
                Buffer.BlockCopy(k.key_data, 0, packet, 21, Math.Min(k.key_data.Length, 32));
            return packet;
        }

        public bool SaveToFile(string path)
        {
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    bw.Write(FILE_MAGIC);
                    bw.Write(FILE_VERSION);
                    bw.Write(MAX_SLOTS);
                    for (int i = 0; i < MAX_SLOTS; i++)
                    {
                        EncryptionKeyEntry k = Keys[i];
                        bw.Write(k.key_id);
                        bw.Write(k.algorithm);
                        bw.Write(k.key_length);
                        bw.Write(k.active);
                        byte[] name = k.key_name ?? new byte[16];
                        bw.Write(name, 0, 16);
                        byte[] data = k.key_data ?? new byte[32];
                        bw.Write(data, 0, 32);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("EncryptionKeyStore.SaveToFile: " + ex.Message);
                return false;
            }
        }

        public bool LoadFromFile(string path)
        {
            if (!File.Exists(path)) return false;
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                using (BinaryReader br = new BinaryReader(fs))
                {
                    int magic = br.ReadInt32();
                    if (magic != FILE_MAGIC) return false;
                    int version = br.ReadInt32();
                    int slots = br.ReadInt32();
                    int count = Math.Min(slots, MAX_SLOTS);
                    for (int i = 0; i < count; i++)
                    {
                        Keys[i].key_id = br.ReadByte();
                        Keys[i].algorithm = br.ReadByte();
                        Keys[i].key_length = br.ReadByte();
                        Keys[i].active = br.ReadByte();
                        Keys[i].key_name = br.ReadBytes(16);
                        Keys[i].key_data = br.ReadBytes(32);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("EncryptionKeyStore.LoadFromFile: " + ex.Message);
                return false;
            }
        }
    }
}
