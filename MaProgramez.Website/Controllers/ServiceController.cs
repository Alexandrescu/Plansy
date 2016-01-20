using System;
using System.Data.Entity.Core.Objects;
using System.Globalization;
using System.Web.DynamicData;
using GoogleMaps.LocationServices;
using MaProgramez.Repository.BusinessLogic;
using MaProgramez.Repository.DbContexts;
using MaProgramez.Repository.Entities;
using MaProgramez.Repository.Utility;
using Org.BouncyCastle.Bcpg;

namespace MaProgramez.Website.Controllers
{
    using Extensions;
    using Microsoft.Ajax.Utilities;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.Owin;
    using Resources;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;
    using Utility;
    using ViewModels;

    [CustomAuthorize]
    public partial class ServiceController : BaseController
    {
        #region PRIVATE FIELDS

        private ApplicationUserManager _userManager;

        #endregion PRIVATE FIELDS

        #region PROPERTIES

        public ApplicationUserManager UserManager
        {
            get { return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }

            private set { _userManager = value; }
        }

        #endregion PROPERTIES

        #region ACTIONS

        // GET: Service
        public virtual ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public virtual void AddOrUpdateRating(int slotId, int score)
        {
            var slot = RetrieveOthers.GetSlotById(slotId);
            var rating = slot.Provider.ProgrammingPerSlot
                ? RetrieveOthers.GetRating(User.Identity.GetUserId(), slotId, null)
                : RetrieveOthers.GetRating(User.Identity.GetUserId(), 0, slot.ProviderId);

            using (var db = new AppDbContext())
            {
                if (rating == null)
                {
                    var r = new Rating()
                    {
                        UserId = User.Identity.GetUserId(),
                        ProviderId = slot.ProviderId,
                        Score = score
                    };
                    if (slot.Provider.ProgrammingPerSlot)
                        r.SlotId = slotId;

                    db.Ratings.Add(r);
                }
                else
                {
                    rating.Score = score;
                    db.Ratings.Attach(rating);
                    db.Entry(rating).State = EntityState.Modified;
                }
                db.SaveChanges();
            }
        }

