using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaProgramez.Repository.Entities
{
    public class InvoicePayment
    {
        [Key]
        public int Id { get; set; }

        public int InvoiceHeaderId { get; set; }

        [ForeignKey("InvoiceHeaderId")]
        public virtual InvoiceHeader InvoiceHeader { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public decimal Amount { get; set; }

        [MaxLength(200)]
        public string Details { get; set; }

        public DateTime Date { get; set; }
    }
}