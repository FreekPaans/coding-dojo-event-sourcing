using System.Collections;
using NerdDinner.Events;
using NerdDinner.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NerdDinner.Controllers;

namespace NerdDinner.Tests.CodingDojo {
    partial class DojoTests {
		[Test]
        public void Create_Dinner_Should_Raise_DinnerCreated()
        {
            var dinnerID = CreateDinner(new Dinner { 
                Title = "TestDinner", 
                EventDate = new DateTime(2016, 1, 1), 
                Description = "TestDinner Description", 
                ContactPhone = "0123456789", 
                Address = "TestDinner Address", 
                Country = "Europe" });

            AssertEventPublished<DinnerCreated>(e=>true, e=> {
                Assert.AreEqual(dinnerID,e.Data.DinnerID);
                Assert.AreEqual("TestDinner",e.Data.Title);
                Assert.AreEqual(new DateTime(2016, 1, 1),e.Data.EventDate); 
                Assert.AreEqual("TestDinner Description",e.Data.Description);
                Assert.AreEqual("scottha", e.Data.HostedBy);
                Assert.AreEqual("scottha",e.Data.HostedById);
                Assert.AreEqual("0123456789", e.Data.ContactPhone);
                Assert.AreEqual("TestDinner Address",e.Data.Address);
                Assert.AreEqual("Europe", e.Data.Country);
            });
        }

        [Test]
        public void GetPopularDinners_Is_Populated_From_PopularDinners_ReadModel() {
            PopulatePopularDinnerReadModelForDinner(dinnerId:1, rsvpCount:10);
            PopulatePopularDinnerReadModelForDinner(dinnerId:3, rsvpCount:5);
            PopulatePopularDinnerReadModelForDinner(dinnerId:4, rsvpCount:15);

            var dinners = GetMostPopularDinners();

            var mostPopular = dinners.First();

            Assert.AreEqual(4,mostPopular.DinnerID,"Not the expected dinner");
            Assert.AreEqual(15, mostPopular.RSVPCount, "RSVP count is wrong");

            var secondPopular = dinners.Skip(1).First();

            Assert.AreEqual(1,secondPopular.DinnerID,"Not the expected dinner");
            Assert.AreEqual(10, secondPopular.RSVPCount, "RSVP count is wrong");
        }

        [Test]
        public void PopularDinners_Updates_RSVP_Count_on_RSVPed() {
            PopulatePopularDinnerReadModelForDinner(dinnerId:1, rsvpCount:0);
            
            Raise(new RSVPed { DinnerId = 1, FriendlyName = "freek", Name = "freek" });

            AssertRSVPCountFor(dinnerId:1, expectedCount:1);
        }

        [Test]
        public void Create_Dinner_Should_Create_Popular_Dinner_Record() {
            NerdDinners.OnEventsPublished(PopularDinner.Handle);

            var dinnerID = CreateDinner(new Dinner { 
                Title = "TestDinner", 
                EventDate = new DateTime(2016, 1, 1), 
                Description = "TestDinner Description", 
                ContactPhone = "0123456789", 
                Address = "TestDinner Address", 
                Country = "Europe" });

            var popular = new NerdDinners().PopularDinners.Find(dinnerID);

            Assert.IsNotNull(popular,"Popular dinner not found");

            Assert.AreEqual(dinnerID,popular.DinnerID);
            Assert.AreEqual("TestDinner",popular.Title);
            Assert.AreEqual(new DateTime(2016, 1, 1),popular.EventDate); 
            Assert.AreEqual("TestDinner Description",popular.Description);
            Assert.AreEqual("scottha", popular.HostedBy);
            Assert.AreEqual("scottha",popular.HostedById);
            Assert.AreEqual("0123456789", popular.ContactPhone);
            Assert.AreEqual("TestDinner Address",popular.Address);
            Assert.AreEqual("Europe", popular.Country);
        }

        private static void Raise(RSVPed rsvped) {
            var dinners = new NerdDinners();
            PopularDinner.Handle(dinners,Event.Make(rsvped,Guid.NewGuid(),0));
            dinners.SaveChanges();
        }

        private void AssertRSVPCountFor(int dinnerId,int expectedCount) {
            var popular = new NerdDinners().PopularDinners.Find(dinnerId);

            Assert.AreEqual(expectedCount,popular.RSVPCount, "RSVP count was wrong");
        }

        private ICollection<JsonDinner> GetMostPopularDinners() {
            var result = CreateSearchControllerAs("freek").GetMostPopularDinners(10);

            var model = GetDataFromJsonResult<ICollection<JsonDinner>>(result);
            return model;
        }

        private void PopulatePopularDinnerReadModelForDinner(int dinnerId, int rsvpCount)
        {
            var ctx = new NerdDinners();
            var pop = PopularDinnerFromDinner(ctx.Dinners.Find(dinnerId));

            pop.RSVPCount = rsvpCount;
            ctx.PopularDinners.Add(pop);

            ctx.SaveChanges();
        }

        private PopularDinner PopularDinnerFromDinner(Dinner dinner)
        {
            var result = new PopularDinner();
            foreach(var prop in typeof(Dinner).GetProperties()) {
                var propOnPopularDinner = typeof(PopularDinner).GetProperties().FirstOrDefault(p => p.Name == prop.Name);
                if(propOnPopularDinner==null) {
                    continue; 
                }
                propOnPopularDinner.SetValue(result,prop.GetValue(dinner,null),null);
            }

            return result;
        }
    }
}