        [AllowAnonymous]
        public virtual JsonResult GetOperations(int id)
        {
            using (var db = new AppDbContext())
            {
                var sop =
                    db.SlotOperations.Where(x => x.SlotId == id).Include("Operation")
                        .OrderBy(x => x.Operation.Description)
                        .Select(x => new { name = x.Operation.Description, id = x.Id })
                        .ToList();
                return Json(sop, JsonRequestBehavior.AllowGet);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public virtual JsonResult GetHours(string providerId, int categoryId, string dateString, List<int> operationIds, int? slotId, bool ignoreTimetable = false)
        {
            if (operationIds == null || !operationIds.Any())
            {
                var res = RetrieveLists.GetProviderSlotOperations(providerId, categoryId, new List<int>(), null, null,
                    null, User.Identity.GetUserId(), ignoreTimetable);

                return
                    Json(
                        new
                        {
                            firstAvailableDate = res.FirstAvailableDate,
                            availableHours = res.AvailableHours,
                            availableMinutes = res.AvailableMinutesForSelectedHour,
                            availableSlots = res.AvailableSlots.Select(x => new {name = x.Name, id = x.Id})
                        }, JsonRequestBehavior.AllowGet);
            }

            //var date = DateTime.Parse(dateString);
            DateTime date = DateTime.ParseExact(dateString, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var result = RetrieveLists.GetProviderSlotOperations(providerId, categoryId, operationIds, date, null,
                slotId, User.Identity.GetUserId(), ignoreTimetable);

            return
                Json(
                    new
                    {
                        firstAvailableDate = result.FirstAvailableDate,
                        availableHours = result.AvailableHours,
                        availableMinutes = result.AvailableMinutesForSelectedHour,
                        availableSlots = result.AvailableSlots.Select(x => new { name = x.Name, id = x.Id })
                    }, JsonRequestBehavior.AllowGet);

        }

        [AllowAnonymous]
        [HttpPost]
        public virtual JsonResult GetMinutes(string providerId, int categoryId, string dateString, List<int> operationIds, int? slotId, int selectedHour, bool ignoreTimetable)
        {
            if (operationIds == null || !operationIds.Any()) return null;
            DateTime date = DateTime.ParseExact(dateString, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            var result = RetrieveLists.GetProviderSlotOperations(providerId, categoryId, operationIds, date, selectedHour,
                slotId, User.Identity.GetUserId(), ignoreTimetable);

            return
                Json(
                    new
                    {
                        availableMinutes = result.AvailableMinutesForSelectedHour,
                    }, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public virtual JsonResult GetCities(int id)
        {
            using (var db = new AppDbContext())
            {
                var cities =
                    db.Cities.Where(x => x.CountyId == id)
                        .OrderBy(x => x.Name)
                        .Select(x => new { name = x.Name, id = x.Id })
                        .ToList();
                return Json(cities, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> UpdateAddress(Address address)
        {
            string userId = User.Identity.GetUserId();
            using (var db = new AppDbContext())
            {
                Address oldAddress = db.Addresses.FirstOrDefault(x => x.Id == address.Id && x.UserId == userId);
                if (oldAddress == null) return null;

                ViewBag.Counties = db.Counties.OrderBy(x => x.Name).ToList();

                oldAddress.AddressText = address.AddressText;
                oldAddress.PostalCode = address.PostalCode;
                oldAddress.CityId = address.CityId;
                oldAddress.CountryId = address.CountryId;

                if (oldAddress.CountryId == 0)
                {
                    oldAddress.CountryId = 1; //default country = ROMANIA
                }

                //Get Latitude and Longitude
                var locationService = new GoogleLocationService();
                var point = locationService.GetLatLongFromAddress(oldAddress.ToString());
                oldAddress.Latitude = point.Latitude;
                oldAddress.Longitude = point.Longitude;

                db.Entry(oldAddress).State = EntityState.Modified;
                if (await db.SaveChangesAsync() < 0)
                {
                    return RedirectToAction(MVC.Error.Index());
                }
                Address newAddress =
                    db.Addresses.Include("UserCity")
                        .Include("UserCity.CityCounty")
                        .Include("UserCity.CityCounty.Cities")
                        .Include("UserCountry")
                        .FirstOrDefault(x => x.Id == address.Id);

                return Json(new
                {
                    view = RenderRazorViewToString(MVC.Manage.Views.PartialViews._AddressListItem, newAddress),
                    addressId = newAddress.Id,
                    addressType = newAddress.AddressType,
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> AddAddress(Address newAddress)
        {
            if (!ModelState.IsValid) return null;

            string userId = User.Identity.GetUserId();
            newAddress.UserId = userId;
            using (var db = new AppDbContext())
            {
                newAddress.AddressType = db.Addresses.Any(x => x.UserId == userId)
                    ? AddressType.InvoiceAddress
                    : AddressType.PlaceOfBusinessAddress;

                if (newAddress.CountryId == 0)
                {
                    newAddress.CountryId = 1; //Default country = ROMANIA
                }

                //Get Latitude and Longitude
                var locationService = new GoogleLocationService();

                if (newAddress.UserCity == null)
                {
                    newAddress.UserCity = db.Cities.Include("CityCounty").FirstOrDefault(c => c.Id == newAddress.CityId);
                }

                if (newAddress.UserCountry == null)
                {
                    newAddress.UserCountry = db.Countries.FirstOrDefault(c => c.Id == newAddress.CountryId);
                }

                var point = locationService.GetLatLongFromAddress(newAddress.ToString());
                if (point != null)
                {
                    newAddress.Latitude = point.Latitude;
                    newAddress.Longitude = point.Longitude;
                }

                db.Addresses.Add(newAddress);
                if (await db.SaveChangesAsync() < 0)
                {
                    return RedirectToAction(MVC.Error.Index());
                }

                ViewBag.Counties = db.Counties.OrderBy(x => x.Name).ToList();
                Address address =
                    db.Addresses.Include("UserCity")
                        .Include("UserCity.CityCounty")
                        .Include("UserCity.CityCounty.Cities")
                        .Include("UserCountry")
                        .FirstOrDefault(x => x.Id == newAddress.Id);

                return
                    Json(
                        new
                        {
                            addressId = address.Id,
                            addressType = address.AddressType,
                            view =
                                RenderRazorViewToString(MVC.Manage.Views.PartialViews._AddressListItem, address)
                        });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult DeleteAddress(int addressId)
        {
            string userId = User.Identity.GetUserId();
            using (var db = new AppDbContext())
            {
                Address address =
                    db.Addresses.FirstOrDefault(
                        x => x.Id == addressId && x.UserId == userId && x.AddressType != AddressType.InvoiceAddress);

                if (address == null) return null;

                db.Addresses.Remove(address);
                if (db.SaveChanges() < 0)
                {
                    return RedirectToAction(MVC.Error.Index());
                }

                return Json(addressId);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Roles = "Admin,Agent")]
        public virtual async Task<ActionResult> UpdateAddressByUserId(Address address)
        {
            if (address == null) return null;
            using (var db = new AppDbContext())
            {
                if (!string.IsNullOrWhiteSpace(address.UserId))
                {
                    Address oldAddress =
                        db.Addresses.FirstOrDefault(x => x.Id == address.Id && x.UserId == address.UserId);
                    if (oldAddress != null)
                    {
                        ViewBag.Counties = db.Counties.OrderBy(x => x.Name).ToList();

                        oldAddress.AddressText = address.AddressText;
                        oldAddress.PostalCode = address.PostalCode;
                        oldAddress.CityId = address.CityId;
                        oldAddress.CountryId = address.CountryId;

                        if (oldAddress.CountryId == 0)
                        {
                            oldAddress.CountryId = 1; //default country = ROMANIA
                        }

                        db.Entry(oldAddress).State = EntityState.Modified;
                        if (await db.SaveChangesAsync() < 0)
                        {
                            return RedirectToAction(MVC.Error.Index());
                        }

                        Address newAddress =
                            db.Addresses.Include("UserCity")
                                .Include("UserCity.CityCounty")
                                .Include("UserCity.CityCounty.Cities")
                                .Include("UserCountry")
                                .FirstOrDefault(x => x.Id == address.Id);

                        return Json(new
                        {
                            view =
                                RenderRazorViewToString(MVC.UsersAdmin.Views.PartialViews._AddressListItem,
                                    newAddress),
                            addressId = newAddress.Id,
                            addressType = newAddress.AddressType,
                        });
                    }
                }
                else
                {
                    Address newAddress = SessionUtility.UpdateAddress(address);
                    if (newAddress != null)
                    {
                        newAddress.UserCity = db.Cities
                            .Include("CityCounty")
                            .Include("CityCounty.Cities")
                            .FirstOrDefault(x => x.Id == newAddress.CityId);
                        newAddress.UserCountry = db.Countries.FirstOrDefault(x => x.Id == newAddress.CountryId);

                        ViewBag.Counties = db.Counties.OrderBy(x => x.Name).ToList();

                        return Json(new
                        {
                            view =
                                RenderRazorViewToString(MVC.UsersAdmin.Views.PartialViews._AddressListItem,
                                    newAddress),
                            addressId = newAddress.Id,
                            addressType = newAddress.AddressType,
                        });
                    }
                }

                return null;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public virtual async Task<ActionResult> AddAddressByUserId(Address newAddress)
        {
            if (ModelState.ContainsKey("newAddress.UserId"))
            {
                ModelState["newAddress.UserId"].Errors.Clear();
            }

            if (!ModelState.IsValid) return null;

            using (var db = new AppDbContext())
            {
                if (!string.IsNullOrWhiteSpace(newAddress.UserId))
                {
                    string userId = newAddress.UserId;
                    if (db.Addresses.Any(x => x.UserId == userId))
                    {
                        newAddress.AddressType = AddressType.PlaceOfBusinessAddress;
                    }
                    else
                    {
                        newAddress.AddressType = AddressType.InvoiceAddress;
                    }

                    if (newAddress.CountryId == 0)
                    {
                        newAddress.CountryId = 1; //Default country = ROMANIA
                    }

                    //Get Latitude and Longitude
                    var locationService = new GoogleLocationService();

                    if (newAddress.UserCity == null)
                    {
                        newAddress.UserCity = db.Cities.Include("CityCounty").FirstOrDefault(c => c.Id == newAddress.CityId);
                    }

                    if (newAddress.UserCountry == null)
                    {
                        newAddress.UserCountry = db.Countries.FirstOrDefault(c => c.Id == newAddress.CountryId);
                    }

                    var point = locationService.GetLatLongFromAddress(newAddress.ToString());
                    if (point != null)
                    {
                        newAddress.Latitude = point.Latitude;
                        newAddress.Longitude = point.Longitude;
                    }

                    db.Addresses.Add(newAddress);
                    if (await db.SaveChangesAsync() < 0)
                    {
                        return RedirectToAction(MVC.Error.Index());
                    }

                    ViewBag.Counties = db.Counties.OrderBy(x => x.Name).ToList();
                    Address address =
                        db.Addresses.Include("UserCity")
                            .Include("UserCity.CityCounty")
                            .Include("UserCity.CityCounty.Cities")
                            .Include("UserCountry")
                            .FirstOrDefault(x => x.Id == newAddress.Id);

                    return
                        Json(
                            new
                            {
                                addressId = address.Id,
                                addressType = address.AddressType,
                                view =
                                    RenderRazorViewToString(MVC.UsersAdmin.Views.PartialViews._AddressListItem,
                                        address)
                            });
                }
                else
                {
                    Address address = SessionUtility.AddAddresses(newAddress);
                    if (address != null)
                    {
                        ViewBag.Counties = db.Counties.OrderBy(x => x.Name).ToList();

                        address.UserCity = db.Cities
                            .Include("CityCounty")
                            .Include("CityCounty.Cities")
                            .FirstOrDefault(x => x.Id == address.CityId);
                        address.UserCountry = db.Countries.FirstOrDefault(x => x.Id == address.CountryId);

                        return
                            Json(
                                new
                                {
                                    addressId = address.Id,
                                    addressType = address.AddressType,
                                    view =
                                        RenderRazorViewToString(
                                            MVC.UsersAdmin.Views.PartialViews._AddressListItem, address)
                                });
                    }
                }

                return null;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Roles = "Admin,Agent")]
        public virtual ActionResult DeleteAddressByUserId(int addressId, string userId)
        {
            if (!string.IsNullOrWhiteSpace(userId))
            {
                using (var db = new AppDbContext())
                {
                    Address address =
                        db.Addresses.FirstOrDefault(
                            x => x.Id == addressId && x.UserId == userId && x.AddressType != AddressType.InvoiceAddress);

                    if (address == null) return null;

                    db.Addresses.Remove(address);
                    if (db.SaveChanges() < 0)
                    {
                        return RedirectToAction(MVC.Error.Index());
                    }

                    return Json(addressId);
                }
            }
            SessionUtility.RemoveAddress(addressId);
            return Json(addressId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult UpdatePersonalDetails(ApplicationUser user)
        {
            string userId = User.Identity.GetUserId();
            using (var db = new AppDbContext())
            {
                ApplicationUser applicationUser = db.Users.FirstOrDefault(x => x.Id == userId);

                if (applicationUser == null)
                    return
                        Json(
                            new
                            {
                                view =
                                    RenderRazorViewToString(MVC.Manage.Views.PartialViews._PersonalDetails,
                                        applicationUser)
                            });

                applicationUser.FirstName = user.FirstName;
                applicationUser.LastName = user.LastName;
                applicationUser.AcceptsNotificationOnEmail = user.AcceptsNotificationOnEmail;
                applicationUser.AcceptsNotificationOnSms = user.AcceptsNotificationOnSms;

                db.Entry(applicationUser).State = EntityState.Modified;
                if (db.SaveChanges() < 0)
                {
                    return RedirectToAction(MVC.Error.Index());
                }

                return
                    Json(
                        new
                        {
                            view =
                                RenderRazorViewToString(MVC.Manage.Views.PartialViews._PersonalDetails,
                                    applicationUser)
                        });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public virtual ActionResult LandingPageContact(ContactViewModel contactModel)
        {
            if (!ModelState.IsValid) return Json(new { Message = "Invalid", Error = true });

            var message = new IdentityMessage
            {
                Subject = "Cerere cont prestator",
                Destination = "contact@Plansy.nl",
                Body =
                    string.Format("{0} {1}(Company: {2}), Judet: {3}, Localitate: {4}, Phone: {5}",
                        contactModel.FirstName, contactModel.LastName, contactModel.Company, contactModel.County,
                        contactModel.Location, contactModel.Phone)
            };

            UserManager.EmailService.Send(message);

            return Json(new { Message = Resource.ContactMessageConfirm, Error = false });
        }

        #endregion ACTIONS
    }
}