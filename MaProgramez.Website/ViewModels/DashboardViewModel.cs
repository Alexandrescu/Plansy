using System.Collections.Generic;
using iTextSharp.text;
using MaProgramez.Repository.Entities;

namespace MaProgramez.Website.ViewModels
{
    public class DashboardViewModel
    {
        #region PROPERTIES
        
        public int SlotId { get; set; }

        public int TotalAppointmentsToday { get; set; }

        public int TotalAppointmentsThisWeek { get; set; }

        public int PendingAppointments { get; set; }

        public int FavouriteProviders { get; set; }

        public int TotalProviders { get; set; }

        public int TotalClients { get; set; }
        
        public bool IsPhoneNumberConfirmed { get; set; }

        public List<Favorite> FavouriteProvidersList { get; set; } 

        public List<ApplicationUser> NewProvidersInTown { get; set; } 

        public List<InvoiceHeader> UnpayedInvoices { get; set; }

        public List<Schedule> LastFiveSchedules { get; set; } 

        public List<Schedule> NextFiveSchedules { get; set; }

        #endregion
    }
}