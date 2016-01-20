using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MaProgramez.Repository.DbContexts;
using MaProgramez.Resources;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
//using Microsoft.Office.Interop.Excel;
using MaProgramez.Website.Extensions;
using MaProgramez.Website.Helpers;
using MaProgramez.Website.Utility;
using MaProgramez.Website.ViewModels;
using MaProgramez.Repository.Entities;

namespace MaProgramez.Website.Controllers
{
    [CustomAuthorize]
    public partial class NotificationController : BaseController
    {
        #region ACTIONS

        // GET: Notification
        public virtual ActionResult List()
        {
            using (var db = new AppDbContext())
            {
                var userId = User.Identity.GetUserId();
                var notifications = db.Notifications.Where(n => n.UserId == userId && n.IsDeleted == false).OrderByDescending(n => n.CreatedDate).ToList();

                return View(notifications);
            }
        }

        [HttpGet]
        public virtual ActionResult MarkAsReadCanceledOfferNotifs(int id)
        {
            var userId = User.Identity.GetUserId();
            using (var db = new AppDbContext())
            {
                var notifications =
                    (from n in db.Notifications
                     where n.UserId == userId && n.ScheduleId == id && n.Type == NotificationType.Cancelation
                     select n).ToList();
                if (notifications.Any())
                {
                    foreach (var notification in notifications)
                    {
                        notification.IsRead = true;
                        db.Notifications.Attach(notification);
                        db.Entry(notification).State = EntityState.Modified;
                    }

                    if (db.SaveChanges() < 0)
                    {
                        return RedirectToAction(MVC.Error.Index());
                    }
                }
            }

            return RedirectToAction(MVC.Home.Index(id, ""));
        }

        [HttpGet]
        public virtual ActionResult MarkAsReadNotifs(int id)
        {
            using (var db = new AppDbContext())
            {
                var notif = db.Notifications.Find(id);
                if (notif != null)
                {
                    notif.IsRead = true;
                    db.Notifications.Attach(notif);
                    db.Entry(notif).State = EntityState.Modified;

                    if (db.SaveChanges() < 0)
                    {
                        return RedirectToAction(MVC.Error.Index());
                    }

                    switch (notif.Type)
                    {
                        case NotificationType.NewSchedule:
                        case NotificationType.ReSchedule:
                        case NotificationType.Confirmation:
                        case NotificationType.Cancelation:
                        case NotificationType.Reminder:
                            return RedirectToAction(MVC.Schedule.ViewSchedule(notif.ScheduleId));
                    }
                }

                return RedirectToAction("List");
            }
        }

        [HttpGet]
        public virtual ActionResult DeleteAll()
        {
            using (var db = new AppDbContext())
            {
                var userId = User.Identity.GetUserId();

                var notifs = db.Notifications.Where(n => n.UserId == userId && n.IsDeleted == false);

                if (notifs.Any())
                {
                    foreach (var notif in notifs)
                    {
                        notif.IsDeleted = true;
                        db.Notifications.Attach(notif);
                        db.Entry(notif).State = EntityState.Modified;
                    }

                    if (db.SaveChanges() < 0)
                    {
                        return RedirectToAction(MVC.Error.Index());
                    }
                }

                return RedirectToAction("List");
            }
        }

        [HttpGet]
        public virtual ActionResult ReadAll()
        {
            using (var db = new AppDbContext())
            {
                var userId = User.Identity.GetUserId();

                var notifs = db.Notifications.Where(n => n.UserId == userId && n.IsDeleted == false && n.IsRead == false);

                if (notifs.Any())
                {
                    foreach (var notif in notifs)
                    {
                        notif.IsRead = true;
                        db.Notifications.Attach(notif);
                        db.Entry(notif).State = EntityState.Modified;
                    }

                    if (db.SaveChanges() < 0)
                    {
                        return RedirectToAction(MVC.Error.Index());
                    }
                }

                return RedirectToAction("List");
            }
        }

        [HttpGet]
        public virtual ActionResult Delete(int id)
        {
            using (var db = new AppDbContext())
            {
                var notif = db.Notifications.Find(id);
                if (notif != null)
                {
                    notif.IsDeleted = true;
                    db.Notifications.Attach(notif);
                    db.Entry(notif).State = EntityState.Modified;

                    if (db.SaveChanges() < 0)
                    {
                        return RedirectToAction(MVC.Error.Index());
                    }
                }
                return RedirectToAction("List");
            }
        }

