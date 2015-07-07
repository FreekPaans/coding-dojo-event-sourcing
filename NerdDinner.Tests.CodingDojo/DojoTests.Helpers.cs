using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using NerdDinner.Controllers;
using NerdDinner.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using NerdDinner.Events;

namespace NerdDinner.Tests.CodingDojo
{
    partial class DojoTests
    {
        private int CreateDinner()
        {
            var testDinner = new Dinner { Title = "TestDinner", EventDate = new DateTime(2016, 1, 1), Description = "TestDinner Description", HostedBy = "scottha", HostedById = "scottha", ContactPhone = "0123456789", Address = "TestDinner Address", Country = "Europe" };

			return CreateDinner(testDinner);
        }

		private int CreateDinner(Dinner testDinner) {
			var controller = CreateDinnersControllerAs("scottha");
			var result = controller.Create(testDinner);

			return (int)GetRedirectResultRouteValues(result)["id"];
		}

        private void CancelRSVP(string userName, int dinnerId)
        {
            var controller = CreateRSVPControllerAs(userName);
            controller.Cancel(dinnerId);
        }

        private void RSVPForDinner(string userName, int dinnerId)
        {
            var controller = CreateRSVPControllerAs(userName);
            controller.Register(dinnerId);
        }

        private ActionResult ChangeDinnerAddress(string newAddress, string asUser, int dinnerId, string reason = null)
        {
            var controller = CreateDinnersControllerAs(asUser);
            return controller.ChangeAddress(dinnerId, newAddress, reason);
        }

        private void AssertDinnerAddress(string expectedAddress, int dinnerId)
        {
            var dinnerDetails = GetDinnerDetails(dinnerId);
            Assert.AreEqual(expectedAddress, dinnerDetails.Address);
        }

        private void AssertRSVPedInDinnerHistory(string expectedDinnerHistoryText, int dinnerId)
        {
            var dinnerDetails = GetDinnerDetails(dinnerId);

            Assert.IsTrue(dinnerDetails.History.Any(h => h.EndsWith(expectedDinnerHistoryText)), "DinnerHistory text '{0}' not found", expectedDinnerHistoryText);
        }

        private void AssertTextInDinnerHistoryAtIndex(string expectedDinnerHistoryText, int dinnerId, int index)
        {
            var dinnerDetails = GetDinnerDetails(dinnerId);

            Assert.IsTrue(dinnerDetails.History.Count() > index, "DinnerHistory does not contain enough entries");
            Assert.IsTrue(dinnerDetails.History.ElementAt(index).EndsWith(expectedDinnerHistoryText), "DinnerHistory text '{0}' not found at index {1}", expectedDinnerHistoryText, index);
        }


        private void AssertDinnerInMyDinners(string userName, int dinnerId)
        {
            var myDinners = GetMyDinners(userName);

            var actualDinner = myDinners.SingleOrDefault(d => d.DinnerID == dinnerId);

            Assert.IsNotNull(actualDinner, "Dinner in MyDinners for User {0} not found", userName);

        }

        private void AssertRSVPedForDinner(string userName, int dinnerId)
        {
            var dinnerDetails = GetDinnerDetails(dinnerId);

            var actualRSVP = dinnerDetails.RSVPs.SingleOrDefault(rsvp => rsvp.AttendeeName == userName);

            Assert.IsNotNull(actualRSVP, "RSVP for user {0} not found", userName);
        }

        private void AssertRSVPedForDinnerCount(string userName, int dinnerId, int expectedCount)
        {
            var dinnerDetails = GetDinnerDetails(dinnerId);

            var actualRSVPCount = dinnerDetails.RSVPs.Count(rsvp => rsvp.AttendeeName == userName);

            Assert.AreEqual(expectedCount, actualRSVPCount, "RSVP count not valid");
        }

        private void AssertRSVPCountInPopularDinnersList(string userName, int dinnerId, int expectedCount) {
            var popularDinners = GetPopularDinners(userName);

            var dinner = popularDinners.SingleOrDefault(d => d.DinnerID == dinnerId);

            Assert.IsNotNull(dinner, "Dinner not found in popuplar dinners");
            Assert.AreEqual(expectedCount, dinner.RSVPCount, "RSVP count does not match expected count");
        }

        private Dinner GetDinnerDetails(int dinnerId)
        {
            var dinnerController = CreateDinnersControllerAs("scotthb");
            var dinnerResult = dinnerController.Details(dinnerId);

            return GetViewModel<Dinner>(dinnerResult);
        }

        private ICollection<Dinner> GetMyDinners(string userName)
        {
            var dinnerController = CreateDinnersControllerAs(userName);
            var dinnerResult = dinnerController.My();

            return GetViewModel<IEnumerable<Dinner>>(dinnerResult).ToList();
        }

        private ICollection<JsonDinner> GetPopularDinners(string userName) {
            var searchController = CreateSearchControllerAs(userName);

            var result = searchController.GetMostPopularDinners(1000);

            return GetDataFromJsonResult<ICollection<JsonDinner>>(result);
        } 


        public RouteValueDictionary GetRedirectResultRouteValues(ActionResult result)
        {
            Assert.IsInstanceOf<RedirectToRouteResult>(result);
            var redirectToRouteResult = result as RedirectToRouteResult;

            Assert.IsNotNull(redirectToRouteResult);
            return redirectToRouteResult.RouteValues;
        }

        public T GetViewModel<T>(ActionResult result) where T : class
        {
            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.IsInstanceOf<T>(viewResult.Model);
            return (T)viewResult.Model;
        }

        public T GetDataFromJsonResult<T>(ActionResult result) where T : class
        {
            Assert.IsInstanceOf<JsonResult>(result);
            var viewResult = result as JsonResult;

            Assert.IsInstanceOf<T>(viewResult.Data);
            return (T)viewResult.Data;
        }

        void AssertEventPublished<T>(Func<Event<T>,bool> filter, Action<Event<T>> assertOn) where T : IEventData{
			var events = _publishedEvents
                .Where(p=>p.EventType == typeof(T).FullName)
                .Select(e=>(Event<T>)e.AddEventType())
                .Where(filter)
                .ToList();
            
            Assert.AreNotEqual(0, events.Count, "event not found");            
            Assert.AreEqual(1, events.Count, "Expected to find exactly one event");            

			assertOn(events.First());
		}
    }
}
