using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NerdDinner.Events {
	public class RSVPCanceled : IEventData{
		public string Name { get; set; }
		public int DinnerId { get; set; }
	}
}
