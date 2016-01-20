using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MaProgramez.Repository.Entities;

namespace MaProgramez.Website.ViewModels
{
    public class AddNonWorkingDayViewModel
    {
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int StartHour { get; set; }

        public int StartMinute { get; set; }

        public int EndHour { get; set; }

        public int EndMinute { get; set; }

        public string Description { get; set; }

        public int SlotId { get; set; }

        public Slot Slot { get; set; }

        public bool IsWorkingDay { get; set; }
    }
}