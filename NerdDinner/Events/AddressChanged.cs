using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NerdDinner.Events {
	public class AddressChanged : IEventData{
		public string NewAddress { get; set; }

		public string Reason { get; set; }
	}
}