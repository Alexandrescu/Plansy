using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaProgramez.Repository.Entities
{
    public class CardTransaction
    {
        [Key]
        public int Id { get; set; }

        public Decimal Amount { get; set; }

        [MaxLength(3)]
        public string Currency { get; set; }

        [MaxLength(1000)]
        public string Details { get; set; }

        public int InvoiceId { get; set; }

        [ForeignKey("InvoiceId")]
        public virtual InvoiceHeader Invoice { get; set; }
        
        [DataType(DataType.DateTime)]
        public DateTime Date { get; set; }

        [MaxLength(500)]
        public string State { get; set; }
        
        [MaxLength(128)]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the transaction histories.
        /// </summary>
        /// <value>
        /// The transaction histories.
        /// </value>
        public virtual List<CardTransactionHistory> TransactionHistories { get; set; }

        #endregion
    }
}
