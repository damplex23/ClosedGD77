using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Runtime.InteropServices;

namespace DMR
{
	public partial class DMRIDForm : Form
	{
		public static byte[] DMRIDBuffer = new byte[0x40000];

		static  List<DMRDataItem> DataList = null;
		private static byte[] SIG_PATTERN_BYTES;
		private WebClient _wc;
		private bool _isDownloading = false;
		//private int MAX_RECORDS = 10920;
		private int _stringLength = 8;
		const int HEADER_LENGTH = 12;
		int ID_NUMBER_SIZE = 3;

		private SerialPort _port = null;

		//private BackgroundWorker worker;

		public enum CommsDataMode { DataModeNone = 0, DataModeReadFlash = 1, DataModeReadEEPROM = 2, DataModeWriteFlash = 3, DataModeWriteEEPROM = 4 };
		public enum CommsAction { NONE, BACKUP_EEPROM, BACKUP_FLASH, RESTORE_EEPROM, RESTORE_FLASH, READ_CODEPLUG, WRITE_CODEPLUG }

		private SaveFileDialog _saveFileDialog = new SaveFileDialog();
		private OpenFileDialog _openFileDialog = new OpenFileDialog();

		private string _radioIdCSV = null;

		public static void ClearStaticData()
		{
			DMRIDBuffer = new byte[0x40000];
		}

		public DMRIDForm()
		{
			SIG_PATTERN_BYTES = new byte[] { (byte)'I', (byte)'D', (byte)'-', 0x56, 0x30, 0x30, 0x31, 0x00 };
			InitializeComponent();
			this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);// Roger Clark. Added correct icon on main form!
			cmbStringLen.Visible = false;
			lblEnhancedLength.Visible = false;

			txtRegionId.Text = (int.Parse(GeneralSetForm.data.RadioId) / 10000).ToString();

			DataList = new List<DMRDataItem>();

			//cmbStringLen.SelectedIndex = 2;


			dataGridView1.AutoGenerateColumns = false;
			DataGridViewCell cell = new DataGridViewTextBoxCell();
			DataGridViewTextBoxColumn colFileName = new DataGridViewTextBoxColumn()
			{
				CellTemplate = cell,
				Name = "Id", // internal name
				HeaderText = "ID",// Column header text
				DataPropertyName = "DMRID" // object property
			};
			dataGridView1.Columns.Add(colFileName);

			cell = new DataGridViewTextBoxCell();
			colFileName = new DataGridViewTextBoxColumn()
			{
				CellTemplate = cell,
				Name = "Call",// internal name
				HeaderText = "Callsign",// Column header text
				DataPropertyName = "Callsign"  // object property
			};
			dataGridView1.Columns.Add(colFileName);

			cell = new DataGridViewTextBoxCell();
			colFileName = new DataGridViewTextBoxColumn()
			{
				CellTemplate = cell,
				Name = "Details",// internal name
				HeaderText = "Details",// Column header text
				DataPropertyName = "Details",  // object property
				Width = 300,
			};
			dataGridView1.Columns.Add(colFileName);

			/*
			cell = new DataGridViewTextBoxCell();
			colFileName = new DataGridViewTextBoxColumn()
			{
				CellTemplate = cell,
				Name = "Age",// internal name
				HeaderText = "Last heard (days ago)",// Column header text
				DataPropertyName = "AgeInDays",  // object property
				Width = 140,
				ValueType = typeof(int),
				SortMode = DataGridViewColumnSortMode.Automatic
			};
			dataGridView1.Columns.Add(colFileName);
			*/
			dataGridView1.UserDeletedRow += new DataGridViewRowEventHandler(dataGridRowDeleted);

			rebindData();	

			cmbStringLen.SelectedIndex = 10;// Default to 16 characters
			cmbStringLen.Visible = true;
			lblEnhancedLength.Visible = true;
			cmbRadioType.SelectedIndex = 0;
			updateTotalNumberMessage();

			FormBorderStyle = FormBorderStyle.FixedSingle;
		
