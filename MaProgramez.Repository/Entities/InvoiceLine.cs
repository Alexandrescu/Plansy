using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaProgramez.Repository.Entities
{
    public class InvoiceLine
    {
        [Key]
        public int Id { get; set; }

        public int InvoiceHeaderId { get; set; }

        [ForeignKey("InvoiceHeaderId")]
        public virtual InvoiceHeader InvoiceHeader { get; set; }

        [MaxLength(500)]
        public string LineDescription { get; set; }

        [MaxLength(32)]
        public string UnitOfMeasurement { get; set; }

        public decimal Quantity { get; set; }

        public decimal Price { get; set; }
    }
}
