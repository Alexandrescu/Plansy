using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MaProgramez.Repository.Entities;

namespace MaProgramez.Website.ViewModels
{
    public class SlotSchedulesViewModel
    {
        public Slot Slot { get; set; }
        public List<Schedule> Schedules { get; set; } 
    }
}