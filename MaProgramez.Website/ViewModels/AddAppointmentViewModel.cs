using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MaProgramez.Repository.Entities;

namespace MaProgramez.Website.ViewModels
{
    public class AddAppointmentViewModel
    {
        public string ProviderId { get; set; }

        public int CategoryId { get; set; }
        
        public ApplicationUser Provider { get; set; }

        public Address ProviderAddress { get; set; }

        public List<Slot> AvailableSlots { get; set; }

        public Slot SelectedSlot { get; set; }

        public int SelectedSlotId { get; set; }
        
        public DateTime SelectedDate { get; set; }

        public List<DateTime> AvailableMinutes { get; set; }
        
        // This property contains the available operations
        public IEnumerable<SelectListItem> Operations { get; set; }

        // This property contains the selected operations
        public List<int> SelectedOperationIds { get; set; }

        public string Text { get; set; }

        public int SelectedHour { get; set; }

        public int SelectedMinute { get; set; }
        
        public List<int> AvailableMinutesForSelectedHour { get; set; }

        public AddAppointmentViewModel()
        {
            SelectedDate = DateTime.Now;
            AvailableMinutes = new List<DateTime>();
        }
    }
}