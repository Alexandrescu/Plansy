using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text;
using MaProgramez.Repository.BusinessLogic;
using MaProgramez.Repository.DbContexts;
using MaProgramez.Repository.Entities;
using MaProgramez.Resources;

namespace MaProgramez.Website.Utility
{
    using System.Web.Mvc;
    using ViewModels;

    public static class InvoiceUtility
    {
        /// <summary>
        /// The vat rate
        /// </summary>
        public static decimal VatRate = Common.GetDbConfig("VatRate").ToDecimal();

        /// <summary>
        /// Determines whether [is invoice payed] [the specified header identifier].
        /// </summary>
        /// <param name="headerId">The header identifier.</param>
        /// <returns></returns>
        public static bool IsInvoicePayed(int headerId)
        {
            using (var db = new AppDbContext())
            {
                var price = GetInvoiceTotalAmount(headerId);
                var payed = db.InvoicePayments.Where(x => x.InvoiceHeaderId == headerId).Sum(x => x.Amount);

                return price == payed;
            }
        }

        /// <summary>
        /// Gets the invoice amount without vat.
        /// </summary>
        /// <param name="headerId">The header identifier.</param>
        /// <returns></returns>
        public static decimal GetInvoiceAmountWithoutVat(int headerId)
        {
            using (var db = new AppDbContext())
            {
                if (db.InvoiceLines.Any(x => x.InvoiceHeaderId == headerId))
                    return
                        db.InvoiceLines.Where(x => x.InvoiceHeaderId == headerId)
                            .Sum(x => x.Quantity * x.Price);
                return 0;
            }
        }

        /// <summary>
        /// Gets the invoice vat.
        /// </summary>
        /// <param name="headerId">The header identifier.</param>
        /// <returns></returns>
        public static decimal GetInvoiceVat(int headerId)
        {
            return GetInvoiceAmountWithoutVat(headerId) * VatRate / 100;
        }

        /// <summary>
        /// Gets the invoice total amount.
        /// </summary>
        /// <param name="headerId">The header identifier.</param>
        /// <returns></returns>
        public static decimal GetInvoiceTotalAmount(int headerId)
        {
            return GetInvoiceAmountWithoutVat(headerId) + GetInvoiceVat(headerId);
        }


        /// <summary>
        /// Invoices the state.
        /// </summary>
        /// <param name="headerId">The header identifier.</param>
        /// <returns></returns>
        public static string InvoiceState(int headerId)
        {
            using (var db = new AppDbContext())
            {
                var payments = db.InvoicePayments.Where(x => x.InvoiceHeaderId == headerId);

                if (payments.Any())
                {
                    return (payments.Sum(ip => ip.Amount) == GetInvoiceTotalAmount(headerId) &&
                            payments.First().Date.Date <= DateTime.Now.Date)
                        ? Resource.InvoiceStatePaid
                        : Resource.InvoiceStateNotPaid;
                }
            }
            return Resource.InvoiceStateNotPaid;
        }

        public static int GenerateBlankInvoice(string userId)
        {
            using (var dbContext = new AppDbContext())
            {
                using (var dbContextTransaction = dbContext.Database.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        var user = RetrieveOthers.GetUserById(userId);
                        var header = new InvoiceHeader
                        {
                            State = Repository.Entities.InvoiceState.Valid,
                            Series = Common.GetDbConfig("InvoiceSeries"),
                            Number = Common.GetDbConfig("InvoiceNumber").ToInteger(),
                            Date = DateTime.Now.Date,
                            DueDate = DateTime.Now.Date.AddDays(15), // generam fiscala dupa ce se face plata la curier
                            UserId = user.Id,
                            VatRate = Common.GetDbConfig("VatRate").ToInteger(),
                        };
                        
                        dbContext.InvoiceHeaders.Add(header);
                        dbContext.SaveChanges();

                        var no = dbContext.Configs.Find("InvoiceNumber");
                        no.Value = (no.Value.ToInteger() + 1).ToString(CultureInfo.InvariantCulture);
                        dbContext.Configs.Attach(no);
                        dbContext.Entry(no).State = EntityState.Modified;

                        dbContext.SaveChanges();
                        dbContextTransaction.Commit();

                        return header.Id;
                    }
                    catch (Exception)
                    {
                        dbContextTransaction.Rollback();
                        return 0;
                    }
                }
            }
        }

