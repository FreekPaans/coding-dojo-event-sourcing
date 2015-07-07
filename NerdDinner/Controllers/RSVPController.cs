using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using NerdDinner.Helpers;
using NerdDinner.Models;

namespace NerdDinner.Controllers
{
    [HandleErrorWithELMAH]
    public class RSVPController : Controller
    {

        IDinnerRepository dinnerRepository;
        

        //
        // Dependency Injection enabled constructors
        public RSVPController()
            : this(new DinnerRepository())
        {
        }

        public RSVPController(IDinnerRepository repository)
        {
            dinnerRepository = repository;
        }

        //
        // AJAX: /Dinners/Register/1
        [Authorize, HttpPost]
        public ActionResult Register(int id)
        {
            Dinner dinner = dinnerRepository.Find(id);

            NerdIdentity nerd = (NerdIdentity)User.Identity;

            var publishedEvents = dinner.RSVP(nerd.Name, nerd.FriendlyName);

            dinnerRepository.StoreEvents(publishedEvents);

            dinnerRepository.SubmitChanges();
            

            return Content("Thanks - we'll see you there!");
        }

        //
        // AJAX: /RSVP/Cancel/1

        [Authorize, HttpPost]
        public ActionResult Cancel(int id) {
            var dinner = dinnerRepository.Find(id);

			NerdIdentity nerd = (NerdIdentity)User.Identity;

            var publishedEvents = dinner.CancelRSVP(nerd.Name);

            dinnerRepository.StoreEvents(publishedEvents);

            dinnerRepository.SubmitChanges();

            return Content("Thanks - we'll see you there!");
        }

        
    }
}
