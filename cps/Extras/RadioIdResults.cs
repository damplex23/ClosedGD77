using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DMR
{
	public class RadioIdResults
	{
		public int count { get; set; }
		public List<RadioIdDataItem> results { get; set; }
	}
	public class RadioIdDataItem
	{
		public string callsign { get; set; }
		public string city { get; set; }
		public string country { get; set; }
		public string fname { get; set; }
		public string id { get; set; }
		public string remarks { get; set; }
		public string state { get; set; }
		public string surname { get; set; }
	}
}
