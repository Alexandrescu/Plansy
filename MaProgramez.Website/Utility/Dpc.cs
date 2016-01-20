using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Web;
using DayPilot.Web.Mvc;
using DayPilot.Web.Mvc.Enums;
using DayPilot.Web.Mvc.Events.Calendar;
using MaProgramez.Repository.BusinessLogic;
using MaProgramez.Repository.DbContexts;
using MaProgramez.Repository.Entities;
using MaProgramez.Resources;

namespace MaProgramez.Website.Utility
{
    class Dpc : DayPilotCalendar
    {
        private readonly int _id;
        private readonly AppDbContext _db = new AppDbContext();

        public Dpc(int id)
        {
            _id = id;
        }

        protected override void OnInit(InitArgs e)
        {
            Update(CallBackUpdateType.Full);
        }

        protected override void OnEventClick(EventClickArgs e)
        {
            Redirect("/ro/Schedule/EditSchedule/" + e.Id);
        }

        protected override void OnEventResize(EventResizeArgs e)
        {
            int id;
            if (!int.TryParse(e.Id, out id)) return;

            var toBeResized = RetrieveOthers.GetScheduleById(id);
            toBeResized.ScheduleDateTimeStart = e.NewStart;
            toBeResized.ScheduleDateTimeEnd = e.NewEnd;

            _db.Schedules.Attach(toBeResized);
            _db.Entry(toBeResized).State = EntityState.Modified;
            _db.SaveChanges();
            
            Update();

            #region ADD_NOTIFICATION

            if (!toBeResized.User.IsAnonymous)
            {
                var slot = RetrieveOthers.GetSlotById(toBeResized.SlotId);
                var name = slot.Provider.ProgrammingPerSlot ? slot.Name : slot.Provider.CompanyDisplayName;
                var n = RetrieveOthers.AddNotification(toBeResized.Id, toBeResized.UserId,
                    NotificationType.ReSchedule,
                    Resource.Reschedule + name + " (" +
                    toBeResized.ScheduleDateTimeStart.ToString(Thread.CurrentThread.CurrentCulture) +
                    "). ");

                NotificationCenter.SendNotificationToUser(n.UserId, n.Text);

                var user = RetrieveOthers.GetUserById(toBeResized.UserId);
                if (user.PhoneNumber != null && user.AcceptsNotificationOnSms && user.PhoneNumberConfirmed)
                {
                    MailAndSmsUtility.SendSms(user.PhoneNumber, n.Text);
                }
            }

            #endregion
        }

        protected override void OnEventMove(EventMoveArgs e)
        {
            int id;
            if (!int.TryParse(e.Id, out id)) return;

            var toBeResized = RetrieveOthers.GetScheduleById(id);
            toBeResized.ScheduleDateTimeStart = e.NewStart;
            toBeResized.ScheduleDateTimeEnd = e.NewEnd;

            _db.Schedules.Attach(toBeResized);
            _db.Entry(toBeResized).State = EntityState.Modified;
            _db.SaveChanges();

            Update();

            #region ADD_NOTIFICATION

            if (!toBeResized.User.IsAnonymous)
            {
                var slot = RetrieveOthers.GetSlotById(toBeResized.SlotId);
                var name = slot.Provider.ProgrammingPerSlot ? slot.Name : slot.Provider.CompanyDisplayName;
                var n = RetrieveOthers.AddNotification(toBeResized.Id, toBeResized.UserId,
                    NotificationType.ReSchedule,
                    Resource.Reschedule + name + " (" +
                    toBeResized.ScheduleDateTimeStart.ToString(Thread.CurrentThread.CurrentCulture) +
                    "). ");

                NotificationCenter.SendNotificationToUser(n.UserId, n.Text);

                var user = RetrieveOthers.GetUserById(toBeResized.UserId);
                if (user.PhoneNumber != null && user.AcceptsNotificationOnSms && user.PhoneNumberConfirmed)
                {
                    MailAndSmsUtility.SendSms(user.PhoneNumber, n.Text);
                }
            }

            #endregion
        }

        protected override void OnTimeRangeSelected(TimeRangeSelectedArgs e)
        {
            Redirect("/ro/Schedule/AddSchedule?slotId=" + _id + "&userId=null&day=" + e.Start.Day +
                     "&month=" + e.Start.Month + "&year=" + e.Start.Year + "&hour=" + e.Start.Hour + "&minute=" +
                     e.Start.Minute);
        }

        protected override void OnFinish()
        {
            if (UpdateType == CallBackUpdateType.None)
            {
                return;
            }

            Events = RetrieveLists.GetSchedulesByProviderSlot(_id, false);

            DataIdField = "id";
            DataTextField = "providerView";
            DataStartField = "scheduleDateTimeStart";
            DataEndField = "scheduleDateTimeEnd";
        }
    }
}