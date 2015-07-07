using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace NerdDinner.Models {

    public class DinnerRepository : IDinnerRepository {

        private readonly NerdDinners db;

        public DinnerRepository() {
            this.db = DependencyResolver.Current.GetService<INerdDinners>() as NerdDinners;
        }

        public DinnerRepository(NerdDinners db) {
            this.db = db;
        }

        public IQueryable<Dinner> FindByLocation(float latitude, float longitude)
        {
            var results = db.Database.SqlQuery<Dinner>("SELECT * FROM Dinners WHERE EventDate >= {0} AND dbo.DistanceBetween({1}, {2}, Latitude, Longitude) < 1000", DateTime.Now, latitude, longitude).ToList();

            foreach (Dinner dinner in results) {
                // TODO Hydrate Events
                //dinner.RSVPs = new List<RSVP>();

                //var rsvps = db.RSVPs.Where(x => x.DinnerID == dinner.DinnerID);

                //foreach (RSVP rsvp in rsvps)
                //{
                //    dinner.RSVPs.Add(rsvp);
                //}
            }

            return results.AsQueryable<Dinner>();
        }

        public IQueryable<Dinner> FindUpcomingDinners() {
            return from dinner in All
                   where dinner.EventDate >= DateTime.Now
                   orderby dinner.EventDate
                   select dinner;
        }

        public IQueryable<Dinner> FindDinnersByText(string q) {
            return All
                .Where(d => d.Title.Contains(q)
                            || d.Description.Contains(q)
                            || d.HostedBy.Contains(q));
        }

        public IQueryable<Dinner> All {
            get { return db.Dinners; }
        }

        public Dinner Find(int id) {
            var dinner = All.Select(_=> new {
                dinner = _,
                events = db.Events.Where(e=>e.AggregateId == _.DinnerGuid)
            }).SingleOrDefault(d => d.dinner.DinnerID == id);

            dinner.dinner.Hydrate(dinner.events.ToList());
            return dinner.dinner;
        }

        

        //
        // Insert/Delete Methods

        public void InsertOrUpdate(Dinner dinner) {
            if (dinner.DinnerID == default(int)) {
                // New entity
                db.Dinners.Add(dinner);
            }
            else {
                // Existing entity
                db.Entry(dinner).State = EntityState.Modified;
            }
        }

        public void Delete(int id) {
            var dinner = Find(id);
            db.Dinners.Remove(dinner);
        }

        public void StoreEvents(ICollection<Event> events) {
            db.StoreEvents(events);
        }

        public IQueryable<Event> AllEvents {
            get {
                return db.Events;
            }
        }

        public void SubmitChanges()
        {
            db.SaveChanges();
        }

		public IQueryable<PopularDinner> FindMostPopularDinners()
        {
            return db.PopularDinners.OrderByDescending(p => p.RSVPCount);
        } 
    }
}
