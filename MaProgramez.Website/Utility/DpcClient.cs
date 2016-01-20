using System.Linq;
using DayPilot.Web.Mvc;
using DayPilot.Web.Mvc.Enums;
using DayPilot.Web.Mvc.Events.Calendar;
using MaProgramez.Repository.BusinessLogic;
using MaProgramez.Repository.DbContexts;

namespace MaProgramez.Website.Utility
{
    class DpcClient : DayPilotCalendar
    {
        private readonly string _id;
        private readonly AppDbContext _db = new AppDbContext();

        public DpcClient(string id)
        {
            _id = id;
        }

        protected override void OnInit(InitArgs e)
        {
            Update(CallBackUpdateType.Full);
        }

        protected override void OnEventClick(EventClickArgs e)
        {
            Redirect("/ro/Schedule/ViewSchedule/" + e.Id);
        }

        protected override void OnFinish()
        {
            if (UpdateType == CallBackUpdateType.None)
            {
                return;
            }

            Events = RetrieveLists.GetAllClientSchedules(_id);

            DataIdField = "id";
            DataTextField = "clientView";
            DataStartField = "scheduleDateTimeStart";
            DataEndField = "scheduleDateTimeEnd";
        }
    }
}