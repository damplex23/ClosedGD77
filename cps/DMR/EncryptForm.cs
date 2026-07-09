using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using UsbLibrary;
#if LINUX_BUILD
using HIDSpecifiedDevice = UsbLibrary.CrossPlatformSpecifiedDevice;
#else
using HIDSpecifiedDevice = UsbLibrary.SpecifiedDevice;
#endif

namespace DMR
{
	public class EncryptForm : DockContent, IDisp
	{
		// ClosedGD77: extended encryption algorithm types
		// Encrypt byte format (matches firmware encryption.h ENC_ALGO_* enum):
		//   bits 0-4: key ID (0-31)
		//   bits 5-7: algorithm (0=None, 1=ARC4, 2=AES-128)
		//
		// Key transfer USB command packet:
		//   ['C'][cmd=N][key_slot][algorithm][key_len][name(16)][key_data(32)]  (53 bytes)
		//   MK22 (GD-77, DM-1801, RD-5R): command 7
		//   STM32 (DM-1701, UV380, RT-3S):  command 8 (command 7 is GPS on STM32)
		// ClosedGD77: Scrambler removed from codeplug — it's a radio-side menu toggle.
	// AES-256 removed to fit legacy Encrypt.keyList (8 bytes/key max).
	private enum EncryptType
	{
		None   = 0,
		ARC4   = 1,  // ARC4 Enhanced Privacy (DMRA standard, 40-bit)
		AES128 = 2   // AES-128-CTR (STM32 only)
	}

		private enum KeyLen
	{
		Length32,   // 32-bit (4 bytes, ARC4 basic)
		Length64,   // 64-bit (8 bytes, ARC4 extended)
		Length40,   // 40-bit (5 bytes, ARC4 DMRA standard)
		Length128   // 128-bit (16 bytes, AES-128)
	}

		// ClosedGD77: Encryption key store for WriteKeysToRadio
		public static EncryptionKeyStore KeyStore = new EncryptionKeyStore();

		// Radio model detection for command byte selection
		// MK22 models: GD-77 (MD-760/P), DM-1801 (MD-2017), RD-5R
		// STM32 models: DM-1701 (MD-UV380), RT-3S (MD-9600)
		private static bool IsSTM32Model()
		{
			string model = System.Text.Encoding.ASCII.GetString(Settings.CUR_MODEL).Trim('\0', '\xFF', ' ');
			return model.Contains("UV380") || model.Contains("9600") || model.Contains("1701");
		}

		private static byte GetKeyTransferCommand()
		{
			return (byte)(IsSTM32Model() ? 8 : 7);
		}

