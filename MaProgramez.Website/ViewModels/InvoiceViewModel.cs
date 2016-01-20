using System.Linq;
using MaProgramez.Repository.Entities;

namespace MaProgramez.Website.ViewModels
{
    using System;

    public class InvoiceViewModel
    {
        #region BCKING FIELDS

        private decimal? vatRate;

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Initializes a new instance of the <see cref="InvoiceViewModel"/> class.
        /// </summary>
        /// <param name="invoice">The invoice.</param>
        public InvoiceViewModel(InvoiceHeader invoice)
        {
            this.Invoice = invoice;
        }

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Gets or sets the invoice.
        /// </summary>
        /// <value>
        /// The invoice.
        /// </value>
        public InvoiceHeader Invoice { get; set; }

        /// <summary>
        /// Gets the total amount.
        /// </summary>
        /// <value>
        /// The total amount.
        /// </value>
        public decimal TotalAmount
        {
            get
            {
                //return this.Invoice != null ? this.Invoice.AmountWithoutVat + this.Invoice.Vat : 0;
                return this.Invoice.InvoiceLines != null
                    ? this.Invoice.InvoiceLines.Sum(x => x.Price*x.Quantity)*(this.VatRate + 1)
                    : 0;
            }
        }

        /// <summary>
        /// Gets the state o the invoice payment.
        /// </summary>
        /// <value>
        /// The state of the invoice.
        /// </value>
        public string InvoiceState
        {
            get
            {
                if (this.Invoice != null)
                {
                    return this.Invoice.InvoicePayments.Sum(ip => ip.Amount) == this.TotalAmount ? 
                        Resources.Resource.InvoiceStatePaid : Resources.Resource.InvoiceStateNotPaid;
                }

                return Resources.Resource.InvoiceStateNotPaid;
            }
        }

        /// <summary>
        /// Gets the vat rate.
        /// </summary>
        /// <value>
        /// The vat rate.
        /// </value>
        public decimal VatRate
        {
            get
            {
                if (this.vatRate == null)
                {
                    this.vatRate = Utility.Common.GetDbConfig("VatRate").ToDecimal();
                }

                return this.vatRate.Value;
            }
        }

        #endregion
    }
}