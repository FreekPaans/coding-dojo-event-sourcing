using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Moq;
using NerdDinner.Controllers;
using NerdDinner.Models;
using NerdDinner.Tests.CodingDojo.Fakes;
using NUnit.Framework;

namespace NerdDinner.Tests.CodingDojo
{
    partial class DojoTests
    {
        readonly List<Event> _publishedEvents = new List<Event>();

        [SetUp]
        public void SetUp()
        {
            InitializeLocalDbWithTestData();

            _publishedEvents.Clear();
            NerdDinners.ClearEventHandlers();
            NerdDinners.OnEventsPublished((db,e) => {
                _publishedEvents.Add(e);
            });
        }

        private static void InitializeLocalDbWithTestData()
        {
            Database.SetInitializer<NerdDinners>(new CreateDatabaseIfNotExistsIncludingIndicesAndUDFs());

            var dbContext = new NerdDinners();
            dbContext.Database.ExecuteSqlCommand("TRUNCATE TABLE [Dinners]");
            dbContext.Database.ExecuteSqlCommand("TRUNCATE TABLE [Events]");

            var testData = FakeDinnerData.CreateTestDinners();

            foreach (var testDinner in testData.Item1)
            {
                dbContext.Dinners.Add(testDinner);
            }
            foreach (var testEvent in testData.Item2)
            {
                dbContext.Events.Add(testEvent);
            }

            dbContext.SaveChanges();
        }

        RSVPController CreateRSVPControllerAs(string userName)
        {
            var mock = new Mock<ControllerContext>();
            var nerdIdentity = FakeIdentity.CreateIdentity(userName);
            mock.SetupGet(p => p.HttpContext.User.Identity).Returns(nerdIdentity);

            var controller = new RSVPController(new DinnerRepository(new NerdDinners()));
            controller.ControllerContext = mock.Object;

            return controller;
        }

        DinnersController CreateDinnersControllerAs(string userName)
        {

            var mock = new Mock<ControllerContext>();
            var nerdIdentity = FakeIdentity.CreateIdentity(userName);
            mock.SetupGet(p => p.HttpContext.User.Identity).Returns(nerdIdentity);

            var controller = new DinnersController(new DinnerRepository(new NerdDinners()), nerdIdentity);
            controller.ControllerContext = mock.Object;

            return controller;
        }

        SearchController CreateSearchControllerAs(string userName) {
            var mock = new Mock<ControllerContext>();
            var nerdIdentity = FakeIdentity.CreateIdentity(userName);
            mock.SetupGet(p => p.HttpContext.User.Identity).Returns(nerdIdentity);

            var controller = new SearchController(new DinnerRepository(new NerdDinners()));
            controller.ControllerContext = mock.Object;

            return controller;
        }

    }
}