		[Serializable]
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public class Encrypt : IVerify<Encrypt>, IData
		{
			private byte type;

			private byte keyLen;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
			private byte[] keyIndex;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
			private byte[] reserve;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
			private byte[] keyList;

			public int Type
			{
				get
				{
					return this.type;
				}
				set
				{
					this.type = Convert.ToByte(value);
				}
			}

			public int KeyLen
			{
				get
				{
					return this.keyLen;
				}
				set
				{
					this.keyLen = Convert.ToByte(value);
				}
			}

			public bool this[int index]
			{
				get
				{
					if (index < 16)
					{
						BitArray bitArray = new BitArray(this.keyIndex);
						return bitArray[index];
					}
					return false;
				}
				set
				{
					if (index < 16)
					{
						BitArray bitArray = new BitArray(this.keyIndex);
						bitArray[index] = value;
						bitArray.CopyTo(this.keyIndex, 0);
					}
				}
			}

			public int Count
			{
				get
				{
					return 16;
				}
			}

			public string Format
			{
				get
				{
					return "";
				}
			}

			public bool ListIsEmpty
			{
				get
				{
					int num = 0;
					while (true)
					{
						if (num < this.Count)
						{
							if (this.DataIsValid(num))
							{
								break;
							}
							num++;
							continue;
						}
						return true;
					}
					return false;
				}
			}

			public Encrypt()
			{
				
				//base._002Ector();
				this.keyIndex = new byte[2];
				this.reserve = new byte[4];
				this.keyList = new byte[128];
			}

			public void RemoveAt(int index)
			{
				int i = 0;
				this[index] = false;
				for (; i < 8; i++)
				{
					this.keyList[i + index * 8] = 255;
				}
			}

			public void Insert(int index)
			{
				int i = 0;
				this[index] = true;
				for (; i < 8; i++)
				{
					this.keyList[i + index * 8] = Convert.ToByte("53474C3953474C39".Substring(i * 2, 2));
				}
			}

			public void Clear()
			{
				for (int i = 0; i < this.keyIndex.Length; i++)
				{
					this.keyIndex[i] = 0;
				}
				for (int i = 0; i < this.keyList.Length; i++)
				{
					this.keyList[i] = 255;
				}
			}

			public string GetKey(int index)
			{
				if (index >= 16)
				{
					return "";
				}
				int i = 0;
				StringBuilder stringBuilder = new StringBuilder(16);
				for (; i < 8; i++)
				{
					byte b = this.keyList[i + index * 8];
					stringBuilder.Append(b.ToString("X2"));
				}
				if (EncryptForm.data.KeyLen == 0)
				{
					return stringBuilder.ToString().Substring(0, 8);
				}
				return stringBuilder.ToString();
			}

			public void SetKey(int index, string key)
			{
				if (index < 16)
				{
					for (int i = 0; i < key.Length / 2; i++)
					{
						this.keyList[i + index * 8] = Convert.ToByte(key.Substring(i * 2, 2), 16);
					}
					if (key.Length == 8)
					{
						Array.Copy(this.keyList, index * 8, this.keyList, index * 8 + 4, 4);
					}
				}
			}

			public int GetMinIndex()
			{
				int num = 0;
				for (num = 0; num < this.Count; num++)
				{
				}
				return -1;
			}

			public bool DataIsValid(int index)
			{
				if (index < 16)
				{
					BitArray bitArray = new BitArray(this.keyIndex);
					return bitArray[index];
				}
				return false;
			}

			public void SetIndex(int index, int value)
			{
				if (value == 0)
				{
					this.SetName(index, "");
				}
			}

			public void ClearIndex(int index)
			{
				this.SetName(index, "");
			}

			public string GetMinName(TreeNode node)
			{
				return "";
			}

			public void SetName(int index, string text)
			{
			}

			public string GetName(int index)
			{
				return this.GetKey(index);
			}

			public void Default(int index)
			{
			}

			public void Paste(int from, int to)
			{
			}

			public void Verify(Encrypt def)
			{
				if (!Enum.IsDefined(typeof(EncryptType), (int)this.type))
				{
					this.type = def.type;
				}
				if (!Enum.IsDefined(typeof(KeyLen), (int)this.keyLen))
				{
					this.keyLen = def.keyLen;
				}
			}
		}

		private const int CNT_KEY = 16;

		private const int CNT_KEY_INDEX = 2;

		private const int SPACE_PER_KEY = 8;

		private const int LEN_KEY_32BIT = 8;

		private const int LEN_KEY_64BIT = 16;

		private const int LEN_KEY_128BIT = 32;

		public const string SZ_ENCRYPT_TYPE_NAME = "EncryptType";

		private const string DEF_KEY = "53474C3953474C39";

		private static readonly string[] SZ_ENCRYPT_TYPE;

		private static readonly string[] SZ_KEY_LEN;

		public static Encrypt DefaultEncrypt;

		public static Encrypt data;

		//private IContainer components;

		private Label lblType;

		private ComboBox cmbType;

		private Label lblKeyLen;

		private ComboBox cmbKeyLen;

		private DataGridView dgvKey;

		private Button btnDel;

		private Button btnAdd;

		private Button btnWriteRadio;  // ClosedGD77: Write keys to radio

		private DataGridViewTextBoxColumn txtKey;

		private CustomPanel pnlEncrypt;

		public TreeNode Node
		{
			get;
			set;
		}

		public EncryptForm()
		{
			
			//base._002Ector();
			this.InitializeComponent();
			base.Scale(Settings.smethod_6());
		}

