using System;
using System.Collections.Generic;

namespace MaProgramez.Repository.Models
{
    public class ProviderParameters
    {
        #region CONSTRUCTORS

        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderParameters"/> class.
        /// </summary>
        public ProviderParameters()
        {
            FullyLoadProvider = false;
            SelectedOperations = new List<int>();
            Page = 1;
            PageSize = 10;
        }

        #endregion

        #region PROPERTIES

        public int CategoryId { get; set; }

        public string ProviderId { get; set; }

        public string UserId { get; set; }

        public int? CityId { get; set; }
        
        public string SearchText { get; set; }

        public List<int> SelectedOperations { get; set; }

        public bool FullyLoadProvider { get; set; }

        public DateTime? SelectedDate { get; set; }

        public int? SelectedHour { get; set; }

        public int? SelectedMinute { get; set; }

        public int? SelectedSlotId { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }

        #endregion PROPERTIES
    }
}