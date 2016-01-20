using System;
using MaProgramez.Repository.Entities;

namespace MaProgramez.Website.ViewModels
{
    public class PaymentReportViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string InvoiceSeries { get; set; }
        public int InvoiceNo { get; set; }
        public string UserName { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public int VatRate { get; set; }
        public PaymentMethod PaymentMethod { get; set; }

        public PaymentReportViewModel()
        {

        }
    }
}