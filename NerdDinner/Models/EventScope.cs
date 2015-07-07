using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NerdDinner.Models {
	public class EventScope {
		[ThreadStatic]
		static List<Event> _publishedEvents;
		public static ICollection<Event> Start(Action code) {
			if(_publishedEvents!=null) {
				throw new Exception("Cannot nest event scope");
			}
			_publishedEvents = new List<Event>();
			try {
				code();
				return _publishedEvents.ToList();
			}
			finally {
				_publishedEvents = null;
			}
		}



		internal static void Raise(Event e) {
			if(_publishedEvents == null) {
				throw new Exception("No opened scope");
			}
			_publishedEvents.Add(e);
		}
	}
}
