using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Data.Entity;
using MaProgramez.Repository.BusinessLogic;
using MaProgramez.Repository.DbContexts;
using MaProgramez.Repository.Entities;
using MaProgramez.Resources;
using MaProgramez.Website.Utility;
using MaProgramez.Website.ViewModels;

namespace MaProgramez.Website.AutomaticProcessingService.Jobs
{
    public class TwoHoursReminderJob : BaseJob
    {
        #region Constructor

        /// <summary>
        ///     Initializes a new instance of the <see cref="TwoHoursReminderJob" /> class.
        /// </summary>
        /// <param name="job"></param>
        public TwoHoursReminderJob(AutomaticProcessingJob job)
            : base(job)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("nl-NL");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("nl-NL");
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("nl-NL");
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("nl-NL");
        }

        #endregion Constructor

        #region Overrides

        /// <summary>
        ///     Override this method in child classes and add Job logic
        /// </summary>
        /// <param name="param">The parameter.</param>
        protected override void Execute(string param)
        {
            try
            {
                var count = 0;
                using (var db = new AppDbContext())
                {
                    var now = DateTime.Now;

                    var schedules =
                        from r in db.Schedules.Include("User").Include("Slot.Provider")
                        where r.State == ScheduleState.Valid &&
                              DbFunctions.DiffMinutes(r.ScheduleDateTimeStart, now) > 109 &&
                              DbFunctions.DiffMinutes(r.ScheduleDateTimeStart, now) < 120
                        select r;

                    foreach (var schedule in schedules)
                    {
                        var name = schedule.Slot.Provider.ProgrammingPerSlot
                            ? schedule.Slot.Provider.CompanyDisplayName + " - " + schedule.Slot.Name
                            : schedule.Slot.Provider.CompanyDisplayName;
                        var phone = schedule.Slot.Provider.ProgrammingPerSlot
                           ? schedule.Slot.Phone
                           : schedule.Slot.Provider.PhoneNumber;
                        var notification = RetrieveOthers.AddNotification(schedule.Id, schedule.UserId,
                            NotificationType.Reminder,
                            Resource.Notification_TwoHoursReminder1 + name +
                            Resource.Notification_TwoHoursReminder2 + phone);

                        db.Notifications.Add(notification);

                        NotificationCenter.SendNotificationToUser(notification.UserId, notification.Text);

                        //send only sms (e-mail makes no sense here)
                        if (schedule.User.PhoneNumber != null && schedule.User.AcceptsNotificationOnSms &&
                            schedule.User.PhoneNumberConfirmed)
                        {
                            MailAndSmsUtility.SendSms(schedule.User.PhoneNumber, notification.Text);
                        }

                        count++;
                    }

                    if (count > 0)
                    {
                        db.SaveChanges();
                        LogJobRun(true, count + " reminders sent.");
                    }
                }
            }
            catch (Exception ex)
            {
                LogJobRun(false, ex.StackTrace);
            }
        }

        #endregion Overrides
    }
}