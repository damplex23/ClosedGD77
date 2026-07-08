using System;
using System.Text;
using System.Globalization;

namespace DMR
{
	public class DMRDataItem : IEquatable<DMRDataItem>, IComparable<DMRDataItem>
	{
		public int DMRId { get; set; }
		public string DMRIdString { get; set; }
		public string Callsign { get; set; }
		public string Details { get; set; }

		static char[] DECOMPRESS_LUT = new char[64] { ' ', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '.' };
		static int[] COMPRESS_LUT = new int[256] { 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 0, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 63, 63, 63, 63, 63, 63, 63, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 63, 63, 63, 63, 63, 63, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63, 63 };



		// default construrctor creates an empty item
		public DMRDataItem()
		{
			Callsign = "";
			DMRId = 0;
		}


		// Create from a semicolon separated string from Hamdigital
		public DMRDataItem FromRadioidDotNet(string CSVLine)
		{
			string[] arr = CSVLine.Split(',');
			if (arr.Length < 3)
            {
				return null;
            }

			DMRIdString = arr[0];
			DMRId = Int32.Parse(arr[0]);
			Callsign = arr[1];
			Details = arr[2];



			if (arr.Length >= 5)
			{
				Details += " " + arr[4];
			}
            else
            {
				return this;
            }

			if (arr.Length >= 7)
			{
				Details += " " + arr[6];
			}

			for (int i = 0; i < Details.Length; i++)
			{
				if ((int)Details[i] >= 128)
				{
					Details = RemoveDiacritics(Details);
				}
			}
			return this;
		}


		static string RemoveDiacritics(string text)
		{
			var normalizedString = text.Normalize(NormalizationForm.FormD);
			var stringBuilder = new StringBuilder();

			foreach (var c in normalizedString)
			{
				var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
				if (unicodeCategory != UnicodeCategory.NonSpacingMark)
				{
					stringBuilder.Append(c);
				}
			}

			return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
		}
		private byte Int8ToBCD(int val)
		{
			int hi = val / 10;
			int lo = val % 10;
			return (byte)((hi * 16) + lo);
		}

		private byte BCDToByte(byte val)
		{
			int hi = val >> 4;
			int lo = val & 0x0F;
			return (byte)(hi * 10 + lo);
		}

		public static byte compressChar(char b)
        {
			if (b == ' ')
            {
				return 0;
            }

			if (b >= '0' && b <= '9')
            {
				return (byte)((int)b - (int)'0' + 1);
            }
			if (b>='a' && b <= 'z')
            {
				return (byte)((int)b - (int)'a' + 10 + 26 + 1);
			}
			if (b >= 'A' && b <= 'Z')
			{
				return (byte)((int)b - (int)'A' + 10 + 1);
			}
			return 63;
		}

		public static int compressSize(int fromSize)
        {
			return ((fromSize / 4) * 3) + (fromSize % 4);

		}

		// Convert to format to send to the radio (GD-77)
		public byte [] compress(string txtBuf)
        {
			for(int i=0;i< txtBuf.Length;i++)
            {
				if (txtBuf[i] > 'z')
                {
					Console.WriteLine((int)txtBuf[i] + " " + txtBuf[i]);
                }
            }
			byte[] compressedBuf = new byte[compressSize(txtBuf.Length)];

			int o = 0;
			int c = 0;
			do
			{
				//AAAAAABB
				//BBBBCCCC
				//CCDDDDDD

				//A << 2 | B >> 4
				//B << 4 | C >> 2
				//C << 6 | D
				compressedBuf[o + 0] =  (byte)(COMPRESS_LUT[(byte)txtBuf[c]] << 2);
				c++;

				if (c == txtBuf.Length)
                {
					break;
                }
				compressedBuf[o + 0] |= (byte)(COMPRESS_LUT[(byte)txtBuf[c]] >> 4);
				compressedBuf[o + 1] = (byte)(COMPRESS_LUT[(byte)txtBuf[c]] << 4);
				c++;
				if (c == txtBuf.Length)
				{
					break;
				}
				compressedBuf[o + 1] |= (byte)(COMPRESS_LUT[(byte)txtBuf[c]] >> 2);
				compressedBuf[o + 2] =  (byte)(COMPRESS_LUT[(byte)txtBuf[c]] << 6);
				c++;
				if (c == txtBuf.Length)
				{
					break;
				}
				compressedBuf[o + 2] |= (byte)COMPRESS_LUT[(byte)txtBuf[c]];
				c++;

				o += 3;

			} while (c < txtBuf.Length);

			

			return compressedBuf;
		}

