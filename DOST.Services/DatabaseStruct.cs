using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOST.Services {
	public class DatabaseStruct {
		public struct SSQLRow {
			public Dictionary<string, object> Columns { get; set; }
			public SSQLRow(Dictionary<string, object> columns) {
				this.Columns = columns;
			}
		}
	}
}
