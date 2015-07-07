using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace NerdDinner.Tests.CodingDojo
{
    partial class DojoTests {

        [Test]
        public void Change_Dinner_Address_Shows_In_Dinner_Details()
        {
            ChangeDinnerAddress("New Test Address Street 2, TestCity", asUser: "SomeUser", dinnerId: 1);

            AssertDinnerAddress("New Test Address Street 2, TestCity", 1);
        }

        [Test]
        public void DinnerHistory_Shows_Address_Changed()
        {
            ChangeDinnerAddress("New Test Address Street 2, TestCity", asUser: "SomeUser", dinnerId: 1);

            AssertRSVPedInDinnerHistory("Address changed to: New Test Address Street 2, TestCity", 1);
        }

        [Test]
        public void DinnerHistory_Shows_Address_Changed_With_Reason()
        {
            ChangeDinnerAddress("New Test Address Street 2, TestCity", asUser: "SomeUser", dinnerId: 1, reason: "New venue");

            AssertRSVPedInDinnerHistory("Address changed to: New Test Address Street 2, TestCity, because of: New venue", 1);
        }

		[Test]
        public void ChangeDinner_Action_Should_Redirect_To_Dinner()
        {
            var result = ChangeDinnerAddress("New Test Address Street 2, TestCity", asUser: "SomeUser", dinnerId: 1, reason: "New venue");

            var routeValue = GetRedirectResultRouteValues(result);

			Assert.AreEqual(1, routeValue["id"]);
			Assert.AreEqual("Details", routeValue["action"]);
        }
       
    }
}