		// Debugging and testing only
		public string decompress(byte []compressedBuf)
        {
			string os = "";
			byte cb1, cb2, cb3;
			int d = 0;
			do
			{
				cb1 = compressedBuf[d++];
				os += DECOMPRESS_LUT[cb1 >> 2];//A
				if (d== compressedBuf.Length)
                {
					break;
                }
				cb2 = compressedBuf[d++];
				os += DECOMPRESS_LUT[((cb1 & 0x03) << 4) + (cb2 >> 4)];//B
				if (d == compressedBuf.Length)
				{
					break;
				}
				cb3 = compressedBuf[d++];
				os += DECOMPRESS_LUT[((cb2 & 0x0F) << 2) + (cb3 >> 6)];//C
				os += DECOMPRESS_LUT[cb3 & 0x3F];//D

			} while (d < compressedBuf.Length);
			return os;
		}

		public byte[] getRadioData(int stringLength)
		{
			int DMR_ID_SIZE = 3;// 3 for native numbers 4 for BCD
			int dataSize = compressSize(stringLength) + DMR_ID_SIZE;// was stringLength + DMR_ID_SIZE;
			byte[] radioData = new byte[dataSize];
			if (DMRId != 0)
			{
				string txtBuf = Callsign + " " + Details;

				byte[] compressedBuf = compress(txtBuf.Substring(0, Math.Min(txtBuf.Length,stringLength)));

/*
 * debugging only
				string os = decompress(compressedBuf);
				Console.WriteLine(compressedBuf.Length + " |" + os + "|");
*/				

				Array.Copy(compressedBuf, 0, radioData, DMR_ID_SIZE, compressedBuf.Length);

				byte[] idBytes = BitConverter.GetBytes(DMRId);
				radioData[0] = idBytes[0];
				radioData[1] = idBytes[1];
				radioData[2] = idBytes[2];
			}
			return radioData; 
		}

		// Note. The 3 comparison functions appear with no references by Visual Studio
		// But this is a bug in Visual Studio as they are used when the list has to be sorted by in DMRIDForm.cs
		public int CompareTo(DMRDataItem comparePart)
		{
			// A null value means that this object is greater.
			if (comparePart == null)
			{
				return 1;
			}
			else
			{
				if (comparePart.DMRId < DMRId)
				{
					return 1;
				}
				else
				{
					if (comparePart.DMRId > DMRId)
					{
						return -1;
					}
					else
					{
						return 0;
					}
				}
			}
		}

		public bool Equals(DMRDataItem other)
		{
			if (other == null)
			{
				return false;
			}
			if (other == this)
			{
				return true;
			}
			return (this.DMRId == other.DMRId);
		}

		public override int GetHashCode()
		{
			//Get hash code for the Name field if it is not null. 
			return DMRId;
		}
#if false
// No longer used. 
// Prossibly incompatible with the latest changes

		// Ham digital service is no longer available
		public DMRDataItem FromHamDigital(string CSVLine)
		{
			string[] arr = CSVLine.Split(';');
			Callsign = arr[1];
			Name = arr[3];
			DMRId = Int32.Parse(arr[2]);
			try
			{
				AgeAsInt = Int32.Parse(arr[4]);// see if its a number (exception will trigger if not)
				AgeInDays = arr[4];// Only gets here is no exception triggered
			}
			catch (Exception)
			{

			}
			return this;
		}

		public object GetValue()
		{
			return AgeAsInt;
		}

		// Create from Data stored in the Codeplug
		public DMRDataItem(byte[] data, int stringLength)
		{
			Callsign = System.Text.Encoding.Default.GetString(data, 4, stringLength);
			DMRId = BitConverter.ToInt32(data, 0);
		}



		public byte[] getCodeplugData(int stringLength)
		{
			byte[] codeplugData = new byte[4+stringLength];
			if (DMRId != 0)
			{
				byte[] callsignbBuf = Encoding.UTF8.GetBytes(Callsign);
				Array.Copy(callsignbBuf, 0, codeplugData, 4, callsignbBuf.Length);
				Array.Copy(BitConverter.GetBytes(DMRId), 0, codeplugData, 0, 4);
			}
			return codeplugData; 
		}



#endif
		// Create from a semicolon separated string from Hamdigital
		public DMRDataItem FromRadio(byte[] record, int stringLength)
		{
			byte[] dmrid = new byte[4];
			Callsign = System.Text.Encoding.Default.GetString(record, 4, stringLength);

			Array.Copy(record, dmrid, 4);
			DMRId = 0;
			for (int i = 0; i < 4; i++)
			{
				DMRId *= 100;
				DMRId += BCDToByte(dmrid[3 - i]);
			}
			return this;
		}
	}
}
