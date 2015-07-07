using NerdDinner.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NerdDinner.Models {
	public class PopularDinner {
		 [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int DinnerID { get; set; }
        public string Title { get; set; }
        public DateTime EventDate { get; set; }
        public string Description { get; set; }
        public string HostedBy { get; set; }
        public string ContactPhone { get; set; }
        public string Address { get; set; }
        public string Country { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string HostedById { get; set; }

        public int RSVPCount { get; set; }

        public static void Handle(NerdDinners context, Event @event) {
            if(Type.GetType(@event.EventType) ==  typeof(DinnerCreated)) {
                Handle(context,(Event<DinnerCreated>)@event.AddEventType());
            }
            if(Type.GetType(@event.EventType) ==  typeof(RSVPed)) {
                Handle(context,(Event<RSVPed>)@event.AddEventType());
            }
        }

        private static void Handle(NerdDinners context,Event<RSVPed> @event) {
            var popular = context.PopularDinners.Find(@event.Data.DinnerId);
            if(popular == null) {
                throw new InvalidOperationException("Couldn't find dinner with id "+@event.Data.DinnerId);
            }

            popular.RSVPCount++;
        }

        private static void Handle(NerdDinners context,Event<DinnerCreated> @event) {
            var popular = new PopularDinner {
                Address = @event.Data.Address,
                ContactPhone = @event.Data.ContactPhone,
                Country = @event.Data.Country,
                Description = @event.Data.Description,
                DinnerID = @event.Data.DinnerID,
                EventDate = @event.Data.EventDate,
                HostedBy = @event.Data.HostedBy,
                HostedById = @event.Data.HostedById,
                Latitude = @event.Data.Latitude,
                Longitude = @event.Data.Longitude,
                RSVPCount = 0,
                Title = @event.Data.Title
            };
            context.PopularDinners.Add(popular);
        }
	}
}