			string s = "";
			//string a = "";
			for (int i=0;i<256;i++)
            {
//				Console.WriteLine(DMRDataItem.compressChar((char)i));
				s += DMRDataItem.compressChar((char)i) + ",";
				//a += (char)i + ",";
			}
			//Console.WriteLine(a);
			//Console.WriteLine(s);
			
		}

		private void dataGridRowDeleted(object sender, DataGridViewRowEventArgs e)
		{
			updateTotalNumberMessage();
		}

		private void rebindData()
		{
			BindingList<DMRDataItem> bindingList = new BindingList<DMRDataItem>(DataList);
			var source = new BindingSource(bindingList, null);
			dataGridView1.DataSource = source;

			updateTotalNumberMessage();
		}
		/*
				private void btnDownload_Click(object sender, EventArgs e)
				{
					if (DataList == null || _isDownloading)
					{
						return;
					}

					_wc = new WebClient();
					try
					{
						lblMessage.Text = Settings.dicCommon["DownloadContactsDownloading"];
						Cursor.Current = Cursors.WaitCursor;
						this.Refresh();
						Application.DoEvents();
						_wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(downloadFromHamDigitalCompleteHandler);
						_wc.DownloadStringAsync(new Uri("http://ham-digital.org/user_by_lh.php?id=" + txtRegionId.Text));

					}
					catch (Exception )
					{
						Cursor.Current = Cursors.Default;
						MessageBox.Show(Settings.dicCommon["UnableDownloadFromInternet"]);
						return;
					}
					_isDownloading = true;

				}

						private void downloadFromHamDigitalCompleteHandler(object sender, DownloadStringCompletedEventArgs e )
				{
					string ownRadioId = GeneralSetForm.data.RadioId;
					string csv;// = e.Result;
					int maxAge = Int32.MaxValue;


					try
					{
						csv = e.Result;
					}
					catch(Exception)
					{
						MessageBox.Show(Settings.dicCommon["UnableDownloadFromInternet"]);
						return;
					}

					try
					{
						maxAge = Int32.Parse(this.txtAgeMaxDays.Text);
					}
					catch(Exception)
					{

					}

					try
					{
						bool first = true;
						foreach (var csvLine in csv.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
						{
							if (first)
							{
								first = false;
								continue;
							}
							DMRDataItem item = (new DMRDataItem()).FromHamDigital(csvLine);
							if (item.AgeAsInt <= maxAge)
							{
								DataList.Add(item);
							}
						}
						DataList = DataList.Distinct().ToList();

						rebindData();
						Cursor.Current = Cursors.Default;

					}
					catch (Exception)
					{
						MessageBox.Show(Settings.dicCommon["ErrorParsingData"]);
					}
					finally
					{
						_wc = null;
						_isDownloading = false;
						Cursor.Current = Cursors.Default;
					}
				}
		*/
		private void downloadFromRadioIdCompleteHandler(object sender, DownloadStringCompletedEventArgs e)
		{
			//string ownRadioId = GeneralSetForm.data.RadioId;

			try
			{
				_radioIdCSV = e.Result;
			}
			catch (Exception)
			{
				MessageBox.Show(Settings.dicCommon["UnableDownloadFromInternet"]);
				return;
			}

			importFromRadioIdCSV();

			_wc = null;
			_isDownloading = false;
			Cursor.Current = Cursors.Default;
		}