        int _003CPreKeyLen_003Ek__BackingField;
		[CompilerGenerated]
		private int method_0()
		{
			return this._003CPreKeyLen_003Ek__BackingField;
		}

		[CompilerGenerated]
		private void method_1(int value)
		{
			this._003CPreKeyLen_003Ek__BackingField = value;
		}

		public void SaveData()
		{
			int i = 0;
			int num = 0;
			string text = null;
			EncryptForm.data.Type = (byte)this.cmbType.SelectedIndex;
			EncryptForm.data.KeyLen = (byte)this.cmbKeyLen.SelectedIndex;
			this.dgvKey.EndEdit();
			EncryptForm.data.Clear();
			for (; i < this.dgvKey.Rows.Count; i++)
			{
				num = (int)this.dgvKey.Rows[i].Tag;
				EncryptForm.data[num] = true;
				text = this.dgvKey.Rows[i].Cells[0].Value.ToString();
				EncryptForm.data.SetKey(num, text);
			}
			SyncToKeyStore();
			SaveKeyStoreToFile();
		}

		public void DispData()
		{
			int num = 0;
			int num2 = 0;
			// Clamp to current enum range — old codeplugs may have Type=3 (AES-256) or Type=4 (Scrambler)
			int typeVal = EncryptForm.data.Type;
			if (typeVal < 0 || typeVal >= this.cmbType.Items.Count) typeVal = 0;
			this.cmbType.SelectedIndex = typeVal;
			int keyLenVal = EncryptForm.data.KeyLen;
			if (keyLenVal < 0 || keyLenVal >= this.cmbKeyLen.Items.Count) keyLenVal = 0;
			this.cmbKeyLen.SelectedIndex = keyLenVal;
			// Key length hex char counts: 32-bit=8, 64-bit=16, 40-bit=10, 128-bit=32
			int[] hexLenMap = { 8, 16, 10, 32 };
			num2 = (this.cmbKeyLen.SelectedIndex < hexLenMap.Length) ? hexLenMap[this.cmbKeyLen.SelectedIndex] : 8;
			this.txtKey.MaxInputLength = num2;
			this.dgvKey.Rows.Clear();
			for (num = 0; num < 16; num++)
			{
				if (EncryptForm.data[num])
				{
					int index = this.dgvKey.Rows.Add();
					this.dgvKey.Rows[index].Tag = num;
					this.dgvKey.Rows[index].Cells[0].Value = EncryptForm.data.GetKey(num).Substring(0, Math.Min(num2, EncryptForm.data.GetKey(num).Length));
				}
			}
			this.method_3();
			this.RefreshByUserMode();
		}

		public void RefreshByUserMode()
		{
			bool flag = Settings.getUserExpertSettings() == Settings.UserMode.Expert;
			this.lblType.Enabled &= flag;
			this.cmbType.Enabled &= flag;
			this.lblKeyLen.Enabled &= flag;
			this.cmbKeyLen.Enabled &= flag;
			this.btnAdd.Enabled &= flag;
			this.btnDel.Enabled &= flag;
			this.dgvKey.Enabled &= flag;
		}

		public void RefreshName()
		{
		}

		private void method_2()
		{
			DataGridViewTextBoxColumn dataGridViewTextBoxColumn = this.dgvKey.Columns[0] as DataGridViewTextBoxColumn;
			dataGridViewTextBoxColumn.MaxInputLength = 64; // up to 256-bit keys
			dataGridViewTextBoxColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
			dataGridViewTextBoxColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
			Settings.smethod_37(this.cmbType, EncryptForm.SZ_ENCRYPT_TYPE);
		}

		public static void RefreshCommonLang()
		{
			string name = typeof(EncryptForm).Name;
			Settings.smethod_78("EncryptType", EncryptForm.SZ_ENCRYPT_TYPE, name);
		}

