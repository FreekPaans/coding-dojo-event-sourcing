using System;
using NerdDinner.Events;
using Newtonsoft.Json;

namespace NerdDinner.Models {

    public class Event {

        public int            Id          { get; set; }
        public Guid           AggregateId { get; set; }
        public string         Data        { get; set; }

        public string EventType { get; set; }

        public DateTimeOffset DateTime    { get; set; }
        public int AggregateEventSequence { get; set; }

        public dynamic AddEventType() {
            //SOMETHING MAGICAL
            var type = Type.GetType(EventType);
            dynamic data = JsonConvert.DeserializeObject(Data, type);

            var eventType = typeof(Event<>).MakeGenericType(type);

            dynamic instance = Activator.CreateInstance(eventType);

            instance.Data = data;

            foreach (var prop in typeof(Event).GetProperties()) {
                prop.SetValue(instance, prop.GetValue(this, null), null);
            }

            return instance;
        }

		public static Event Make(IEventData eventData, Guid aggregateId, int eventSequence) {
            return new Event {
                AggregateId = aggregateId, 
                AggregateEventSequence = eventSequence, 
                DateTime = System.DateTime.UtcNow, 
                EventType = eventData.GetType().FullName, 
                Data = JsonConvert.SerializeObject(eventData)
            };
        }
    }  

    public class Event<T> : Event where T : IEventData {
        public new T Data { get; set;}
    }

}