		private void importFromRadioIdCSV()
        {
			if (_radioIdCSV == null)
            {
				return;
            }
			try
			{
				bool first = true;
				foreach (var csvLine in _radioIdCSV.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries))
				{
					if (first)
					{
						first = false;
						continue;
					}
					if (csvLine.Length > 0)
					{
						if (csvLine.IndexOf(txtRegionId.Text) == 0)
						{ 
							DMRDataItem item = (new DMRDataItem()).FromRadioidDotNet(csvLine);
							if (item != null)
							{
								DataList.Add(item);
							}
						}
					}
				}
				DataList = DataList.Distinct().ToList();

				rebindData();
				Cursor.Current = Cursors.Default;
			}
			catch (Exception)
			{
				MessageBox.Show(Settings.dicCommon["ErrorParsingData"]);
			}
		}



		private void updateTotalNumberMessage()
		{
			string message = Settings.dicCommon["DMRIdContcatsTotal"];// "Total number of IDs = {0}. Max of MAX_RECORDS can be uploaded";
			lblMessage.Text = string.Format(message, DataList.Count, MAX_RECORDS);
		}

		private void downloadProgressHandler(object sender, DownloadProgressChangedEventArgs e)
		{
			try
			{
				BeginInvoke((Action)(() =>
				{
					lblMessage.Text = Settings.dicCommon["DownloadContactsDownloading"] + e.ProgressPercentage + "%";
				}));
			}
			catch (Exception)
			{
				// No nothing
			}
		}


		private int MAX_RECORDS
		{
			get
			{
				int[] memorySizes =  { 0x88000, 0x88000 + 0x100000, 0x88000, 0x88000 + 0x700000 };
				return (memorySizes[Math.Max(0,cmbRadioType.SelectedIndex)] - HEADER_LENGTH + (chkUseVPMemory.Checked?0x28C00:0)) / (DMRDataItem.compressSize(_stringLength) + ID_NUMBER_SIZE);
			}
		}

		private byte[] GenerateUploadData(UInt32 flashMemoryId)
		{
			int dmrIdMemorySize;

			switch(flashMemoryId)
            {
				case 0x4015:// 4015 25Q16 16M bits 2M bytes, used in the Baofeng DM-1801 ?
					dmrIdMemorySize = 0x88000 + 0x100000;// 1M
					break;
				case 0x4017:    // 4017 25Q64 64M bits. Used in Roger's special GD-77 radios modified on the TYT production line
					dmrIdMemorySize = 0x700000;//7M
					break;
				case 0x4014:// 4014 25Q80 8M bits 2M bytes, used in the GD-77
				// fallthrough
				default:
					dmrIdMemorySize = 0x88000;//544k
					break;
			}

			dmrIdMemorySize += (chkUseVPMemory.Checked ? 0x28C00 : 0);

			int recordSize = DMRDataItem.compressSize(_stringLength) + ID_NUMBER_SIZE;// _stringLength + ID_NUMBER_SIZE

			int maxRecords =  (dmrIdMemorySize - HEADER_LENGTH) / (recordSize);
			int numRecords = Math.Min(DataList.Count, maxRecords);
			int dataSize = numRecords * (recordSize) + HEADER_LENGTH;
			dataSize = ((dataSize / 32)+1) * 32;
			byte[] buffer = new byte[dataSize];

			Array.Copy(SIG_PATTERN_BYTES, buffer, SIG_PATTERN_BYTES.Length);


			Array.Copy(BitConverter.GetBytes(numRecords), 0, buffer, 8, 4);

			if (DataList == null)
			{
				return buffer;
			}
			List<DMRDataItem> uploadList = new List<DMRDataItem>(DataList);// Need to make a copy so we can sort it and not screw up the list in the dataGridView
			uploadList.Sort();



			for (int i = 0; i < numRecords; i++)
			{
				Array.Copy(uploadList[i].getRadioData(_stringLength), 0, buffer, HEADER_LENGTH + i * (recordSize), (recordSize));
			}
			return buffer;
		}


		private void btnClear_Click(object sender, EventArgs e)
		{
			DataList = new List<DMRDataItem>();
			rebindData();
			//DataToCodeplug();

		}

		private void btnWriteToGD77_Click(object sender, EventArgs e)
		{
			if (SetupDiWrap.ComPortNameFromFriendlyNamePrefix("OpenGD77") != null)
			{
				writeToOpenGD77();
			}
			else
			{
				MessageBox.Show("Please connect the GD-77 running OpenGD77 firmware, and try again.", "OpenGD77 radio not detected.");
			}
		}

		private void DMRIDFormNew_FormClosing(object sender, FormClosingEventArgs e)
		{
		}

		private void DMRIDForm_Load(object sender, EventArgs e)
		{
			Settings.smethod_59(base.Controls);
			Settings.UpdateComponentTextsFromLanguageXmlData(this);// Update texts etc from language xml file
		}

		private void cmbStringLen_SelectedIndexChanged(object sender, EventArgs e)
		{
			_stringLength = cmbStringLen.SelectedIndex + 6;
			updateTotalNumberMessage();
		}

		#region OpenGD77
		bool flashWritePrepareSector(int address, ref byte[] sendbuffer, ref byte[] readbuffer, OpenGD77CommsTransferData dataObj)
		{
			dataObj.data_sector = address / 4096;

			sendbuffer[0] = (byte)'W';
			sendbuffer[1] = 1;
			sendbuffer[2] = (byte)((dataObj.data_sector >> 16) & 0xFF);
			sendbuffer[3] = (byte)((dataObj.data_sector >> 8) & 0xFF);
			sendbuffer[4] = (byte)((dataObj.data_sector >> 0) & 0xFF);
			_port.Write(sendbuffer, 0, 5);
			while (_port.BytesToRead == 0)
			{
				Thread.Sleep(0);
			}
			_port.Read(readbuffer, 0, 64);

			return ((readbuffer[0] == sendbuffer[0]) && (readbuffer[1] == sendbuffer[1]));
		}

		bool flashSendData(int address, int len, ref byte[] sendbuffer, ref byte[] readbuffer)
		{
			sendbuffer[0] = (byte)'W';
			sendbuffer[1] = 2;
			sendbuffer[2] = (byte)((address >> 24) & 0xFF);
			sendbuffer[3] = (byte)((address >> 16) & 0xFF);
			sendbuffer[4] = (byte)((address >> 8) & 0xFF);
			sendbuffer[5] = (byte)((address >> 0) & 0xFF);
			sendbuffer[6] = (byte)((len >> 8) & 0xFF);
			sendbuffer[7] = (byte)((len >> 0) & 0xFF);
			_port.Write(sendbuffer, 0, len + 8);
			while (_port.BytesToRead == 0)
			{
				Thread.Sleep(0);
			}
			_port.Read(readbuffer, 0, 64);

			return ((readbuffer[0] == sendbuffer[0]) && (readbuffer[1] == sendbuffer[1]));
		}

		bool flashWriteSector(ref byte[] sendbuffer, ref byte[] readbuffer, OpenGD77CommsTransferData dataObj)
		{
			dataObj.data_sector = -1;

			sendbuffer[0] = (byte)'W';
			sendbuffer[1] = 3;
			_port.Write(sendbuffer, 0, 2);
			while (_port.BytesToRead == 0)
			{
				Thread.Sleep(0);
			}
			_port.Read(readbuffer, 0, 64);

			return ((readbuffer[0] == sendbuffer[0]) && (readbuffer[1] == sendbuffer[1]));
		}

		private void close_data_mode()
		{
			//data_mode = OpenGD77CommsTransferData.CommsDataMode.DataModeNone;
		}


		private bool ReadRadioInfo(OpenGD77CommsTransferData dataObj)
		{

			byte[] sendbuffer = new byte[512];
			byte[] readbuffer = new byte[512];
			int currentDataAddressInLocalBuffer = 0;


			sendbuffer[0] = (byte)'R';
			sendbuffer[1] = (byte)dataObj.mode;
			sendbuffer[2] = (byte)(0);
			sendbuffer[3] = (byte)(0);
			sendbuffer[4] = (byte)(0);
			sendbuffer[5] = (byte)(0);
			sendbuffer[6] = (byte)(0);
			sendbuffer[7] = (byte)(0);


			_port.Write(sendbuffer, 0, 8);
			while (_port.BytesToRead == 0)
			{
				Thread.Sleep(0);
			}
			_port.Read(readbuffer, 0, 64);

			if (readbuffer[0] == 'R')
			{
				int len = (readbuffer[1] << 8) + (readbuffer[2] << 0);
				for (int i = 0; i < len; i++)
				{
					dataObj.dataBuff[currentDataAddressInLocalBuffer++] = readbuffer[i + 3];
				}
			}
			else
			{
				Console.WriteLine(String.Format("read stopped (error at {0:X8})", 0));
				close_data_mode();
				return false;

			}

			close_data_mode();
			return true;
		}

		private void ReadFlashOrEEPROM(OpenGD77CommsTransferData dataObj)
		{
			int old_progress = 0;
			byte[] sendbuffer = new byte[512];
			byte[] readbuffer = new byte[512];
			byte[] com_Buf = new byte[256];

			int currentDataAddressInTheRadio = dataObj.startDataAddressInTheRadio;
			int currentDataAddressInLocalBuffer = dataObj.localDataBufferStartPosition;

			int size = (dataObj.startDataAddressInTheRadio + dataObj.transferLength) - currentDataAddressInTheRadio;

			while (size > 0)
			{
				if (size > 32)
				{
					size = 32;
				}

				sendbuffer[0] = (byte)'R';
				sendbuffer[1] = (byte)dataObj.mode;
				sendbuffer[2] = (byte)((currentDataAddressInTheRadio >> 24) & 0xFF);
				sendbuffer[3] = (byte)((currentDataAddressInTheRadio >> 16) & 0xFF);
				sendbuffer[4] = (byte)((currentDataAddressInTheRadio >> 8) & 0xFF);
				sendbuffer[5] = (byte)((currentDataAddressInTheRadio >> 0) & 0xFF);
				sendbuffer[6] = (byte)((size >> 8) & 0xFF);
				sendbuffer[7] = (byte)((size >> 0) & 0xFF);
				_port.Write(sendbuffer, 0, 8);
				while (_port.BytesToRead == 0)
				{
					Thread.Sleep(0);
				}
				_port.Read(readbuffer, 0, 64);

				if (readbuffer[0] == 'R')
				{
					int len = (readbuffer[1] << 8) + (readbuffer[2] << 0);
					for (int i = 0; i < len; i++)
					{
						dataObj.dataBuff[currentDataAddressInLocalBuffer++] = readbuffer[i + 3];
					}

					int progress = (currentDataAddressInTheRadio - dataObj.startDataAddressInTheRadio) * 100 / dataObj.transferLength;
					if (old_progress != progress)
					{
						updateProgess(progress);
						old_progress = progress;
					}

					currentDataAddressInTheRadio = currentDataAddressInTheRadio + len;
				}
				else
				{
					Console.WriteLine(String.Format("read stopped (error at {0:X8})", currentDataAddressInTheRadio));
					close_data_mode();

				}
				size = (dataObj.startDataAddressInTheRadio + dataObj.transferLength) - currentDataAddressInTheRadio;
			}
			close_data_mode();
		}

		private void WriteFlash(OpenGD77CommsTransferData dataObj)
		{
			int old_progress = 0;
			byte[] sendbuffer = new byte[512];
			byte[] readbuffer = new byte[512];
			byte[] com_Buf = new byte[256];
			int currentDataAddressInTheRadio = dataObj.startDataAddressInTheRadio;

			int currentDataAddressInLocalBuffer = dataObj.localDataBufferStartPosition;
			dataObj.data_sector = -1;// Always needs to be initialised to -1 so the first flashWritePrepareSector is called

			int size = (dataObj.startDataAddressInTheRadio + dataObj.transferLength) - currentDataAddressInTheRadio;
			while (size > 0)
			{
				if (size > 32)
				{
					size = 32;
				}

				if (dataObj.data_sector == -1)
				{
					if (!flashWritePrepareSector(currentDataAddressInTheRadio, ref sendbuffer, ref readbuffer, dataObj))
					{
						close_data_mode();
						break;
					};
				}

				if (dataObj.mode != 0)
				{
					int len = 0;
					for (int i = 0; i < size; i++)
					{
						sendbuffer[i + 8] = dataObj.dataBuff[currentDataAddressInLocalBuffer++];
						len++;

						if (dataObj.data_sector != ((currentDataAddressInTheRadio + len) / 4096))
						{
							break;
						}
					}
					if (flashSendData(currentDataAddressInTheRadio, len, ref sendbuffer, ref readbuffer))
					{
						int progress = (currentDataAddressInTheRadio - dataObj.startDataAddressInTheRadio) * 100 / dataObj.transferLength;
						if (old_progress != progress)
						{
							updateProgess(progress);
							old_progress = progress;
						}

						currentDataAddressInTheRadio = currentDataAddressInTheRadio + len;

						if (dataObj.data_sector != (currentDataAddressInTheRadio / 4096))
						{
							if (!flashWriteSector(ref sendbuffer, ref readbuffer, dataObj))
							{
								close_data_mode();
								break;
							};
						}
					}
					else
					{
						close_data_mode();
						break;
					}
				}
				size = (dataObj.startDataAddressInTheRadio + dataObj.transferLength) - currentDataAddressInTheRadio;
			}

			if (dataObj.data_sector != -1)
			{
				if (!flashWriteSector(ref sendbuffer, ref readbuffer, dataObj))
				{
					Console.WriteLine(String.Format("Error. Write stopped (write sector error at {0:X8})", currentDataAddressInTheRadio));
				};
			}

			close_data_mode();
		}

		void updateProgess(int progressPercentage)
		{
			if (progressBar1.InvokeRequired)
				progressBar1.Invoke(new MethodInvoker(delegate()
				{
					progressBar1.Value = progressPercentage;
				}));
			else
			{
				progressBar1.Value = progressPercentage;
			}
		}
		#endregion

		bool sendCommand(int commandNumber, int x_or_command_option_number = 0, int y = 0, int iSize = 0, int alignment = 0, int isInverted = 0, string message = "")
		{
			byte[] buffer = new byte[64];
			buffer[0] = (byte)'C';
			buffer[1] = (byte)commandNumber;

			switch (commandNumber)
			{
				case 2:
					buffer[3] = (byte)y;
					buffer[4] = (byte)iSize;
					buffer[5] = (byte)alignment;
					buffer[6] = (byte)isInverted;
					Buffer.BlockCopy(Encoding.ASCII.GetBytes(message), 0, buffer, 7, Math.Min(message.Length, 16));// copy the string into bytes 7 onwards
					break;
				case 6:
					// Special command
					buffer[2] = (byte)x_or_command_option_number;
					break;
				default:

					break;

			}
			_port.Write(buffer, 0, 32);
			while (_port.BytesToRead == 0)
			{
				Thread.Sleep(0);
			}
			_port.Read(buffer, 0, 64);

			return ((buffer[1] == commandNumber));
		}



		private void writeToOpenGD77()
		{
			String gd77CommPort = SetupDiWrap.ComPortNameFromFriendlyNamePrefix("OpenGD77");
			int radioMemoryAddress = 0x30000;
			try
			{
				_port = new SerialPort(gd77CommPort, 115200, Parity.None, 8, StopBits.One);
				_port.ReadTimeout = 1000;
				_port.Open();
			}
			catch (Exception)
			{
				_port = null;
				MessageBox.Show("Failed to open comm port", "Error");
				return;
			}

			RadioInfo radioInfo = readOpenGD77RadioInfo();

			if (radioInfo.buildDateTime != null)
			{
				// Commands to control the screen etc in the firmware
				sendCommand(0);
				sendCommand(1);
				sendCommand(2, 0, 0, 3, 1, 0, "CPS");
				sendCommand(2, 0, 16, 3, 1, 0, "Writing");
				sendCommand(2, 0, 32, 3, 1, 0, "DMRID");
				sendCommand(2, 0, 48, 3, 1, 0, "Database");
				sendCommand(3);
				sendCommand(6, 4);// flash red LED

				OpenGD77CommsTransferData dataObj = new OpenGD77CommsTransferData(OpenGD77CommsTransferData.CommsAction.NONE);
				dataObj.mode = OpenGD77CommsTransferData.CommsDataMode.DataModeWriteFlash;

				if (ID_NUMBER_SIZE == 3)
				{
					if (chkUseVPMemory.Checked)
					{
						SIG_PATTERN_BYTES[2] = (byte)'n';// signal use VP memory to the firmware
					}
					else
					{
						SIG_PATTERN_BYTES[2] = (byte)'N';
					}
				}

				int recordLength = DMRDataItem.compressSize(_stringLength) + ID_NUMBER_SIZE;

				SIG_PATTERN_BYTES[3] = (byte)(0x4a + recordLength);

				dataObj.dataBuff = GenerateUploadData(radioInfo.flashId);
				//File.WriteAllBytes("d:\\dmrid_db_NEW.bin", dataObj.dataBuff);// Write for debugging purposes


				int localBufferPosition = 0;

				dataObj.localDataBufferStartPosition = localBufferPosition;
				dataObj.startDataAddressInTheRadio = radioMemoryAddress;

				int totalTransferSize = (dataObj.dataBuff.Length / 32) * 32;

				int splitPoint = HEADER_LENGTH + (recordLength * ((0x40000 - HEADER_LENGTH) / recordLength));

				if (totalTransferSize > splitPoint)
				{
					dataObj.transferLength = splitPoint;
				}
				else
				{
					dataObj.transferLength = totalTransferSize;
				}
				WriteFlash(dataObj);// transfer the first data section

				totalTransferSize -= dataObj.transferLength;
				localBufferPosition += dataObj.transferLength;

				if (totalTransferSize > 0)
				{
					dataObj.startDataAddressInTheRadio = (chkUseVPMemory.Checked ? 0x8F400 : 0xB8000);
					dataObj.localDataBufferStartPosition = localBufferPosition;// continue on from last transfer length
					dataObj.transferLength = totalTransferSize;
					WriteFlash(dataObj);
				}

				progressBar1.Value = 0;
			}
			sendCommand(5);
			if (_port != null)
			{
				try
				{
					_port.Close();
				}
				catch (Exception)
				{
					MessageBox.Show("Failed to close OpenGD77 comm port", "Warning");
				}
			}
			if (radioInfo.buildDateTime == null)
            {
				MessageBox.Show("Incompatible firmware. Please update the firmware in your radio", "Error");
			}

		}

		private void dataGridView1_SortCompare(object sender, DataGridViewSortCompareEventArgs e)
		{
			if (e.Column.Index != 3)
			{
				return;
			}
			try
			{
				if (Int32.Parse(e.CellValue1.ToString()) < Int32.Parse(e.CellValue2.ToString()))
				{
					e.SortResult = -1;
				}
				else
				{
					e.SortResult = 1;
				}
				e.Handled = true;
			}
			catch (Exception)
			{
			}

		}

		private void btnImportCSV_Click(object sender, EventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "firmware files|*.csv";

			if (openFileDialog.ShowDialog() == DialogResult.OK && openFileDialog.FileName != null)
			{
				try
				{

					bool first = true;
					String region = txtRegionId.Text;
					using (var reader = new StreamReader(openFileDialog.FileName))
					{
						while (!reader.EndOfStream)
						{
							string csvLine = reader.ReadLine();
							if (first)
							{
								first = false;
								continue;
							}
							if (csvLine.Length > 0)
							{
								if (csvLine.IndexOf(txtRegionId.Text) == 0)
								{
									DMRDataItem item = (new DMRDataItem()).FromRadioidDotNet(csvLine);
									if (item != null)
									{
										DataList.Add(item);
									}
								}
							}
						}
						DataList = DataList.Distinct().ToList();

						rebindData();
						Cursor.Current = Cursors.Default;

					}
				}
				catch (Exception)
				{
					MessageBox.Show("The CSV file could not be opened");
				}
			}
		}

		private void btnDownloadFromRadioId_Click(object sender, EventArgs e)
		{
			if (DataList == null || _isDownloading)
			{
				return;
			}

			if (_radioIdCSV != null)
            {
				importFromRadioIdCSV();
				return;
			}

			_wc = new WebClient();
			try
			{
				lblMessage.Text = Settings.dicCommon["DownloadContactsDownloading"];
				Cursor.Current = Cursors.WaitCursor;
				this.Refresh();
				Application.DoEvents();
				ServicePointManager.Expect100Continue = true;
				// If you have .Net 4.5
				//ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
				// otherwise
				ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
				_wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(downloadFromRadioIdCompleteHandler);
				_wc.DownloadStringAsync(new Uri("https://database.radioid.net/static/user.csv"));

			}
			catch (Exception)
			{
				Cursor.Current = Cursors.Default;
				MessageBox.Show(Settings.dicCommon["UnableDownloadFromInternet"]);
				return;
			}
			_isDownloading = true;
		}


		private RadioInfo readOpenGD77RadioInfo()
		{

			/*
			String gd77CommPort = SetupDiWrap.ComPortNameFromFriendlyNamePrefix("OpenGD77");
			try
			{
				_port = new SerialPort(gd77CommPort, 115200, Parity.None, 8, StopBits.One);
				_port.ReadTimeout = 1000;
				_port.Open();
			}
			catch (Exception)
			{
				_port = null;
				MessageBox.Show("Failed to open comm port", "Error");
				return;
			}*/


			sendCommand(0);
			sendCommand(1);
			sendCommand(2, 0, 0, 3, 1, 0, "CPS");
			sendCommand(2, 0, 16, 3, 1, 0, "Read");
			sendCommand(2, 0, 32, 3, 1, 0, "Radio");
			sendCommand(2, 0, 48, 3, 1, 0, "Info");
			sendCommand(3);
			sendCommand(6, 4);// flash red LED

			OpenGD77CommsTransferData dataObjRead = new OpenGD77CommsTransferData(OpenGD77CommsTransferData.CommsAction.NONE);
			dataObjRead.mode = OpenGD77CommsTransferData.CommsDataMode.DataModeReadRadioInfo;
			dataObjRead.localDataBufferStartPosition = 0;
			dataObjRead.transferLength = 0;
			dataObjRead.dataBuff = new byte[128];

			RadioInfo radioInfo = new RadioInfo();
			if (ReadRadioInfo(dataObjRead))
			{
				radioInfo = ByteArrayToRadioInfo(dataObjRead.dataBuff);
			}

			sendCommand(5);
			/*
			if (_port != null)
			{
				try
				{
					_port.Close();
				}
				catch (Exception)
				{
					MessageBox.Show("Failed to close OpenGD77 comm port", "Warning");
				}
			}
			*/
			return radioInfo;
		}

		RadioInfo ByteArrayToRadioInfo(byte[] bytes)
		{
			RadioInfo radioInfo;
			GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
			try
			{
				radioInfo = (RadioInfo)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(RadioInfo));
			}
			finally
			{
				handle.Free();
			}
			return radioInfo;
		}

		[StructLayout(LayoutKind.Explicit, Size = 38, Pack = 1)]
		public struct RadioInfo
		{
			[MarshalAs(UnmanagedType.U4)]
			[FieldOffset(0)]
			public UInt32 structVersion;

			[MarshalAs(UnmanagedType.U4)]
			[FieldOffset(4)]
			public UInt32 radioType;
			
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
			[FieldOffset(8)]
			public string gitRevision;
			
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
			[FieldOffset(24)]
			public string buildDateTime;

			[MarshalAs(UnmanagedType.U4)]
			[FieldOffset(40)]
			public UInt32 flashId;
		}

        private void cmbRadioType_SelectedIndexChanged(object sender, EventArgs e)
        {
			updateTotalNumberMessage();
		}

        private void chkUseVPMemory_CheckedChanged(object sender, EventArgs e)
        {
			updateTotalNumberMessage();
		}
    }


}

