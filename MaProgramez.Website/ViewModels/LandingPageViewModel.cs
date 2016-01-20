using MaProgramez.Repository.Entities;
using System.Collections.Generic;

namespace MaProgramez.Website.ViewModels
{
    public class LandingPageViewModel
    {
        #region PROPERTIES

        public int SuppliersTotal { get; set; }

        public int ClientsTotal { get; set; }

        public int SchedulesTotal { get; set; }

        public int OffersTotal { get; set; }

        public IEnumerable<string> Logos { get; set; }

        public IEnumerable<ApplicationUser> Suppliers { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Message { get; set; }

        #endregion PROPERTIES
    }
}