		private void EncryptForm_Load(object sender, EventArgs e)
		{
			try
			{
				Settings.smethod_59(base.Controls);
				Settings.UpdateComponentTextsFromLanguageXmlData(this);
				this.method_2();
				this.DispData();
				LoadKeyStoreFromFile();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void EncryptForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			try
			{
				this.SaveData();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void btnAdd_Click(object sender, EventArgs e)
		{
			int num = 0;
			int num2 = 0;
			string text = null;
			for (num = 0; num < this.dgvKey.Rows.Count && num2 == (int)this.dgvKey.Rows[num].Tag; num++)
			{
				num2++;
			}
			this.dgvKey.Rows.Insert(num, 1);
			this.dgvKey.Rows[num].Tag = num2;
			// Generate default key for selected key length
			int[] hexLenMap = { 8, 16, 10, 32 };
			int hexLen = (this.cmbKeyLen.SelectedIndex < hexLenMap.Length) ? hexLenMap[this.cmbKeyLen.SelectedIndex] : 16;
			string baseKey = "53474C3953474C39"; // "SGL9SGL9" = 16 hex chars
			while (baseKey.Length < hexLen)
				baseKey += baseKey;
			text = baseKey.Substring(0, hexLen);
			this.dgvKey.Rows[num].Cells[0].Value = text;
			this.SaveData();
			this.method_3();
			((MainForm)base.MdiParent).RefreshRelatedForm(base.GetType());
		}

		private void btnDel_Click(object sender, EventArgs e)
		{
			int index = this.dgvKey.CurrentRow.Index;
			int keyIndex = (int)this.dgvKey.Rows[index].Tag;
			this.dgvKey.Rows.RemoveAt(index);
			ChannelForm.data.ClearByEncrypt(keyIndex);
			this.method_3();
			((MainForm)base.MdiParent).RefreshRelatedForm(base.GetType());
		}

		// ClosedGD77: Write encryption keys to radio via USB HID
		// Uses command byte 7 (MK22) or 8 (STM32) per firmware protocol.
		private void btnWriteRadio_Click(object sender, EventArgs e)
		{
			try
			{
				this.SaveData();
				SyncToKeyStore();
				int keyCount = KeyStore.Count;
				if (keyCount == 0)
				{
					MessageBox.Show("No keys configured. Add at least one key before writing to radio.");
					return;
				}

				DialogResult result = MessageBox.Show(
					string.Format("Write {0} encryption key(s) to radio?", keyCount),
					"Write Keys to Radio", MessageBoxButtons.YesNo);
				if (result != DialogResult.Yes) return;

				var outcome = WriteKeysToRadio();
				MessageBox.Show(outcome, "Write Keys Result");
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error: " + ex.Message);
			}
		}

		/// <summary>
		/// Sync the legacy EncryptForm.data into EncryptionKeyStore.
		/// The legacy store only holds 8-byte keys and a single global algorithm type.
		/// This is a best-effort migration — for full 32-byte keys, use the key store directly.
		/// </summary>
		private void SyncToKeyStore()
		{
			byte globalAlgo = (byte)this.cmbType.SelectedIndex;
			byte globalKeyLen = (byte)this.cmbKeyLen.SelectedIndex;

			// Map KeyLen enum index -> byte count
			byte[] keyLenMap = { 4, 8, 5, 16 }; // Length32=0->4, Length64=1->8, Length40=2->5, Length128=3->16
			byte keyBytes = (globalKeyLen < keyLenMap.Length) ? keyLenMap[globalKeyLen] : (byte)8;

			for (int i = 0; i < 16; i++)
			{
				if (EncryptForm.data[i])
				{
					EncryptionKeyEntry entry = EncryptionKeyEntry.CreateNew((byte)i);
					entry.algorithm = globalAlgo;
					entry.key_length = keyBytes;
					entry.IsActive = true;

					// Copy legacy key hex (up to 8 bytes) into expanded key storage
					string legacyHex = EncryptForm.data.GetKey(i);
					if (!string.IsNullOrEmpty(legacyHex))
					{
						entry.KeyHex = legacyHex;
						entry.key_length = (byte)(legacyHex.Length / 2);
					}
					KeyStore.SetKey(i, entry);
				}
			}
		}

		/// <summary>
		/// Write all active encryption keys from EncryptionKeyStore to the radio via USB HID.
		///
		/// Protocol (matches firmware handleCPSRequest -> cpsHandleCommand case 7/8):
		///   1. Open HID device (VID 0x15A2, PID 0x0073)
		///   2. Handshake: send PROGRA, receive ACK; send PROG2, receive info; send ACK
		///   3. For each key: send 53-byte 'C' command packet, wait for ACK
		///   4. Send ENDW to finalize
		///
		/// Packet format: ['C'][cmd=7|8][key_slot][algorithm][key_len][name(16)][key_data(32)]
		/// </summary>
		private static string GetKeyStorePath()
		{
			return System.IO.Path.Combine(Application.StartupPath, "encryption_keys.bin");
		}

		private static void SaveKeyStoreToFile()
		{
			try { KeyStore.SaveToFile(GetKeyStorePath()); }
			catch (Exception ex) { System.Diagnostics.Debug.WriteLine("SaveKeyStore: " + ex.Message); }
		}

		private static void LoadKeyStoreFromFile()
		{
			try { KeyStore.LoadFromFile(GetKeyStorePath()); }
			catch (Exception ex) { System.Diagnostics.Debug.WriteLine("LoadKeyStore: " + ex.Message); }
		}

		private string WriteKeysToRadio()
		{
			const int HID_VID = 0x15A2;
			const int HID_PID = 0x0073;
			byte[] CMD_PRG  = new byte[7] { 2, (byte)'P', (byte)'R', (byte)'O', (byte)'G', (byte)'R', (byte)'A' };
			byte[] CMD_PRG2 = new byte[2] { 77, 2 };
			byte[] CMD_ACK  = new byte[1] { 65 }; // 'A'
			byte[] CMD_ENDW = Encoding.ASCII.GetBytes("ENDW");

			HIDSpecifiedDevice device = null;
			try
			{
				device = HIDSpecifiedDevice.FindSpecifiedDevice(HID_VID, HID_PID);
				if (device == null)
				{
					return "Device not found. Connect radio and ensure it is in CPS communication mode.";
				}

				byte[] recvBuf = new byte[160];
				int keysWritten = 0;
				int keysFailed = 0;
				System.Text.StringBuilder errors = new System.Text.StringBuilder();

				byte cmdByte = GetKeyTransferCommand();

				// ---- Handshake: PROGRA ----
				Array.Clear(recvBuf, 0, recvBuf.Length);
				device.SendData(CMD_PRG);
				device.ReceiveData(recvBuf);
				if (recvBuf[0] != CMD_ACK[0])
				{
					device.Dispose();
					return "Handshake failed (PROGRA). Is radio in CPS mode?";
				}

				// ---- Handshake: PROG2 ----
				Array.Clear(recvBuf, 0, recvBuf.Length);
				device.SendData(CMD_PRG2);
				device.ReceiveData(recvBuf);
				// recvBuf[0..7] contains device info, we don't validate model for key transfer

				// ---- Handshake: ACK ----
				Array.Clear(recvBuf, 0, recvBuf.Length);
				device.SendData(CMD_ACK);
				device.ReceiveData(recvBuf);
				if (recvBuf[0] != CMD_ACK[0])
				{
					device.Dispose();
					return "Handshake failed (ACK).";
				}

				// ---- Send each key ----
				for (int slot = 0; slot < EncryptionKeyStore.MAX_SLOTS; slot++)
				{
					if (!KeyStore.Keys[slot].IsActive) continue;

					byte[] packet = KeyStore.BuildTransferPacket(slot, cmdByte);

					Array.Clear(recvBuf, 0, recvBuf.Length);
					device.SendData(packet);

					// Wait for ACK or timeout
					if (device.ReceiveData(recvBuf) && recvBuf[0] == CMD_ACK[0])
					{
						keysWritten++;
					}
					else if (recvBuf[0] != 0)
					{
						keysFailed++;
						string keyName = KeyStore.Keys[slot].Name;
						if (string.IsNullOrEmpty(keyName)) keyName = "Key " + (slot + 1);
						errors.AppendLine(keyName + ": unexpected response 0x" + recvBuf[0].ToString("X2"));
					}
					else
					{
						keysFailed++;
						string keyName = KeyStore.Keys[slot].Name;
						if (string.IsNullOrEmpty(keyName)) keyName = "Key " + (slot + 1);
						errors.AppendLine(keyName + ": no response (timeout)");
					}
				}

				// ---- Finalize ----
				Array.Clear(recvBuf, 0, recvBuf.Length);
				device.SendData(CMD_ENDW);
				device.ReceiveData(recvBuf);

				device.Dispose();
				device = null;

				if (keysFailed == 0)
				{
					return string.Format("Success: {0} key(s) written to radio.", keysWritten);
				}
				else
				{
					return string.Format("{0} written, {1} failed.\n{2}",
						keysWritten, keysFailed, errors.ToString());
				}
			}
			catch (TimeoutException ex)
			{
				return "Communication timeout: " + ex.Message;
			}
			catch (Exception ex)
			{
				return "Error: " + ex.Message;
			}
			finally
			{
				if (device != null) device.Dispose();
			}
		}

		private void method_3()
		{
			int selectedIndex = this.cmbType.SelectedIndex;
			int count = this.dgvKey.Rows.Count;
			this.btnAdd.Enabled = true;
			this.btnDel.Enabled = true;
			if (count == 16 || selectedIndex == 0)
			{
				this.btnAdd.Enabled = false;
			}
			if (count != 0 && selectedIndex != 0)
			{
				return;
			}
			this.btnDel.Enabled = false;
		}

		private void cmbKeyLen_SelectedIndexChanged(object sender, EventArgs e)
		{
			int num = 0;
			int selectedIndex = this.cmbKeyLen.SelectedIndex;
			if (this.method_0() != selectedIndex)
			{
				this.method_1(selectedIndex);
				// Key length hex char counts: 32-bit=8, 64-bit=16, 40-bit=10, 128-bit=32
				int[] hexLenMap = { 8, 16, 10, 32 };
				int maxHex = (selectedIndex < hexLenMap.Length) ? hexLenMap[selectedIndex] : 16;
				this.txtKey.MaxInputLength = maxHex;
				for (num = 0; num < this.dgvKey.Rows.Count; num++)
				{
					string text = this.dgvKey.Rows[num].Cells[0].Value as string;
					if (string.IsNullOrEmpty(text)) text = "";
					if (text.Length > maxHex)
					{
						text = text.Substring(0, maxHex);
					}
					else if (text.Length < maxHex)
					{
						text = text.PadRight(maxHex, '0');
					}
					this.dgvKey.Rows[num].Cells[0].Value = text;
				}
			}
		}

		private void cmbType_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (this.cmbType.SelectedIndex == 0)
			{
				this.cmbKeyLen.Enabled = false;
				this.dgvKey.Enabled = false;
				this.btnAdd.Enabled = false;
				this.btnDel.Enabled = false;
			}
			else
			{
				this.cmbKeyLen.Enabled = true;
				this.dgvKey.Enabled = true;
				this.btnAdd.Enabled = true;
				this.btnDel.Enabled = true;
				this.method_3();
			}
		}

		private void dgvKey_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
		{
			if (e.RowIndex >= this.dgvKey.FirstDisplayedScrollingRowIndex)
			{
				using (SolidBrush brush = new SolidBrush(this.dgvKey.RowHeadersDefaultCellStyle.ForeColor))
				{
					string s = (Convert.ToInt32(this.dgvKey.Rows[e.RowIndex].Tag) + 1).ToString();
					e.Graphics.DrawString(s, e.InheritedRowStyle.Font, brush, (float)(e.RowBounds.Location.X + 15), (float)(e.RowBounds.Location.Y + 5));
				}
			}
		}

		private void cmbKeyLen_Enter(object sender, EventArgs e)
		{
			this.method_1(this.cmbKeyLen.SelectedIndex);
		}

		private void dgvKey_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
		{
			if (e.Control is DataGridViewTextBoxEditingControl)
			{
				DataGridViewTextBoxEditingControl dataGridViewTextBoxEditingControl = (DataGridViewTextBoxEditingControl)e.Control;
				dataGridViewTextBoxEditingControl.KeyPress -= Settings.smethod_58;
				dataGridViewTextBoxEditingControl.KeyPress += Settings.smethod_58;
				dataGridViewTextBoxEditingControl.CharacterCasing = CharacterCasing.Upper;
			}
		}

		private void dgvKey_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
		{
			DataGridView dataGridView = (DataGridView)sender;
			if (string.IsNullOrEmpty(e.FormattedValue.ToString()))
			{
				e.Cancel = true;
				dataGridView.CancelEdit();
				dataGridView.EndEdit();
			}
		}

		private void dgvKey_CellValidated(object sender, DataGridViewCellEventArgs e)
		{
			DataGridView dataGridView = (DataGridView)sender;
			int maxInputLength = ((DataGridViewTextBoxColumn)this.dgvKey.Columns[e.ColumnIndex]).MaxInputLength;
			object value = dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
			if (value != null)
			{
				string text = value.ToString();
				if (text.Length < maxInputLength)
				{
					text = text.PadRight(maxInputLength, 'F');
					dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = text;
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
            /*
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}*/
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			this.lblType = new Label();
			this.cmbType = new ComboBox();
			this.lblKeyLen = new Label();
			this.cmbKeyLen = new ComboBox();
			this.dgvKey = new DataGridView();
			this.txtKey = new DataGridViewTextBoxColumn();
			this.btnDel = new Button();
			this.btnAdd = new Button();
			this.pnlEncrypt = new CustomPanel();
			((ISupportInitialize)this.dgvKey).BeginInit();
			this.pnlEncrypt.SuspendLayout();
			base.SuspendLayout();
			this.lblType.Location = new Point(45, 41);
			this.lblType.Name = "lblType";
			this.lblType.Size = new Size(109, 24);
			this.lblType.TabIndex = 0;
			this.lblType.Text = "Privacy Type";
			this.lblType.TextAlign = ContentAlignment.MiddleRight;
			this.cmbType.DropDownStyle = ComboBoxStyle.DropDownList;
			this.cmbType.FormattingEnabled = true;
			this.cmbType.Items.AddRange(new object[3]
			{
				"None",
				"ARC4",
				"AES-128"

			});
			this.cmbType.Location = new Point(168, 41);
			this.cmbType.Name = "cmbType";
			this.cmbType.Size = new Size(121, 24);
			this.cmbType.TabIndex = 1;
			this.cmbType.SelectedIndexChanged += new EventHandler(this.cmbType_SelectedIndexChanged);
			this.lblKeyLen.Location = new Point(45, 71);
			this.lblKeyLen.Name = "lblKeyLen";
			this.lblKeyLen.Size = new Size(109, 24);
			this.lblKeyLen.TabIndex = 2;
			this.lblKeyLen.Text = "Key Length";
			this.lblKeyLen.TextAlign = ContentAlignment.MiddleRight;
			this.cmbKeyLen.DropDownStyle = ComboBoxStyle.DropDownList;
			this.cmbKeyLen.FormattingEnabled = true;
			this.cmbKeyLen.Items.AddRange(new object[4]
			{
				"32",
				"64",
				"40",
				"128"
			});
			this.cmbKeyLen.Location = new Point(168, 71);
			this.cmbKeyLen.Name = "cmbKeyLen";
			this.cmbKeyLen.Size = new Size(121, 24);
			this.cmbKeyLen.TabIndex = 3;
			this.cmbKeyLen.SelectedIndexChanged += new EventHandler(this.cmbKeyLen_SelectedIndexChanged);
			this.cmbKeyLen.Enter += new EventHandler(this.cmbKeyLen_Enter);
			this.dgvKey.AllowUserToAddRows = false;
			this.dgvKey.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgvKey.Columns.AddRange(this.txtKey);
			this.dgvKey.Location = new Point(75, 145);
			this.dgvKey.Name = "dgvKey";
			this.dgvKey.RowTemplate.Height = 23;
			this.dgvKey.Size = new Size(240, 353);
			this.dgvKey.TabIndex = 6;
			this.dgvKey.CellValidated += new DataGridViewCellEventHandler(this.dgvKey_CellValidated);
			this.dgvKey.RowPostPaint += new DataGridViewRowPostPaintEventHandler(this.dgvKey_RowPostPaint);
			this.dgvKey.CellValidating += new DataGridViewCellValidatingEventHandler(this.dgvKey_CellValidating);
			this.dgvKey.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(this.dgvKey_EditingControlShowing);
			this.txtKey.HeaderText = "Key";
			this.txtKey.Name = "txtKey";
			this.btnDel.Location = new Point(220, 107);
			this.btnDel.Name = "btnDel";
			this.btnDel.Size = new Size(75, 23);
			this.btnDel.TabIndex = 5;
			this.btnDel.Text = "Delete";
			this.btnDel.UseVisualStyleBackColor = true;
			this.btnDel.Click += new EventHandler(this.btnDel_Click);
			this.btnAdd.Location = new Point(95, 107);
			this.btnAdd.Name = "btnAdd";
			this.btnAdd.Size = new Size(75, 23);
			this.btnAdd.TabIndex = 4;
			this.btnAdd.Text = "Add";
			this.btnAdd.UseVisualStyleBackColor = true;
			this.btnAdd.Click += new EventHandler(this.btnAdd_Click);
			this.btnWriteRadio = new Button();
			this.btnWriteRadio.Location = new Point(95, 504);
			this.btnWriteRadio.Name = "btnWriteRadio";
			this.btnWriteRadio.Size = new Size(200, 30);
			this.btnWriteRadio.TabIndex = 6;
			this.btnWriteRadio.Text = "Write Keys to Radio";
			this.btnWriteRadio.UseVisualStyleBackColor = true;
			this.btnWriteRadio.Click += new EventHandler(this.btnWriteRadio_Click);
			this.pnlEncrypt.AutoScroll = true;
			this.pnlEncrypt.AutoSize = true;
			this.pnlEncrypt.Controls.Add(this.cmbType);
			this.pnlEncrypt.Controls.Add(this.dgvKey);
			this.pnlEncrypt.Controls.Add(this.lblType);
			this.pnlEncrypt.Controls.Add(this.btnDel);
			this.pnlEncrypt.Controls.Add(this.lblKeyLen);
			this.pnlEncrypt.Controls.Add(this.btnAdd);
			this.pnlEncrypt.Controls.Add(this.btnWriteRadio);
			this.pnlEncrypt.Controls.Add(this.cmbKeyLen);
			this.pnlEncrypt.Dock = DockStyle.Fill;
			this.pnlEncrypt.Location = new Point(0, 0);
			this.pnlEncrypt.Name = "pnlEncrypt";
			this.pnlEncrypt.Size = new Size(390, 539);
			this.pnlEncrypt.TabIndex = 0;
			base.AutoScaleDimensions = new SizeF(7f, 16f);
//			base.AutoScaleMode = AutoScaleMode.Font;
			base.ClientSize = new Size(390, 539);
			base.Controls.Add(this.pnlEncrypt);
			this.Font = new Font("Arial", 10f, FontStyle.Regular, GraphicsUnit.Point, 0);
			base.Name = "EncryptForm";
			this.Text = "Privacy Setting";
			base.Load += new EventHandler(this.EncryptForm_Load);
			base.FormClosing += new FormClosingEventHandler(this.EncryptForm_FormClosing);
			((ISupportInitialize)this.dgvKey).EndInit();
			this.pnlEncrypt.ResumeLayout(false);
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		static EncryptForm()
		{

			EncryptForm.SZ_ENCRYPT_TYPE = new string[3]
			{
				"None",
				"ARC4",
				"AES-128"
			};
			EncryptForm.SZ_KEY_LEN = new string[4]
			{
				"32",
				"64",
				"40",
				"128"
			};
			EncryptForm.data = new Encrypt();
		}
	}
}
