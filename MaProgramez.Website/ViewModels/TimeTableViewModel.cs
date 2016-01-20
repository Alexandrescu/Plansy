using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Web;
using System.Web.Mvc;
using MaProgramez.Repository.Entities;
using MaProgramez.Website.Helpers;

namespace MaProgramez.Website.ViewModels
{
    public class TimeTableViewModel
    {
        public List<SlotTimeTable> Timetable { get; set; }

        // This property contains the available options
        public IEnumerable<SelectListItem> Days { get; set; }

        // This property contains the selected options
        public IEnumerable<Day> WorkingDays { get; set; }

        public string StartTime { get; set; }

        public string EndTime { get; set; }

        public bool IsWorkingByWeekly { get; set; }

        public DateTime WorkingWeekStartDate { get; set; }

        public Slot Slot { get; set; }

        public TimeTableViewModel()
        {
            Days = typeof(Day).GetItems(-1);
            WorkingDays = new List<Day>();
        }
    }
}