        public static int GenerateInvoice(string userId, int noMonths, decimal price, bool withReceipt = false)
        {
            using (var dbContext = new AppDbContext())
            {
                using (var dbContextTransaction = dbContext.Database.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        var user = RetrieveOthers.GetUserById(userId);
                        var header = new InvoiceHeader
                        {
                            State = Repository.Entities.InvoiceState.Valid,
                            Series = Common.GetDbConfig("InvoiceSeries"),
                            Number = Common.GetDbConfig("InvoiceNumber").ToInteger(),
                            Date = DateTime.Now.Date,
                            DueDate = DateTime.Now.Date.AddDays(15), // generam fiscala dupa ce se face plata la curier
                            UserId = user.Id,
                            VatRate = Common.GetDbConfig("VatRate").ToInteger(),
                        };
                        if (withReceipt) 
                            header.ReceiptNo = Common.GetDbConfig("ReceiptNumber").ToInteger();

                        dbContext.InvoiceHeaders.Add(header);
                        dbContext.SaveChanges();

                        var invoiceLine1 = new InvoiceLine()
                        {
                            InvoiceHeaderId = header.Id,
                            LineDescription = "Plata abonament MaProgramez.net pentru " + noMonths + " luni",
                            UnitOfMeasurement = "Buc",
                            Quantity = 1,
                            Price = price,
                        };
                        dbContext.InvoiceLines.Add(invoiceLine1);

                        var no = dbContext.Configs.Find("InvoiceNumber");
                        no.Value = (no.Value.ToInteger() + 1).ToString(CultureInfo.InvariantCulture);
                        dbContext.Configs.Attach(no);
                        dbContext.Entry(no).State = EntityState.Modified;

                        if (withReceipt)
                        {
                            var receiptNo = dbContext.Configs.Find("ReceiptNumber");
                            receiptNo.Value = (receiptNo.Value.ToInteger() + 1).ToString(CultureInfo.InvariantCulture);
                            dbContext.Configs.Attach(receiptNo);
                            dbContext.Entry(receiptNo).State = EntityState.Modified;
                        }

                        dbContext.SaveChanges();
                        dbContextTransaction.Commit();

                        return header.Id;
                    }
                    catch (Exception)
                    {
                        dbContextTransaction.Rollback();
                        return 0;
                    }
                }
            }
        }


        /// <summary>
        /// Adds an invoice payment.
        /// </summary>
        /// <param name="invoiceId">The invoice identifier.</param>
        /// <param name="paymentDate">The payment date.</param>
        /// <param name="paymentMethod">The payment method.</param>
        /// <returns></returns>
        public static decimal AddInvoicePayment(int invoiceId, DateTime? paymentDate = null, PaymentMethod paymentMethod = PaymentMethod.Ramburs)
        {
            paymentDate = paymentDate == null ? DateTime.Today : paymentDate.Value.Date;

            using (var db = new AppDbContext())
            {
                var invoiceHeader =
                    db.InvoiceHeaders.Include("InvoiceLines").Include("InvoicePayments").First(i => i.Id == invoiceId);

                // Add payment to client invoice
                if (invoiceHeader.InvoicePayments == null || !invoiceHeader.InvoicePayments.Any())
                {
                    var vatRate = VatRate == 0 ? 1 : (1 + VatRate / (decimal)100.00);
                    var invoicePayment = new InvoicePayment
                    {
                        Amount = invoiceHeader.InvoiceLines.Sum(l => l.Price * l.Quantity) * vatRate,
                        Date = paymentDate.Value,
                        PaymentMethod = paymentMethod,
                        InvoiceHeaderId = invoiceHeader.Id,
                        Details =
                            "Fact " + invoiceHeader.Series + invoiceHeader.Number + " din " +
                            invoiceHeader.Date.ToString("dd/MM/yyyy"),
                    };

                    db.InvoicePayments.Add(invoicePayment);

                    invoiceHeader.InvoicePayments = new List<InvoicePayment>() {invoicePayment};
                    db.Entry(invoiceHeader).State = EntityState.Modified;

                    db.SaveChanges();

                    return invoicePayment.Amount;
                }

                return 0;
            }
        }



    }
}