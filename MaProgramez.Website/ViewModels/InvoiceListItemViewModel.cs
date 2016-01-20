using System;
using System.Collections.Generic;
using System.Linq;
using MaProgramez.Repository.Entities;

namespace MaProgramez.Website.ViewModels
{
    public class InvoiceListItemViewModel
    {
        public int Id { get; set; }
        public string Series { get; set; }
        public int Number { get; set; }
        public DateTime Date { get; set; }
        public DateTime DueDate { get; set; }
        public decimal AmountWithoutVat { get; set; }
        public decimal AmountWithVat { get; set; }
        public decimal Vat { get; set; }
        public string PaymentState { get; set; }
        public InvoiceState State { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string UserId { get; set; }
        public int? ReceiptNo { get; set; }
    }
}