using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaProgramez.Repository.Entities
{
    public class InvoiceHeader
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(20)]
        public string Series { get; set; }

        public int Number { get; set; }

        public DateTime Date { get; set; }

        public DateTime DueDate { get; set; }

        [MaxLength(100)]
        public string DelegateName { get; set; }

        [MaxLength(50)]
        public string DelegateIdCardDetails { get; set; }

        [MaxLength(50)]
        public string TransportationVehicleDetails { get; set; }
        
        public int? StornoInvoiceId { get; set; }

        [ForeignKey("StornoInvoiceId")]
        public virtual InvoiceHeader StornoInvoice { get; set; }

        [MaxLength(128)]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        //[Display(ResourceType = typeof(Resources.Resource), Name = "Invoice_VatRate")]
        public int VatRate { get; set; }

        public InvoiceState State { get; set; }

        public int? ReceiptNo { get; set; }

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the invoice lines.
        /// </summary>
        /// <value>
        /// The invoice lines.
        /// </value>
        public virtual List<InvoiceLine> InvoiceLines { get; set; }

        /// <summary>
        /// Gets or sets the payments.
        /// </summary>
        /// <value>
        /// The payments.
        /// </value>
        public virtual List<InvoicePayment> InvoicePayments { get; set; }

        #endregion
    }
}