        [CustomAuthorize(Roles = "Admin, Agent")]
        public virtual ActionResult SendMessage()
        {
            return this.View();
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin, Agent")]
        public virtual ActionResult SendMessage(string message, string emails, string phoneNumbers, bool? sendEmail, bool? sendSms, bool? sendToSuppliers, bool? sendToClients)
        {
            var success = false;
            if (!string.IsNullOrWhiteSpace(message))
            {
                List<ApplicationUser> suppliers = null;
                List<ApplicationUser> clients = null;

                // GET SUPPLIERS
                if (sendToSuppliers.HasValue && sendToSuppliers.Value)
                {
                    using (var db = new AppDbContext())
                    {
                        var role = db.Roles.First(x => x.Name == "Dezmembrator");
                        suppliers = db.Users.Where(x => x.Roles.Any(r => r.RoleId == role.Id)).ToList();
                    }
                }

                // GET CLIENTS
                if (sendToClients.HasValue && sendToClients.Value)
                {
                    using (var db = new AppDbContext())
                    {
                        var roles = db.Roles.Where(x => x.Name == "Client" || x.Name == "Provider").Select(x => x.Id).ToList();
                        clients = db.Users.Where(x => x.Roles.Any(role => roles.Any(y => y == role.RoleId))).ToList();
                    }
                }

                // SEND EMAILS
                if (sendEmail.HasValue && sendEmail.Value)
                {
                    if (!string.IsNullOrWhiteSpace(emails) || (suppliers != null && suppliers.Any()) || (clients != null && clients.Any()))
                    {
                        var model = new MailViewModel
                        {
                            Title = Resource.MessageFromMaProgramez,
                            Content = message,
                            Footer = Resource.NoReply
                        };
                        var body = MvcUtility.RenderRazorViewToString(this, MVC.Mail.Views.GenericMail, model);

                        if (!string.IsNullOrWhiteSpace(emails))
                        {
                            var emailList = emails.Split(';').ToList();
                            emailList = emailList.Select(x => x.Trim()).ToList();

                            foreach (var email in emailList)
                            {
                                MailAndSmsUtility.SendEmail(email, Resource.MessageFromMaProgramez, body);
                            }
                        }

                        if (suppliers != null && suppliers.Any())
                        {
                            foreach (var supplier in suppliers)
                            {
                                MailAndSmsUtility.SendEmail(supplier.Email, Resource.MessageFromMaProgramez, body);
                            }
                        }

                        if (clients != null && clients.Any())
                        {
                            foreach (var client in clients)
                            {
                                MailAndSmsUtility.SendEmail(client.Email, Resource.MessageFromMaProgramez, body);
                            }
                        }
                    }
                    success = true;
                }

                // SEND SMS
                if (sendSms.HasValue && sendSms.Value)
                {
                    if (!string.IsNullOrWhiteSpace(phoneNumbers) || (suppliers != null && suppliers.Any()) || (clients != null && clients.Any()))
                    {
                        if (!string.IsNullOrWhiteSpace(phoneNumbers))
                        {
                            var numbers = phoneNumbers.Split(';').ToList();
                            numbers = numbers.Select(x => x.Trim()).ToList();

                            foreach (var number in numbers)
                            {
                                MailAndSmsUtility.SendSms(number, message);
                            }
                        }

                        if (suppliers != null && suppliers.Any())
                        {
                            foreach (var supplier in suppliers)
                            {
                                MailAndSmsUtility.SendSms(supplier.PhoneNumber, message);
                            }
                        }

                        if (clients != null && clients.Any())
                        {
                            foreach (var client in clients)
                            {
                                MailAndSmsUtility.SendSms(client.PhoneNumber, message);
                            }
                        }
                    }
                    success = true;
                }
            }


            if (success)
            {
                var confirmation = new ConfirmationViewModel
                {
                    Title = Resource.TheMessageHasBeenSent,
                    Message = Resource.TheMessageHasBeenSentMessage,
                    Link = Url.Action(MVC.Home.Index()),
                    Type = ConfirmationType.Success
                };


                return this.RedirectToConfirmation(confirmation);
            }

            var confirmationError = new ConfirmationViewModel
            {
                Title = Resource.TheMessageHasNotBeenSent,
                Message = Resource.TheMessageHasNotBeenSentMessage,
                Link = Url.Action(MVC.Home.Index()),
                Type = ConfirmationType.Error
            };

            return this.RedirectToConfirmation(confirmationError);
        }

        #endregion
    }
}