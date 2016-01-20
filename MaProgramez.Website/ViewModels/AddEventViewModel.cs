using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Windows.Forms;
using MaProgramez.Repository.Entities;

namespace MaProgramez.Website.ViewModels
{
    public class AddEventViewModel
    {
        public Schedule Schedule { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }
        
        public int StartHour { get; set; }
        
        public int StartMinute { get; set; }
        
        public int EndHour { get; set; }
        
        public int EndMinute { get; set; }

        public int ShortestOperationId { get; set; }

        public string ProviderId { get; set; }

        public int CategoryId { get; set; }

        public bool SendNotificationToClient { get; set; }

        public string UserId { get; set; }

        public List<int> AvailableHours { get; set; }

        public List<int> AvailableMinutes { get; set; }

        public IEnumerable<Address> Addresses { get; set; }

    }
}