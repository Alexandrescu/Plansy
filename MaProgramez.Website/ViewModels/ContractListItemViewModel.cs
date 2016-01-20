using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MaProgramez.Website.ViewModels
{
    public class ContractListItemViewModel
    {
        public int ContractNo { get; set; }
        public DateTime ContractDate { get; set; }
        public string ClientName { get; set; }
        public string Address { get; set; }
        public string Agent { get; set; }

        public ContractListItemViewModel()
        {
            
        }
    }
}