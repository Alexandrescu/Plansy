using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using MaProgramez.Repository.BusinessLogic;
using MaProgramez.Repository.DbContexts;
using MaProgramez.Repository.Entities;
using MaProgramez.Resources;
using MaProgramez.Website.Utility;

namespace MaProgramez.Website.AutomaticProcessingService.Jobs
{
    public class ScheduleValidationJob : BaseJob
    {
        #region Constructor

        /// <summary>
        ///     Initializes a new instance of the <see cref="ScheduleValidationJob" /> class.
        /// </summary>
        /// <param name="job"></param>
        public ScheduleValidationJob(AutomaticProcessingJob job)
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
                var minutes = Common.GetDbConfig("DefaultAppointmentAcceptDelay").ToInteger();
                var now = DateTime.Now;

                using (var db = new AppDbContext())
                {
                    var schedules =
                        db.Schedules.Include("User").Include("Slot.Provider").
                            Where(r => r.State == ScheduleState.Pending &&
                                       DbFunctions.DiffMinutes(r.CreatedDateTime, now) >= minutes).ToList();

                    foreach (var schedule in schedules)
                    {
                        schedule.State = ScheduleState.Valid;
                        db.Schedules.Attach(schedule);
                        db.Entry(schedule).State = EntityState.Modified;

                        var name = schedule.Slot.Provider.ProgrammingPerSlot
                            ? schedule.Slot.Provider.CompanyDisplayName + " - " + schedule.Slot.Name
                            : schedule.Slot.Provider.CompanyDisplayName;
                        var phone = schedule.Slot.Provider.ProgrammingPerSlot
                            ? schedule.Slot.Phone
                            : schedule.Slot.Provider.PhoneNumber;
                        var notification = RetrieveOthers.AddNotification(schedule.Id, schedule.UserId,
                            NotificationType.Confirmation,
                            Resource.Notification_Validation1 + name + " (" +
                            schedule.ScheduleDateTimeStart.ToString(Thread.CurrentThread.CurrentCulture) +
                            ") " + Resource.Notification_Validation2 + phone);

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
                        LogJobRun(true, count + " validated appointments");
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