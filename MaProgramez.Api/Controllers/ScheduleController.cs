using MaProgramez.Repository.BusinessLogic;
using MaProgramez.Repository.DbContexts;
using MaProgramez.Repository.Entities;
using MaProgramez.Repository.Models;
using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web.Http;

namespace MaProgramez.Api.Controllers
{
    [RoutePrefix("api/Schedules")]
    public class ScheduleController : ApiController
    {
        #region Public Constructors

        public ScheduleController()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("nl-NL");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("nl-NL");
        }

        #endregion Public Constructors

        #region Public Methods

        [Route("CancelSchedule")]
        [Authorize]
        [HttpPost]
        public IHttpActionResult CancelSchedule(ScheduleParameters scheduleParams)
        {
            using (var db = new AppDbContext())
            {
                var schedule = db.Schedules.FirstOrDefault(x =>
                    x.Id == scheduleParams.Id
                    && (x.State == ScheduleState.Pending || x.State == ScheduleState.Valid)
                    && x.ScheduleDateTimeStart > DateTime.Now
                    && x.UserId == scheduleParams.UserId);
                schedule.State = ScheduleState.CancelledByUser;
                db.Entry(schedule).State = EntityState.Modified;
                db.SaveChanges();

                return Ok(schedule);
            }
        }

        [Route("GetClientNoSchedulesFromNowOn")]
        [Authorize]
        [HttpPost]
        public IHttpActionResult GetClientNoSchedulesFromNowOn(ScheduleParameters scheduleParameters)
        {
            return Ok(RetrieveOthers.NoSchedulesFromNowOnByUserId(scheduleParameters.UserId));
        }

        [Authorize]
        [Route("GetClientSchedules")]
        [HttpPost]
        public IHttpActionResult GetClientSchedules(ScheduleParameters scheduleParameters)
        {
            return Ok(RetrieveLists.GetAllClientSchedules(scheduleParameters.UserId, scheduleParameters.ShowHistory, scheduleParameters.ShowCurrent));
        }

        [Route("GetSchedule")]
        [Authorize]
        [HttpPost]
        public IHttpActionResult GetSchedule(ScheduleParameters scheduleParams)
        {
            using (var db = new AppDbContext())
            {
                var schedule = db.Schedules
                    .Include(x => x.User)
                    .Include(x => x.User)
                    .Include(x => x.Slot)
                    .Include(x => x.Slot.Provider)
                    .Include(x => x.Slot.Provider.Addresses)
                    .Include(x => x.Slot.Provider.Addresses.Select(a => a.UserCity))
                    .Include(x => x.Slot.Provider.Addresses.Select(a => a.UserCity.CityCounty))
                    .FirstOrDefault(x => x.Id == scheduleParams.Id &&
                                         x.UserId == scheduleParams.UserId);

                if (schedule.Slot.Provider.ProgrammingPerSlot && !string.IsNullOrWhiteSpace(schedule.Slot.UserId))
                {
                    var slotAddres = RetrieveOthers.GetUserAddress(schedule.Slot.UserId);
                    schedule.Slot.FullAddress = slotAddres.ToString();
                }

                var operations = db.ScheduleSlotOperations.Include(x => x.SlotOperation.Operation).Where(so => so.ScheduleId == scheduleParams.Id);
                schedule.OperationsForSchedule = operations.Select(o => o.SlotOperation.Operation).ToList();

                return Ok(schedule);
            }
        }

        [Route("SaveNewSchedule")]
        [Authorize]
        [HttpPost]
        public IHttpActionResult SaveNewSchedule(ScheduleParameters scheduleParameters)
        {
            var saveResult = ScheduleBusinessLogic.SaveNewApointment(scheduleParameters);

            var message = string.Empty;

            if (saveResult == 1) //Overlapping
            {
                message = Resources.Resource.Error_Schedule1;
            }
            else if (saveResult == 2)// More than one schedule at the same provider in the same day
            {
                message = Resources.Resource.Error_Schedule2;
            }
            else if (saveResult == 3)
            {
                message = "Nu ati selectat nicio operatiune.";
            }

            var result = new
            {
                saveResult,
                message
            };

            return Ok(result);
        }

        #endregion Public Methods
    }
}