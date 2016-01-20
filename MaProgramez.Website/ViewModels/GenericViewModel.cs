using System.Collections.Generic;
using MaProgramez.Repository.Entities;

namespace MaProgramez.Website.ViewModels
{
    public class GenericViewModel
    {
        #region PROPERTIES

        public string Status { get; set; }

        public IEnumerable<ApplicationUser> Users { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }

        public int Total { get; set; }

        #endregion
    }
}