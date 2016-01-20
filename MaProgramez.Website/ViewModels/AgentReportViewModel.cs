using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MaProgramez.Website.ViewModels
{
    public class AgentReportViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string AgentName { get; set; }
        public int SellersNo { get; set; }
        public int CustomersNo { get; set; }

        public AgentReportViewModel(DateTime startDate, DateTime endDate, string agentName, int sellersNo, int customersNo)
        {
            StartDate = startDate;
            EndDate = endDate;
            AgentName = agentName;
            SellersNo = sellersNo;
            CustomersNo = customersNo;
        }

        public AgentReportViewModel(DateTime startDate, DateTime endDate)
        {
            StartDate = startDate;
            EndDate = endDate;
        }

        public AgentReportViewModel()
        {
        }
 

    }
}