using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MaProgramez.Repository.Entities;

namespace MaProgramez.Website.ViewModels
{
    public class SlotViewModel
    {
        public Slot Slot { get; set; }

        public bool CreateSlotUser { get; set; }

        //public string Password { get; set; }
        
        //public string ConfirmPassword { get; set; }
    }
}