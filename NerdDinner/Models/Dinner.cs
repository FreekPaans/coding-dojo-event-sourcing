using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using NerdDinner.Events;
using Newtonsoft.Json;

namespace NerdDinner.Models {

    public class Dinner {

        [HiddenInput(DisplayValue = false)]
        public int DinnerID { get; set; }

        [HiddenInput(DisplayValue = false)]
        public Guid DinnerGuid { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(50, ErrorMessage = "Title may not be longer than 50 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Event Date is required")]
        [Display(Name = "Event Date")]
        public DateTime EventDate { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(256, ErrorMessage = "Description may not be longer than 256 characters")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [StringLength(20, ErrorMessage = "Hosted By name may not be longer than 20 characters")]
        [Display(Name = "Host's Name")]
        public string HostedBy { get; set; }

        [Required(ErrorMessage = "Contact info is required")]
        [StringLength(20, ErrorMessage = "Contact info may not be longer than 20 characters")]
        [Display(Name = "Contact Info")]
        public string ContactPhone { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(50, ErrorMessage = "Address may not be longer than 50 characters")]
        [Display(Name = "Address, City, State, ZIP")]
        public string Address { get; set; }

        [UIHint("CountryDropDown")]
        public string Country { get; set; }

        [HiddenInput(DisplayValue = false)]
        public double Latitude { get; set; }

        [HiddenInput(DisplayValue = false)]
        public double Longitude { get; set; }

        [HiddenInput(DisplayValue = false)]
        public string HostedById { get; set; }

        public bool IsHostedBy(string userName) {
            return String.Equals(HostedById ?? HostedBy, userName, StringComparison.Ordinal);
        }

        public bool IsUserRegistered(string userName) {
            return RSVPs.Any(r => r.AttendeeNameId == userName || (r.AttendeeNameId == null && r.AttendeeName == userName));
        }

        [UIHint("LocationDetail")]
        [NotMapped]
        public LocationDetail Location {
            get {
                return new LocationDetail() { Latitude = this.Latitude, Longitude = this.Longitude, Title = this.Title, Address = this.Address };
            }
            set {
                this.Latitude  = value.Latitude;
                this.Longitude = value.Longitude;
                this.Title     = value.Title;
                this.Address   = value.Address;
            }
        }


        private int _currentEvent = 0;
        private List<RSVP> _rsvps = new List<RSVP>();

        [NotMapped]
        public ICollection<RSVP> RSVPs {
            get {
                return _rsvps.AsReadOnly();
            }
        }


        private readonly List<string> _eventHistory = new List<string>();
        [NotMapped]
        public ICollection<string> History {
            get {
                return this._eventHistory.AsReadOnly();
            }
        }

		

        private void RaiseAndApply(IEventData eventData) {
            var @event = MakeEvent(eventData);
            RaiseEvent(@event);
            Apply(@event);
        }

        private Event MakeEvent(IEventData eventDataObject) {
            return new Event {
                AggregateId            = DinnerGuid,
                AggregateEventSequence = _currentEvent,
                DateTime               = DateTime.UtcNow,
                EventType              = eventDataObject.GetType().FullName,
                Data                   = JsonConvert.SerializeObject(eventDataObject)
            };
        }

        private void RaiseEvent(Event e) {
            EventScope.Raise(e);
            _currentEvent++;
        }
        
        void Apply(Event e) {
            ApplyEvent(e.AddEventType());
        }

		

        

        public void Hydrate(ICollection<Event> events) {
            foreach (var e in events.Where(e => e.AggregateEventSequence >= _currentEvent).OrderBy(e => e.AggregateEventSequence)) { 
                if (e.AggregateEventSequence != _currentEvent) {
                    throw new Exception("Unexpected event sequence");
                }
                Apply(e);
                _currentEvent++;
            }
        }

        public static ICollection<Dinner> HydrateAll(List<Dinner> dinners, List<Event> events) {
            foreach (var dinner in dinners) {
                dinner.Hydrate(events.Where(e => e.AggregateId == dinner.DinnerGuid).ToList());
            }
            return dinners;
        }


		public ICollection<Event> RSVP(string name, string friendlyName) {
            return EventScope.Start(()=>{
                if (IsUserRegistered(name)) {
                    return;
                }

                var RSVPedEvent = new RSVPed {
                    Name         = name,
                    FriendlyName = friendlyName,
                    DinnerId     = DinnerID
                };

                RaiseAndApply(RSVPedEvent);
            });
        }
		
		internal ICollection<Event> CancelRSVP(string userId) {
			return EventScope.Start(()=>{
				if(!IsUserRegistered(userId)) {
					return;
				}
				var rsvpCanceled = new RSVPCanceled {
                    Name = userId,
                    DinnerId = DinnerID
                };

				RaiseAndApply(rsvpCanceled);
			});	
		}

		internal ICollection<Event> ChangeAddress(string newAddress,string changedReason) {
			return EventScope.Start(()=> {
				var addressChanged = new AddressChanged {
					NewAddress = newAddress,
					Reason = changedReason
				};

				RaiseAndApply(addressChanged);
			});
		}


        void ApplyEvent(Event<RSVPed> @event) {
            var rsvp = new RSVP();
            rsvp.DinnerID       = this.DinnerID;
            rsvp.AttendeeName   = @event.Data.FriendlyName;
            rsvp.AttendeeNameId = @event.Data.Name;
            _rsvps.Add(rsvp);

			_eventHistory.Add(string.Format("{0} {1} RSVPed", @event.DateTime,@event.Data.FriendlyName));
        }

		void ApplyEvent(Event<RSVPCanceled> @event) {
            var rsvp = _rsvps.First(_=>_.AttendeeNameId == @event.Data.Name);
			_rsvps.Remove(rsvp);
			_eventHistory.Add(string.Format("{0} {1} canceled", @event.DateTime,rsvp.AttendeeName));
        }

		void ApplyEvent(Event<AddressChanged> @event) {
            Address = @event.Data.NewAddress;

			if(!string.IsNullOrWhiteSpace(@event.Data.Reason)) {
				_eventHistory.Add(string.Format("{0} Address changed to: {1}, because of: {2}",@event.DateTime,@event.Data.NewAddress,@event.Data.Reason));
			}
			else {	
				_eventHistory.Add(string.Format("{0} Address changed to: {1}",@event.DateTime,@event.Data.NewAddress));
			}
        }
    }


    public class LocationDetail {
        public double Latitude;
        public double Longitude;
        public string Title;
        public string Address;
    }

}