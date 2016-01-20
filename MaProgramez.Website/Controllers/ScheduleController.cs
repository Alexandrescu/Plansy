using MaProgramez.Repository.BusinessLogic;
using MaProgramez.Repository.DbContexts;
using MaProgramez.Repository.Entities;
using MaProgramez.Repository.Models;
using MaProgramez.Website.Extensions;
using MaProgramez.Website.Helpers;
using MaProgramez.Website.Utility;
using MaProgramez.Website.ViewModels;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using iTextSharp.text;
using MvcPaging;
using Resource = MaProgramez.Resources.Resource;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Util.Store;
using Google.Apis.Auth.OAuth2.Web;
using System.IO;

namespace MaProgramez.Website.Controllers
{
    public partial class ScheduleController : BaseController
    {
        #region Private Fields

        private ApplicationUserManager _userManager;

        #endregion Private Fields

        #region Public Properties

        public ApplicationUserManager UserManager
        {
            get { return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
            private set { _userManager = value; }
        }

        #endregion Public Properties

        #region Public Methods

        [HttpGet]
        [CustomAuthorize(Roles = "Admin, Provider, Employee")]
        public virtual ActionResult AcceptAllPendingSchedules(int slotId)
        {
            var schedules = RetrieveOthers.AcceptAllPendingSchedulesOnSlot(slotId);

            foreach (var sId in schedules)
            {
                var schedule = RetrieveOthers.GetScheduleById(sId);
                var name = schedule.Slot.Provider.ProgrammingPerSlot
                    ? schedule.Slot.Name + "(" + schedule.Slot.Provider.CompanyDisplayName + ")"
                    : schedule.Slot.Provider.CompanyDisplayName;
                var phone = schedule.Slot.Provider.ProgrammingPerSlot
                           ? schedule.Slot.Phone
                           : schedule.Slot.Provider.PhoneNumber;
                var n = RetrieveOthers.AddNotification(schedule.Id, schedule.UserId, NotificationType.Confirmation,
                    Resource.Notification_Validation1 + name + " (" +
                    schedule.ScheduleDateTimeStart.ToString(Thread.CurrentThread.CurrentCulture) + ") "
                    + Resource.Notification_Validation2);

                NotificationCenter.SendNotificationToUser(n.UserId,
                    RenderRazorViewToString("~/Views/Shared/PartialViews/_Notification.cshtml", n));

                if (schedule.User.PhoneNumber != null && schedule.User.AcceptsNotificationOnSms && schedule.User.PhoneNumberConfirmed)
                {
                    MailAndSmsUtility.SendSms(schedule.User.PhoneNumber, n.Text);
                }
            }

            return RedirectToAction(MVC.Schedule.ViewPendingSchedules(slotId));
        }

        [HttpGet]
        [CustomAuthorize(Roles = "Admin, Provider, Employee")]
        public virtual ActionResult AcceptPendingSchedule(int scheduleId)
        {
            RetrieveOthers.AcceptPendingSchedule(scheduleId);

            var schedule = RetrieveOthers.GetScheduleById(scheduleId);
            var name = schedule.Slot.Provider.ProgrammingPerSlot ? schedule.Slot.Name : schedule.Slot.Provider.CompanyDisplayName;
            var phone = schedule.Slot.Provider.ProgrammingPerSlot
                           ? schedule.Slot.Phone
                           : schedule.Slot.Provider.PhoneNumber;
            var n = RetrieveOthers.AddNotification(scheduleId, schedule.UserId, NotificationType.Confirmation,
                   Resource.Notification_Validation1 + name + " (" +
                    schedule.ScheduleDateTimeStart.ToString(Thread.CurrentThread.CurrentCulture) + ") "
                    + Resource.Notification_Validation2);

            NotificationCenter.SendNotificationToUser(n.UserId,
                RenderRazorViewToString("~/Views/Shared/PartialViews/_Notification.cshtml", n));

            if (schedule.User.PhoneNumber != null && schedule.User.AcceptsNotificationOnSms && schedule.User.PhoneNumberConfirmed)
            {
                MailAndSmsUtility.SendSms(schedule.User.PhoneNumber, n.Text);
            }

            return RedirectToAction(MVC.Schedule.ViewPendingSchedules(schedule.SlotId));
        }

        [HttpGet]
        [CustomAuthorize(Roles = "Client, Admin, Provider")]
        public virtual ActionResult Add(string providerId, int categoryId)
        {
            if (categoryId == 0) categoryId = RetrieveOthers.GetProviderCategories(providerId).First();
            var result = RetrieveLists.GetProviderSlotOperations(
                providerId,
                categoryId,
                new List<int>(), //selectedOperations
                null, //providerParameters.SelectedDate,
                null, //providerParameters.SelectedHour,
                null,
                User.Identity.GetUserId());

            var ops = result.AvailableOperations //RetrieveLists.GetSlotOperationsBySlot(slot.Id)
                .Select(o => new SelectListItem()
                {
                    Value = o.Id.ToString(CultureInfo.InvariantCulture),
                    Text = o.DurationMinutes + Resource.Semicolon + o.Price + Resource.Semicolon +
                           (o.Description.Length > 25 ? o.Description.Substring(0, 25) + "..." : o.Description)
                })
                .ToList();

            var model = new AddAppointmentViewModel()
            {
                Provider = RetrieveOthers.GetUserById(providerId),
                ProviderId = providerId,
                CategoryId = categoryId,
                AvailableSlots = result.AvailableSlots,
                SelectedSlot = result.AvailableSlots.First(),
                SelectedSlotId = result.AvailableSlots.First().Id,
                Operations = ops,
                SelectedOperationIds = result.SelectedOperationIds,//new List<int>(),
                ProviderAddress = RetrieveOthers.GetUserAddress(providerId),
                SelectedDate = result.FirstAvailableDate != null ? DateTime.ParseExact(result.FirstAvailableDate, "dd-MM-yyyy", CultureInfo.InvariantCulture) : DateTime.Today,
                SelectedHour = result.SelectedHour,
                AvailableMinutesForSelectedHour = result.AvailableMinutesForSelectedHour
            };

            if (model.AvailableSlots != null && model.AvailableSlots.Count == 1)
            {
                model.SelectedSlot = model.AvailableSlots.First();
                model.SelectedSlotId = model.SelectedSlot.Id;

                if (!string.IsNullOrWhiteSpace(model.SelectedSlot.UserId))
                {
                    model.ProviderAddress = RetrieveOthers.GetUserAddress(model.SelectedSlot.UserId);
                    model.SelectedSlot.Name += " - " + model.ProviderAddress;
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Roles = "Client, Admin, Provider")]
        public virtual ActionResult Add(AddAppointmentViewModel model)
        {
            ModelState.Clear();

            var selectedDateAndTime =
                model.SelectedDate.Date.AddHours(model.SelectedHour).AddMinutes(model.SelectedMinute);

            if (!ModelState.IsValid ||
                model.SelectedOperationIds == null ||
                !model.SelectedOperationIds.Any() ||
                selectedDateAndTime <= DateTime.Now)
            {
                var result = RetrieveLists.GetProviderSlotOperations(
                model.ProviderId,
                model.CategoryId,
                new List<int>(), //selectedOperations
                null, //providerParameters.SelectedDate,
                null,//providerParameters.SelectedHour,
                model.SelectedSlotId,
                User.Identity.GetUserId());

                var ops = result.AvailableOperations//RetrieveLists.GetSlotOperationsBySlot(slot.Id)
                    .Select(o => new SelectListItem()
                    {
                        Value = o.Id.ToString(CultureInfo.InvariantCulture),
                        Text = o.DurationMinutes + Resource.Semicolon + o.Price + Resource.Semicolon +
                                (o.Description.Length > 25 ? o.Description.Substring(0, 25) + "..." : o.Description)
                    })
                          .ToList();

                model.Provider = RetrieveOthers.GetUserById(model.ProviderId);
                model.AvailableSlots = result.AvailableSlots;
                model.SelectedSlot = result.AvailableSlots.First();
                model.Operations = ops;
                model.SelectedOperationIds = result.SelectedOperationIds;//new List<int>(),

                model.ProviderAddress = RetrieveOthers.GetUserAddress(!string.IsNullOrWhiteSpace(model.SelectedSlot.UserId) ?
                                                            model.SelectedSlot.UserId :
                                                            model.SelectedSlot.ProviderId);
                model.SelectedSlot.Name += " - " + model.ProviderAddress;

                model.SelectedDate = DateTime.Parse(result.FirstAvailableDate);
                model.SelectedHour = result.SelectedHour;
                model.AvailableMinutesForSelectedHour = result.AvailableMinutesForSelectedHour;


                ModelState.AddModelError("",
                    @Resource.Error_AddSchedule);
                return View(model);

            }

            var isProvider = User.IsInRole("Provider");
            var scheduleStartDate = new DateTime(model.SelectedDate.Year, model.SelectedDate.Month,
                                                 model.SelectedDate.Day, model.SelectedHour, model.SelectedMinute, 0);

            if (model.SelectedOperationIds.Any())
            {
                var provider = RetrieveOthers.GetProviderById(model.ProviderId);
                var slot = provider.ProgrammingPerSlot
                    ? RetrieveOthers.GetSlotById(model.SelectedSlotId)
                    : RetrieveOthers.GetFirstAvailableSlot(RetrieveLists.GetSlotsByProvider(model.ProviderId),
                        scheduleStartDate, model.SelectedOperationIds);

                var slotOperations = RetrieveLists.GetSlotOperationsByIds(model.SelectedOperationIds, slot.Id);

                var s = new Schedule()
                {
                    CreatedDateTime = DateTime.Now,
                    ScheduleDateTimeStart = scheduleStartDate,
                    SlotId = slot.Id,
                    State = ScheduleState.Pending,
                    UserId = User.Identity.GetUserId(),
                    Text = model.Text,
                    AddedByProvider = isProvider,
                };
                s.ScheduleDateTimeEnd = scheduleStartDate.AddMinutes(slotOperations.Sum(x => x.DurationMinutes));

                var errorCode = RetrieveOthers.IsSchedulePossible(s);
                if (errorCode == 0) //Schedule is possible
                {
                    using (var db = new AppDbContext())
                    {
                        db.Schedules.Add(s);

                        foreach (var slotOperation in slotOperations)
                        {
                            db.ScheduleSlotOperations.Add(new ScheduleSlotOperation()
                            {
                                ScheduleId = s.Id,
                                SlotOperationId = slotOperation.Id
                            });
                        }

                        db.SaveChanges();

                        #region GOOGLE_CALENDAR
                        /*
                        var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                               new ClientSecrets
                               {
                                   ClientId = "502256246791-c68uakfblt5l27mbtaf5gp4a4mvmrlgl.apps.googleusercontent.com",
                                   ClientSecret = "QW4iXDxQwMyXaPba3-lwWyY9"
                               },
                               new[] { CalendarService.Scope.Calendar },
                               "user",
                               CancellationToken.None).Result;

                        var service = new CalendarService(new BaseClientService.Initializer
                        {
                            HttpClientInitializer = credential,
                            ApplicationName = "Plansy",
                        });
                        var myEvent = new Event
                        {
                            Summary = "Google Calendar Api Sample Code by Mukesh Salaria",
                            Location = "Gurdaspur, Punjab, India",
                            Start = new EventDateTime
                            {
                                DateTime = new DateTime(2015, 12, 11, 6, 0, 0),
                            },
                            End = new EventDateTime
                            {
                                DateTime = new DateTime(2015, 12, 11, 7, 30, 0),
                            }
                            //,Recurrence = new String[] { "RRULE:FREQ=WEEKLY;BYDAY=MO" }
                        };

                        var recurringEvent = service.Events.Insert(myEvent, "primary");
                        recurringEvent.SendNotifications = true;
                        recurringEvent.Execute();
                        */
                        #endregion

                        var client = db.Users.First(c => c.Id == s.UserId);

                        var n = RetrieveOthers.AddNotification(s.Id, slot.ProviderId, NotificationType.NewSchedule,
                            slot.Provider.ProgrammingPerSlot
                                ? Resource.Notification_NewAppointment + " (" + slot.Name + ") - " +
                                  s.ScheduleDateTimeStart.ToString(Thread.CurrentThread.CurrentCulture) +
                                  ", Client: " + client.FirstName + " " + client.LastName + " " + client.PhoneNumber
                                : Resource.Notification_NewAppointment +
                                  s.ScheduleDateTimeStart.ToString(Thread.CurrentThread.CurrentCulture) +
                                  ", Client: " + client.FirstName + " " + client.LastName + " " + client.PhoneNumber);
                        NotificationCenter.SendNotificationToUser(n.UserId,
                            RenderRazorViewToString("~/Views/Shared/PartialViews/_Notification.cshtml", n));

                        if (slot.Provider.PhoneNumber != null &&
                            slot.Provider.AcceptsNotificationOnSms &&
                            slot.Provider.PhoneNumberConfirmed)
                        {
                            MailAndSmsUtility.SendSms(slot.Provider.PhoneNumber, n.Text);
                        }

                        if (!slot.UserId.IsNullOrWhiteSpace())
                        {
                            var nn = RetrieveOthers.AddNotification(s.Id, slot.UserId, NotificationType.NewSchedule,
                                Resource.Notification_NewAppointment +
                                  s.ScheduleDateTimeStart.ToString(Thread.CurrentThread.CurrentCulture) +
                                  ", Client: " + client.FirstName + " " + client.LastName + " " + client.PhoneNumber);

                            NotificationCenter.SendNotificationToUser(nn.UserId,
                                RenderRazorViewToString("~/Views/Shared/PartialViews/_Notification.cshtml", nn));

                            if (slot.User.PhoneNumber != null &&
                                slot.User.AcceptsNotificationOnSms &&
                                slot.User.PhoneNumberConfirmed)
                            {
                                MailAndSmsUtility.SendSms(slot.User.PhoneNumber, nn.Text);
                            }
                        }
                    }
                }
                else if (errorCode == 1) //2 programari in acelasi timp
                {
                    var result = RetrieveLists.GetProviderSlotOperations(
               model.ProviderId,
               model.CategoryId,
               new List<int>(), //selectedOperations
               null, //providerParameters.SelectedDate,
               null,//providerParameters.SelectedHour,
               model.SelectedSlotId,
               User.Identity.GetUserId());

                    var ops = result.AvailableOperations//RetrieveLists.GetSlotOperationsBySlot(slot.Id)
                        .Select(o => new SelectListItem()
                        {
                            Value = o.Id.ToString(CultureInfo.InvariantCulture),
                            Text = o.DurationMinutes + Resource.Semicolon + o.Price + Resource.Semicolon +
                                    (o.Description.Length > 25 ? o.Description.Substring(0, 25) + "..." : o.Description)
                        })
                              .ToList();

                    model.Provider = RetrieveOthers.GetUserById(model.ProviderId);
                    model.AvailableSlots = result.AvailableSlots;
                    model.SelectedSlot = result.AvailableSlots.First();
                    model.Operations = ops;
                    model.SelectedOperationIds = result.SelectedOperationIds;//new List<int>(),
                    model.ProviderAddress = RetrieveOthers.GetUserAddress(!string.IsNullOrWhiteSpace(model.SelectedSlot.UserId) ?
                                                             model.SelectedSlot.UserId :
                                                             model.SelectedSlot.ProviderId);
                    model.SelectedSlot.Name += " - " + model.ProviderAddress;
                    model.SelectedDate = DateTime.Parse(result.FirstAvailableDate);
                    model.SelectedHour = result.SelectedHour;
                    model.AvailableMinutesForSelectedHour = result.AvailableMinutesForSelectedHour;

                    ModelState.AddModelError("", Resource.Error_Schedule1);
                    return View(model);
                }
                else //if (errorCode == 2) - doua programari la acelasi Provider in aceeasi zi
                {
                    var result = RetrieveLists.GetProviderSlotOperations(
                    model.ProviderId,
                    model.CategoryId,
                    new List<int>(), //selectedOperations
                    null, //providerParameters.SelectedDate,
                    null,//providerParameters.SelectedHour,
                    model.SelectedSlotId,
                    User.Identity.GetUserId());

                    var ops = result.AvailableOperations//RetrieveLists.GetSlotOperationsBySlot(slot.Id)
                        .Select(o => new SelectListItem()
                        {
                            Value = o.Id.ToString(CultureInfo.InvariantCulture),
                            Text = o.DurationMinutes + Resource.Semicolon + o.Price + Resource.Semicolon +
                                    (o.Description.Length > 25 ? o.Description.Substring(0, 25) + "..." : o.Description)
                        })
                                .ToList();

                    model.Provider = RetrieveOthers.GetUserById(model.ProviderId);
                    model.AvailableSlots = result.AvailableSlots;
                    model.SelectedSlot = result.AvailableSlots.First();
                    model.Operations = ops;
                    model.SelectedOperationIds = result.SelectedOperationIds;//new List<int>(),
                    model.ProviderAddress = RetrieveOthers.GetUserAddress(!string.IsNullOrWhiteSpace(model.SelectedSlot.UserId) ?
                                                            model.SelectedSlot.UserId :
                                                            model.SelectedSlot.ProviderId);
                    model.SelectedSlot.Name += " - " + model.ProviderAddress;
                    model.SelectedDate = DateTime.Parse(result.FirstAvailableDate);
                    model.SelectedHour = result.SelectedHour;
                    model.AvailableMinutesForSelectedHour = result.AvailableMinutesForSelectedHour;


                    ModelState.AddModelError("", Resource.Error_Schedule2);
                    return View(model);
                }
            }
            else
            {
                //model.SelectedSlot = slot;
                model.ProviderAddress = RetrieveOthers.GetUserAddress(!string.IsNullOrWhiteSpace(model.SelectedSlot.UserId) ?
                                                                            model.SelectedSlot.UserId :
                                                                            model.SelectedSlot.ProviderId);
                model.SelectedSlot.Name += " - " + model.ProviderAddress;

                model.Operations = RetrieveLists.GetSlotOperationsBySlot(model.SelectedSlot.Id).Select(o => new SelectListItem()
                {
                    Value = o.Id.ToString(CultureInfo.InvariantCulture),
                    Text = o.DurationMinutes + Resource.Semicolon + o.Price + Resource.Semicolon + o.Operation.Description
                }).ToList();

                ModelState.AddModelError("", Resource.Error_MandatoryFields);
                return View(model);
            }

            return isProvider ? RedirectToAction(MVC.Home.Index()) : RedirectToAction(MVC.Schedule.ClientViewSchedules());
        }

        [HttpGet]
        [CustomAuthorize(Roles = "Admin, Provider, Employee")]
        public virtual ActionResult AddSchedule(int slotId, string userId, int day, int month, int year, int? hour, int? minute)
        {
            if (userId == "null") userId = null;
            var date = day > 0 && month > 0 && year > 0 ? new DateTime(year, month, day) : DateTime.Now;
            var shortestOperationId =
                RetrieveLists.GetSlotOperationsBySlot(slotId).OrderBy(s => s.DurationMinutes).First().OperationId;
            var slot = RetrieveOthers.GetSlotById(slotId);
            var result = RetrieveLists.GetProviderSlotOperations(slot.ProviderId, slot.CategoryId,
                new List<int> { shortestOperationId }, date, null, slotId, userId, true);
            var addresses = RetrieveLists.GetUserAddresses(userId);
            ViewBag.Counties = RetrieveLists.GetCounties();

            if (!result.AvailableHours.Any())
            {
                result.AvailableHours.Add(DateTime.Now.AddHours(1).Hour);
                result.AvailableMinutesForSelectedHour.Add(0);
            }

            var model = new AddEventViewModel()
            {
                Schedule = new Schedule()
                {
                    SlotId = slotId,
                    CreatedDateTime = DateTime.Now,
                    ScheduleDateTimeStart = date,
                    Slot = slot,
                    State = ScheduleState.Valid,
                    AddedByProvider = true,
                },
                StartHour =
                    hour.HasValue && result.AvailableHours.Contains((int)hour)
                        ? (int)hour
                        : result.AvailableHours.First(),
                StartMinute =
                    minute.HasValue && result.AvailableMinutesForSelectedHour.Contains((int)minute)
                        ? (int)minute
                        : result.AvailableMinutesForSelectedHour.First(),
                EndHour = hour.HasValue && result.AvailableHours.Contains((int)hour + 1)
                        ? (int)hour + 1
                        : result.AvailableHours.First(),
                EndMinute = result.AvailableMinutesForSelectedHour.First(),

                AvailableHours = result.AvailableHours,
                AvailableMinutes = result.AvailableMinutesForSelectedHour,
                CategoryId = slot.CategoryId,
                ProviderId = slot.ProviderId,
                ShortestOperationId = shortestOperationId,
                Addresses = addresses
            };

            if (userId.IsNullOrWhiteSpace()) return View(model);
            var user = RetrieveOthers.GetUserById(userId);
            model.FirstName = user.FirstName;
            model.LastName = user.LastName;
            model.PhoneNumber = user.PhoneNumber;
            model.Email = user.Email;
            model.UserId = user.Id;

            return View(model);
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin, Provider, Employee")]
        public virtual ActionResult AddSchedule(AddEventViewModel model)
        {
            if (Common.IsPhoneNumber(model.PhoneNumber) && !model.FirstName.IsNullOrWhiteSpace() && !model.LastName.IsNullOrWhiteSpace())
            {
                using (var db = new AppDbContext())
                {
                    var startDate = new DateTime(model.Schedule.ScheduleDateTimeStart.Year,
                        model.Schedule.ScheduleDateTimeStart.Month,
                        model.Schedule.ScheduleDateTimeStart.Day, model.StartHour, model.StartMinute, 0);
                    var endDate = new DateTime(model.Schedule.ScheduleDateTimeStart.Year,
                        model.Schedule.ScheduleDateTimeStart.Month,
                        model.Schedule.ScheduleDateTimeStart.Day, model.EndHour, model.EndMinute, 0);
                    model.Schedule.ScheduleDateTimeStart = startDate;
                    model.Schedule.ScheduleDateTimeEnd = endDate;

                    if (startDate >= endDate)
                    {
                        ModelState.AddModelError(
                            "",
                            @Resource.Error_NoSchedulesBeforeToday);
                        var result = RetrieveLists.GetProviderSlotOperations(model.ProviderId, model.CategoryId,
                            new List<int>(model.ShortestOperationId), model.Schedule.ScheduleDateTimeStart, null, model.Schedule.SlotId,
                            model.UserId);
                        model.AvailableHours = result.AvailableHours;
                        model.AvailableMinutes = result.AvailableMinutesForSelectedHour;

                        return View(model);
                    }

                    if (model.Schedule.ScheduleDateTimeStart <= DateTime.Now)
                    {
                        ModelState.AddModelError(
                            "",
                            @Resource.Error_NoSchedulesBeforeToday);
                        var result = RetrieveLists.GetProviderSlotOperations(model.ProviderId, model.CategoryId,
                            new List<int>(model.ShortestOperationId), model.Schedule.ScheduleDateTimeStart, null, model.Schedule.SlotId,
                            model.UserId);
                        model.AvailableHours = result.AvailableHours;
                        model.AvailableMinutes = result.AvailableMinutesForSelectedHour;

                        return View(model);
                    }

                    model.Schedule.CreatedDateTime = DateTime.Now;
                    model.Schedule.Slot = null;

                    var user = RetrieveOthers.GetUserByPhoneNumber(model.PhoneNumber);
                    model.Schedule.UserId = user != null
                        ? user.Id
                        : CreateAnonymousUser(model.FirstName, model.LastName, model.PhoneNumber, model.Email/*, model.Addresses*/);
                    model.Schedule.State = ScheduleState.Valid;
                    model.Schedule.AddedByProvider = true;

                    db.Schedules.Add(model.Schedule);
                    db.SaveChanges();

                    var slot = RetrieveOthers.GetSlotById(model.Schedule.SlotId);
                    var name = slot.Provider.ProgrammingPerSlot ? slot.Name : slot.Provider.CompanyDisplayName;
                    var phone = slot.Provider.ProgrammingPerSlot ? slot.Phone : slot.Provider.PhoneNumber;

                    if (user != null)
                    {
                        var n = RetrieveOthers.AddNotification(model.Schedule.Id, user.Id,
                             NotificationType.NewSchedule,
                             Resource.Notification_Scheduled1 + " " + name + " (" +
                             model.Schedule.ScheduleDateTimeStart.ToString(Thread.CurrentThread.CurrentCulture) +
                             "). " + Resource.Notification_TwoHoursReminder2 + " " + phone);

                        NotificationCenter.SendNotificationToUser(n.UserId,
                            RenderRazorViewToString("~/Views/Shared/PartialViews/_Notification.cshtml", n));

                        if (user.PhoneNumber != null && user.AcceptsNotificationOnSms && user.PhoneNumberConfirmed)
                        {
                            MailAndSmsUtility.SendSms(user.PhoneNumber, n.Text);
                        }
                    }
                    else
                    {
                        MailAndSmsUtility.SendSms(model.PhoneNumber,
                            Resource.Notification_Scheduled1 + " " + name + " (" +
                            model.Schedule.ScheduleDateTimeStart.ToString(Thread.CurrentThread.CurrentCulture)
                            + Resource.Notification_Scheduled2 + ". " +
                            Resource.Notification_TwoHoursReminder2 + " " + phone
                            );
                    }
                }

                return RedirectToAction(MVC.Schedule.ViewSchedules(model.Schedule.SlotId));
            }

            var res = RetrieveLists.GetProviderSlotOperations(model.ProviderId, model.CategoryId,
               new List<int> { model.ShortestOperationId }, model.Schedule.ScheduleDateTimeStart, null,
               model.Schedule.SlotId, model.UserId, true);
            model.AvailableHours = res.AvailableHours;
            model.AvailableMinutes = res.AvailableMinutesForSelectedHour;

            ModelState.AddModelError("", Resource.Error_MandatoryFields);
            return View(model);
        }

        [HttpGet]
        [CustomAuthorize(Roles = "Admin, Client")]
        public virtual ActionResult AddToFavourites(int slotId, int scheduleId)
        {
            var s = RetrieveOthers.GetSlotById(slotId);

            var fav = new Favorite()
            {
                FavoriteUserId = s.ProviderId,
                UserId = User.Identity.GetUserId(),
            };

            if (s.Provider.ProgrammingPerSlot)
            {
                fav.FavoriteSlotId = slotId;
            }

            using (var db = new AppDbContext())
            {
                db.Favorites.Add(fav);
                db.SaveChanges();
            }

            return RedirectToAction(MVC.Schedule.ViewSchedule(scheduleId));
        }

        public virtual ActionResult Backend(int slotId)
        {
            return new Dpc(slotId).CallBack(this);
        }

        public virtual ActionResult BackendClient(string userId)
        {
            return new DpcClient(userId).CallBack(this);
        }

        [HttpGet]
        [CustomAuthorize(Roles = "Client, Admin, Provider, Employee")]
        public virtual ActionResult CancelSchedule(int id, bool isClient)
        {
            var schedule = RetrieveOthers.GetScheduleById(id);
            schedule.State = isClient ? ScheduleState.CancelledByUser : ScheduleState.CancelledByProvider;

            using (var db = new AppDbContext())
            {
                db.Schedules.Attach(schedule);
                db.Entry(schedule).State = EntityState.Modified;
                db.SaveChanges();
            }

            if (isClient)
            {
                var client = RetrieveOthers.GetUserById(schedule.UserId);
                var n = RetrieveOthers.AddNotification(schedule.Id, schedule.Slot.ProviderId,
                    NotificationType.Cancelation,
                    schedule.Slot.Provider.ProgrammingPerSlot
                        ? Resource.Notification_Cancel1 + schedule.Slot.Name + " (" +
                          schedule.ScheduleDateTimeStart.ToString(Thread.CurrentThread.CurrentCulture) + ") " +
                          Resource.Notification_Cancel3 + client.PhoneNumber
                        : Resource.Notification_Cancel2 +
                          schedule.ScheduleDateTimeStart.ToString(Thread.CurrentThread.CurrentCulture) +
                          Resource.Notification_Cancel3 + client.PhoneNumber);

                NotificationCenter.SendNotificationToUser(n.UserId,
                    RenderRazorViewToString("~/Views/Shared/PartialViews/_Notification.cshtml", n));

                if (schedule.Slot.Provider.PhoneNumber != null &&
                    schedule.Slot.Provider.AcceptsNotificationOnSms &&
                    schedule.Slot.Provider.PhoneNumberConfirmed)
                {
                    MailAndSmsUtility.SendSms(schedule.Slot.Provider.PhoneNumber, n.Text);
                }

                if (!schedule.Slot.UserId.IsNullOrWhiteSpace())
                {
                    var nn = RetrieveOthers.AddNotification(schedule.Id, schedule.Slot.UserId,
                        NotificationType.Cancelation,
                        Resource.Notification_Cancel2 + schedule.ScheduleDateTimeStart.ToString(Thread.CurrentThread.CurrentCulture) +
                        Resource.Notification_Cancel3 + client.PhoneNumber);

                    NotificationCenter.SendNotificationToUser(nn.UserId,
                        RenderRazorViewToString("~/Views/Shared/PartialViews/_Notification.cshtml", nn));

                    var user = RetrieveOthers.GetUserById(schedule.Slot.UserId);
                    if (user.PhoneNumber != null && user.AcceptsNotificationOnSms && user.PhoneNumberConfirmed)
                    {
                        MailAndSmsUtility.SendSms(user.PhoneNumber, n.Text);
                    }
                }
            }
            else
            {
                var name = schedule.Slot.Provider.ProgrammingPerSlot ? schedule.Slot.Name : schedule.Slot.Provider.CompanyDisplayName;
                var phone = schedule.Slot.Provider.ProgrammingPerSlot ? schedule.Slot.Phone : schedule.Slot.Provider.PhoneNumber;

                var n = RetrieveOthers.AddNotification(schedule.Id, schedule.UserId, NotificationType.Cancelation,
                    Resource.Notification_Validation1 + name + " (" +
                    schedule.ScheduleDateTimeStart.ToString(Thread.CurrentThread.CurrentCulture)
                    + ") " + phone);

                NotificationCenter.SendNotificationToUser(n.UserId,
                    RenderRazorViewToString("~/Views/Shared/PartialViews/_Notification.cshtml", n));

                if (schedule.User.PhoneNumber != null && schedule.User.AcceptsNotificationOnSms && schedule.User.PhoneNumberConfirmed)
                {
                    MailAndSmsUtility.SendSms(schedule.User.PhoneNumber, n.Text);
                }
            }

            return isClient
                ? RedirectToAction(MVC.Schedule.ClientViewSchedules())
                : RedirectToAction(MVC.Schedule.ViewPendingSchedules(schedule.SlotId));
        }

        [HttpGet]
        [CustomAuthorize(Roles = "Client, Admin")]
        public virtual ActionResult CategoriesList(int? parentCategoryId)
        {
            string searchText = null;
            int cityId = -1;

            HttpCookie cookieSearch = Request.Cookies["SearchText"];
            if (cookieSearch != null)
                searchText = cookieSearch.Value;

            HttpCookie cookieCity = Request.Cookies["CityId"];
            if (cookieCity != null)
                int.TryParse(cookieCity.Value, out cityId);

            if (!RetrieveOthers.IsCityIdValid(cityId))
            {
                cityId = -1;
                Response.Cookies.Remove("CityId");
            }


            if (parentCategoryId != null && parentCategoryId.HasValue)
            {
                ViewBag.CssIcon = RetrieveOthers.GetCategoryById((int)parentCategoryId).CssIcon;

                if (RetrieveLists.IsLeaf((int)parentCategoryId))
                    return RedirectToAction(MVC.Schedule.ProvidersList((int)parentCategoryId, cityId));
            }
            ViewBag.Categories = RetrieveLists.GetCategoriesFiltered(parentCategoryId, cityId, searchText);
            ViewBag.Cities = RetrieveLists.GetCitiesWithProviders(parentCategoryId);

            return
                View(new CategoryParameters()
                {
                    CityId = cityId,
                    SearchText = searchText,
                    ParentCategoryId = parentCategoryId
                });
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Client, Admin")]
        public virtual ActionResult CategoriesList(CategoryParameters model)
        {
            if (Request.Cookies["SearchText"] != null)
            {
                var cookieSearch = Request.Cookies["SearchText"];
                cookieSearch.Value = model.SearchText;
                Response.SetCookie(cookieSearch);
            }
            else
            {
                var cookieSearch = new HttpCookie("SearchText", model.SearchText);
                Response.Cookies.Add(cookieSearch);
            }

            if (Request.Cookies["CityId"] != null)
            {
                var cityId = Request.Cookies["CityId"];
                cityId.Value = model.CityId.ToString();
                Response.SetCookie(cityId);
            }
            else
            {
                var cityId = new HttpCookie("CityId", model.CityId.ToString());
                Response.Cookies.Add(cityId);
            }

            ViewBag.Categories = RetrieveLists.GetCategoriesFiltered(model.ParentCategoryId, model.CityId, model.SearchText);
            ViewBag.Cities = RetrieveLists.GetCitiesWithProviders(model.ParentCategoryId);

            return View(model);
        }

        [HttpGet]
        [CustomAuthorize(Roles = "Admin, Client")]
        public virtual ActionResult ClientViewSchedules(DateTime? date = null)
        {
            if (!date.HasValue)
            {
                date = DateTime.Now;
            }

            ViewBag.Date = date;
            var schedules = RetrieveLists.GetSchedulesByUser(User.Identity.GetUserId());

            var startDate = date;
            var endDate = date.Value.AddDays(6);

            schedules = schedules.Where(s => s.ScheduleDateTimeStart >= startDate && s.ScheduleDateTimeEnd <= endDate).ToList();

            return View(schedules);
        }

        public string CreateAnonymousUser(string firstName, string lastName, string phoneNumber, string email/*, IEnumerable<Address> addresses*/)
        {
            using (var db = new AppDbContext())
            {
                var au = new ApplicationUser()
                {
                    IsAnonymous = true,
                    PhoneNumber = phoneNumber,
                    LastName = lastName,
                    FirstName = firstName,
                    CreatedDate = DateTime.Now,
                    //Addresses = addresses.ToList()
                };

                using (var dbContextTransaction = db.Database.BeginTransaction(IsolationLevel.Serializable))
                {
                    au.UserName = email.IsNullOrWhiteSpace()
                        ? "anonymous" + Common.GetDbConfig("AnonymousCount") + "@mp.net"
                        : email;
                    au.Email = au.UserName;
                    IdentityResult adminresult = UserManager.Create(au);

                    //Add User to the selected Roles
                    if (adminresult.Succeeded && email.IsNullOrWhiteSpace())
                    {
                        var no = db.Configs.Find("AnonymousCount");
                        no.Value = ((Convert.ToInt32(no.Value) + 1)).ToString(CultureInfo.InvariantCulture);
                        db.Configs.Attach(no);
                        db.Entry(no).State = EntityState.Modified;
                    }

                    db.SaveChanges();
                    dbContextTransaction.Commit();
                }

                return au.Id;
            }
        }

        [HttpGet]
        [CustomAuthorize(Roles = "Admin, Client")]
        public virtual ActionResult DeleteFromFavourites(int slotId, int scheduleId)
        {
            using (var db = new AppDbContext())
            {
                var userId = User.Identity.GetUserId();
                var fav = db.Favorites.First(f => f.FavoriteSlotId == slotId && f.UserId == userId);

                db.Favorites.Remove(fav);
                db.SaveChanges();
            }

            return RedirectToAction(MVC.Schedule.ViewSchedule(scheduleId));
        }

        [HttpGet]
        [CustomAuthorize(Roles = "Admin")]
        public virtual ActionResult DeleteSchedule(int id, int slotId)
        {
            using (var db = new AppDbContext())
            {
                var s = db.Schedules.Find(id);
                db.Schedules.Remove(s);
                db.SaveChanges();
            }

            return RedirectToAction(MVC.Schedule.ViewSchedules(slotId));
        }

        [HttpGet]
        [CustomAuthorize(Roles = "Admin, Provider, Employee")]
        public virtual ActionResult EditSchedule(int id)
        {
            var s = RetrieveOthers.GetScheduleById(id);
            ViewBag.Operations = RetrieveLists.GetOperationsBySchedule(id);

            var shortestOperationId =
               RetrieveLists.GetSlotOperationsBySlot(s.SlotId).OrderBy(x => x.DurationMinutes).First().OperationId;
            var slot = RetrieveOthers.GetSlotById(s.SlotId);
            var result = RetrieveLists.GetProviderSlotOperations(slot.ProviderId, slot.CategoryId,
                new List<int> { shortestOperationId }, s.ScheduleDateTimeStart, null, s.SlotId, s.UserId, true);

            if (!result.AvailableHours.Any())
            {
                result.AvailableHours.Add(DateTime.Now.AddHours(1).Hour);
                result.AvailableMinutesForSelectedHour.Add(0);
            }

            var model = new AddEventViewModel()
            {
                Schedule = s,
                StartHour =
                    result.AvailableHours.Contains(s.ScheduleDateTimeStart.Hour)
                        ? s.ScheduleDateTimeStart.Hour
                        : result.AvailableHours.First(),
                StartMinute =
                    result.AvailableMinutesForSelectedHour.Contains(s.ScheduleDateTimeStart.Minute)
                        ? s.ScheduleDateTimeStart.Minute
                        : result.AvailableMinutesForSelectedHour.First(),
                EndHour = result.AvailableHours.Contains(s.ScheduleDateTimeEnd.Hour)
                        ? s.ScheduleDateTimeEnd.Hour
                        : result.AvailableHours.First(),
                EndMinute = result.AvailableMinutesForSelectedHour.Contains(s.ScheduleDateTimeEnd.Minute)
                        ? s.ScheduleDateTimeEnd.Minute
                        : result.AvailableMinutesForSelectedHour.First(),

                AvailableHours = result.AvailableHours,
                AvailableMinutes = result.AvailableMinutesForSelectedHour,
                CategoryId = slot.CategoryId,
                ProviderId = slot.ProviderId,
                ShortestOperationId = shortestOperationId,
                SendNotificationToClient = true
            };

            model.FirstName = s.User.FirstName;
            model.LastName = s.User.LastName;
            model.PhoneNumber = s.User.PhoneNumber;
            model.Email = s.User.Email;
            model.UserId = s.UserId;

            return View(model);
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin, Provider, Employee")]
        public virtual ActionResult EditSchedule(AddEventViewModel model)
        {
            if (Common.IsPhoneNumber(model.PhoneNumber) && !model.LastName.IsNullOrWhiteSpace() && !model.FirstName.IsNullOrWhiteSpace())
            {
                var s = RetrieveOthers.GetScheduleById(model.Schedule.Id);

                using (var db = new AppDbContext())
                {
                    var startDate = new DateTime(model.Schedule.ScheduleDateTimeStart.Year,
                        model.Schedule.ScheduleDateTimeStart.Month,
                        model.Schedule.ScheduleDateTimeStart.Day, model.StartHour, model.StartMinute, 0);
                    var endDate = new DateTime(model.Schedule.ScheduleDateTimeStart.Year,
                        model.Schedule.ScheduleDateTimeStart.Month,
                        model.Schedule.ScheduleDateTimeStart.Day, model.EndHour, model.EndMinute, 0);
                    s.ScheduleDateTimeStart = startDate;
                    s.ScheduleDateTimeEnd = endDate;
                    s.Text = model.Schedule.Text;

                    if (startDate >= endDate)
                    {
                        ModelState.AddModelError(
                            "",
                            Resource.Error_EndBeforeStart);
                        var result = RetrieveLists.GetProviderSlotOperations(model.ProviderId, model.CategoryId,
                            new List<int> { model.ShortestOperationId }, s.ScheduleDateTimeStart, null,
                            model.Schedule.SlotId, model.UserId);
                        model.AvailableHours = result.AvailableHours;
                        model.AvailableMinutes = result.AvailableMinutesForSelectedHour;

                        return View(model);
                    }

                    if (s.ScheduleDateTimeStart <= DateTime.Now)
                    {
                        ModelState.AddModelError(
                            "",
                            Resource.Error_NoSchedulesBeforeToday);
                        var result = RetrieveLists.GetProviderSlotOperations(model.ProviderId, model.CategoryId,
                            new List<int> { model.ShortestOperationId }, s.ScheduleDateTimeStart, null,
                            model.Schedule.SlotId, model.UserId);
                        model.AvailableHours = result.AvailableHours;
                        model.AvailableMinutes = result.AvailableMinutesForSelectedHour;

                        return View(model);
                    }

                    if (s.User.IsAnonymous &&
                        (s.User.LastName != model.LastName ||
                         s.User.FirstName != model.FirstName ||
                         s.User.PhoneNumber != model.PhoneNumber ||
                         (!model.Email.IsNullOrWhiteSpace() && s.User.Email != model.Email)))
                    {
                        s.User.FirstName = model.FirstName;
                        s.User.LastName = model.LastName;
                        s.User.PhoneNumber = model.PhoneNumber;
                        if (!model.Email.IsNullOrWhiteSpace())
                            s.User.Email = model.Email;
                        db.Users.Attach(s.User);
                        db.Entry(s.User).State = EntityState.Modified;
                    }

                    s.Slot = null;
                    db.Schedules.Attach(s);
                    db.Entry(s).State = EntityState.Modified;
                    db.SaveChanges();

                    if (!s.User.IsAnonymous && model.SendNotificationToClient)
                    {
                        var slot = RetrieveOthers.GetSlotById(s.SlotId);
                        var name = slot.Provider.ProgrammingPerSlot ? slot.Name : slot.Provider.CompanyDisplayName;
                        var phone = slot.Provider.ProgrammingPerSlot ? slot.Phone : slot.Provider.PhoneNumber;

                        var n = RetrieveOthers.AddNotification(s.Id, s.UserId,
                            NotificationType.ReSchedule,
                            Resource.Reschedule + name + " (" +
                            s.ScheduleDateTimeStart.ToString(Thread.CurrentThread.CurrentCulture) +
                            "). " + Resource.Notification_TwoHoursReminder2 + phone);

                        NotificationCenter.SendNotificationToUser(n.UserId,
                            RenderRazorViewToString("~/Views/Shared/PartialViews/_Notification.cshtml", n));

                        var user = RetrieveOthers.GetUserById(s.UserId);
                        if (user.PhoneNumber != null && user.AcceptsNotificationOnSms && user.PhoneNumberConfirmed)
                        {
                            MailAndSmsUtility.SendSms(user.PhoneNumber, n.Text);
                        }
                    }
                }
                return RedirectToAction(MVC.Schedule.ViewSchedules(s.SlotId));
            }

            var res = RetrieveLists.GetProviderSlotOperations(model.ProviderId, model.CategoryId,
                new List<int> { model.ShortestOperationId }, model.Schedule.ScheduleDateTimeStart, null,
                model.Schedule.SlotId, model.UserId, true);
            model.AvailableHours = res.AvailableHours;
            model.AvailableMinutes = res.AvailableMinutesForSelectedHour;

            ViewBag.Operations = RetrieveLists.GetOperationsBySchedule(model.Schedule.Id);
            ModelState.AddModelError("", Resource.Error_MandatoryFields);
            return View(model);
        }

        // GET: Schedule
        public virtual ActionResult Index()
        {
            return View(RetrieveLists.GetCategoriesFiltered(null, null));
        }

        [HttpGet]
        [CustomAuthorize(Roles = "Client, Admin")]
        public virtual ActionResult ProvidersList(int categoryId, int? cityId)
        {
            ViewBag.Providers = RetrieveLists.GetProvidersFiltered(categoryId, cityId <= 0 ? null : cityId, 0, 20);
            ViewBag.Cities = RetrieveLists.GetCitiesWithProviders(categoryId);

            return View(new CategoryParameters() { CategoryId = categoryId, CityId = cityId });
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Client, Admin")]
        public virtual ActionResult ProvidersList(CategoryParameters model)
        {
            ViewBag.Providers = RetrieveLists.GetProvidersFiltered(model.CategoryId, model.CityId, 0, 20, model.SearchText);
            ViewBag.Cities = RetrieveLists.GetCitiesWithProviders(model.CategoryId);

            return View(model);
        }

        [HttpGet]
        [CustomAuthorize(Roles = "Admin, Provider, Employee")]
        public virtual ActionResult ViewPendingSchedules(int slotId)
        {
            return View(RetrieveLists.GetSchedulesByProviderSlot(slotId, true));
        }

        //[HttpGet]
        //[CustomAuthorize(Roles = "Admin, Provider")]
        //public virtual ActionResult ViewPendingSchedules(string providerId)
        //{
        //    return View(RetrieveLists.GetSchedulesByProvider(providerId, true));
        //}

        [HttpGet]
        [CustomAuthorize(Roles = "Admin, Agent, Client, Provider, Employee")]
        public virtual ActionResult ViewSchedule(int id)
        {
            var s = RetrieveOthers.GetScheduleById(id);
            ViewBag.Address = RetrieveOthers.GetUserAddress(s.Slot.ProviderId);
            ViewBag.Operations = RetrieveLists.GetOperationsBySchedule(id);

            ViewBag.IsFavourite = s.Slot.Provider.ProgrammingPerSlot
                ? RetrieveOthers.IsFavourite(User.Identity.GetUserId(), s.SlotId, null)
                : RetrieveOthers.IsFavourite(User.Identity.GetUserId(), 0, s.Slot.ProviderId);

            var rating = s.Slot.Provider.ProgrammingPerSlot
                ? RetrieveOthers.GetRating(User.Identity.GetUserId(), s.SlotId, null)
                : RetrieveOthers.GetRating(User.Identity.GetUserId(), 0, s.Slot.ProviderId);
            ViewBag.Rating = rating == null ? 0 : rating.Score;

            return View(s);
        }

        [HttpGet]
        [CustomAuthorize(Roles = "Admin, Agent")]
        public virtual ActionResult SchedulesList(string userId, int? page)
        {
            var model = new List<Schedule>();
            ViewBag.Page = page.HasValue ? page.Value - 1 : 0;

            using (var db = new AppDbContext())
            {
                if (UserRoleHelper.UserHasRole(userId, "Provider"))
                {
                    model =
                        db.Schedules.Include("Slot")
                            .Include("Slot.Provider")
                            .Include("User").Where(x => x.Slot.ProviderId == userId)
                            .OrderByDescending(y => y.ScheduleDateTimeStart).ToList();
                }
                else if (UserRoleHelper.UserHasRole(userId, "Employee"))
                {
                    model =
                        db.Schedules.Include("Slot")
                            .Include("Slot.Provider")
                            .Include("User").Where(x => x.Slot.UserId == userId)
                            .OrderByDescending(y => y.ScheduleDateTimeStart).ToList();
                }
                else if (UserRoleHelper.UserHasRole(userId, "Client"))
                {
                    model =
                        db.Schedules.Include("Slot")
                            .Include("Slot.Provider")
                            .Include("User")
                            .Where(x => x.UserId == userId)
                            .OrderByDescending(y => y.ScheduleDateTimeStart).ToList();
                }
                else if (userId == null)
                {
                    model =
                        db.Schedules.Include("Slot")
                            .Include("Slot.Provider")
                            .Include("User").OrderByDescending(y => y.ScheduleDateTimeStart).ToList();
                }
            }

            return View(model.ToPagedList((int)ViewBag.Page, 10));
        }


        [HttpGet]
        [CustomAuthorize(Roles = "Admin, Provider, Employee")]
        public virtual ActionResult ViewSchedules(int slotId, DateTime? date = null)
        {
            var slot = RetrieveOthers.GetSlotById(slotId);

            if (!date.HasValue)
            {
                date = DateTime.Now;
            }

            ViewBag.Date = date;
            var schedules = RetrieveLists.GetSchedulesByProviderSlot(slotId);

            var startDate = date;
            var endDate = date.Value.AddDays(6);

            schedules = schedules.Where(s => s.ScheduleDateTimeStart >= startDate && s.ScheduleDateTimeEnd <= endDate).ToList();

            ViewBag.SlotId = slotId;
            ViewBag.SlotName = slot.Name;

            return View(schedules);
        }

        [HttpGet]
        [CustomAuthorize(Roles = "Admin, Provider, Employee")]
        public virtual ActionResult ViewAllSlotsSchedules(DateTime? date = null)
        {
            var slots = RetrieveLists.GetSlotsByProvider(User.Identity.GetUserId());

            if (!date.HasValue)
            {
                date = DateTime.Now;
            }

            ViewBag.Date = date;

            if (!slots.Any()) return RedirectToAction(MVC.Home.Index());
            var model = (from slot in slots
                         let schedules = RetrieveLists.GetSchedulesByDateAndSlot(slot.Id, (DateTime)date)
                         select new SlotSchedulesViewModel() { Schedules = schedules, Slot = slot }).ToList();

            return View(model);
        }

        #region GoogleCalendar

        CalendarService calService;
        private const string calID = "xxxxxxxxx...@group.calendar.google.com";
        private const string UserId = "user-id";
        private static string gFolder = System.Web.HttpContext.Current.Server.MapPath("/App_Data/MyGoogleStorage");

        public void Authenticate()
        {
            CalendarService service = null;

            IAuthorizationCodeFlow flow = new GoogleAuthorizationCodeFlow(
                new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = GetClientConfiguration().Secrets,
                    DataStore = new FileDataStore(gFolder),
                    Scopes = new[] { CalendarService.Scope.Calendar }
                });

            var uri = Request.Url.ToString();
            var code = Request["code"];
            if (code != null)
            {
                var token = flow.ExchangeCodeForTokenAsync(UserId, code,
                    uri.Substring(0, uri.IndexOf("?")), CancellationToken.None).Result;

                // Extract the right state.
                var oauthState = AuthWebUtility.ExtracRedirectFromState(
                    flow.DataStore, UserId, Request["state"]).Result;
                Response.Redirect(oauthState);
            }
            else
            {
                var result = new AuthorizationCodeWebApp(flow, uri, uri).AuthorizeAsync(UserId,
                    CancellationToken.None).Result;
                if (result.RedirectUri != null)
                {
                    // Redirect the user to the authorization server.
                    Response.Redirect(result.RedirectUri);
                }
                else
                {
                    // The data store contains the user credential, so the user has been already authenticated.
                    service = new CalendarService(new BaseClientService.Initializer
                    {
                        ApplicationName = "Plansy",
                        HttpClientInitializer = result.Credential
                    });
                }
            }

            calService = service;
        }

        public static GoogleClientSecrets GetClientConfiguration()
        {
            using (var stream = new FileStream(gFolder + @"\client_secrets.json", FileMode.Open, FileAccess.Read))
            {
                return GoogleClientSecrets.Load(stream);
            }
        }

        public string CreateUpdateEvent(string ExpKey, string ExpVal, string evTitle, string evDate)
        {
            EventsResource er = new EventsResource(calService);
            var queryEvent = er.List(calID);
            queryEvent.SharedExtendedProperty = ExpKey + "=" + ExpVal; //"EventKey=9999"
            var EventsList = queryEvent.Execute();

            Event ev = new Event();
            EventDateTime StartDate = new EventDateTime();
            StartDate.Date = evDate; //"2014-11-17";
            EventDateTime EndDate = new EventDateTime();
            EndDate.Date = evDate;

            ev.Start = StartDate;
            ev.End = EndDate;
            ev.Summary = evTitle; //"My Google Calendar V3 Event!";

            string FoundEventID = String.Empty;
            foreach (var evItem in EventsList.Items)
            {
                FoundEventID = evItem.Id;
            }

            if (String.IsNullOrEmpty(FoundEventID))
            {
                //If event does not exist, Append Extended Property and create the event
                Event.ExtendedPropertiesData exp = new Event.ExtendedPropertiesData();
                exp.Shared = new Dictionary<string, string>();
                exp.Shared.Add(ExpKey, ExpVal);
                ev.ExtendedProperties = exp;
                return er.Insert(ev, calID).Execute().Summary;
            }
            else
            {
                //If existing, Update the event
                return er.Update(ev, calID, FoundEventID).Execute().Summary;
            }
        }

        public bool DeleteEvent(string ExpKey, string ExpVal)
        {
            EventsResource er = new EventsResource(calService);
            var queryEvent = er.List(calID);

            queryEvent.SharedExtendedProperty = ExpKey + "=" + ExpVal; //"EventKey=9999"
            var EventsList = queryEvent.Execute();

            string FoundEventID = String.Empty;
            foreach (Event ev in EventsList.Items)
            {
                FoundEventID = ev.Id;
                er.Delete(calID, FoundEventID).Execute();
                return true;
            }

            return false;
        }

        #endregion


        #endregion Public Methods

    }
}