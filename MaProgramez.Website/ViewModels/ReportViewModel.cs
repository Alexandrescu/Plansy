using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MaProgramez.Repository.Entities;

namespace MaProgramez.Website.ViewModels
{
    public class ReportViewModel 
    {
         #region PROPERTIES

        public DateTime Date { get; set; }
        public int SlotId { get; set; }
        public List<Slot> Slots { get; set; }
        public bool ShowPhoneNumber { get; set; }

        #endregion

        #region CONSTRUCTORS

        public ReportViewModel() { }

        #endregion
    }
}