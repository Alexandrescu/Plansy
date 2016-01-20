using System;
using System.Collections.Generic;

namespace MaProgramez.Repository.Models
{
    public class ScheduleParameters
    {
        public int Id { get; set; }

        public int SlotId { get; set; }

        public DateTime AppointmenDateTime { get; set; }

        public string UserId { get; set; }

        public string AppointmentText { get; set; }

        public List<int> SelectedOperationIds { get; set; }

        public DateTime Date { get; set; }

        public bool ShowHistory { get; set; }

        public bool ShowCurrent { get; set; }
    }
}
