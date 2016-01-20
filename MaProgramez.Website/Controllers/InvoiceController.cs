using System.Diagnostics;
using System.Globalization;
using MaProgramez.Repository.BusinessLogic;
using MaProgramez.Repository.DbContexts;
using MaProgramez.Repository.Entities;
using MaProgramez.Website.Helpers;

namespace MaProgramez.Website.Controllers
{
    using Extensions;
    using Microsoft.Ajax.Utilities;
    using Microsoft.AspNet.Identity;
    using MvcPaging;
    using RazorPDF;
    using Resources;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Validation;
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.WebPages;
    using Utility;
    using ViewModels;

    [CustomAuthorize]
    public partial class InvoiceController : BaseController
    {
        #region LIST

        /// <summary>
        ///     Invoices the list.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="page">The page.</param>
        /// <param name="sortBy">The sort by.</param>
        /// <param name="filterBy">The filter by.</param>
        /// <param name="ascending">if set to <c>true</c> [ascending].</param>
        /// <returns></returns>
        public virtual ActionResult InvoiceList(string userId, int? page, string sortBy, string filterBy,
            bool ascending = true)
        {
            int currentPageIndex = page.HasValue ? page.Value - 1 : 0;
            if (string.IsNullOrWhiteSpace(userId))
            {
                userId = User.Identity.GetUserId();
            }

            using (var db = new AppDbContext())
            {
                ApplicationUser user = db.Users.FirstOrDefault(u => u.Id == userId);
                ViewBag.UserName = user.FirstName + " " + user.LastName;

                int filterByInt;
                int.TryParse(filterBy, out filterByInt);

                decimal filterByDecimal;
                decimal.TryParse(filterBy, out filterByDecimal);
                IQueryable<InvoiceHeader> invoices = db.InvoiceHeaders
                    .Include(i => i.User.Addresses.Select(a => a.UserCity.CityCounty))
                    .Include(i => i.User.Addresses.Select(a => a.UserCountry))
                    .Include(i => i.InvoiceLines)
                    .Include(i => i.InvoicePayments)
                    .Where(ih => ih.UserId == userId);

                if (!string.IsNullOrWhiteSpace(filterBy))
                {
                    invoices = invoices.Where(x => x.Number == filterByInt ||
                                                   InvoiceUtility.GetInvoiceAmountWithoutVat(x.Id) == filterByDecimal ||
                                                   x.InvoiceLines.Any(il => il.LineDescription.Contains(filterBy)));
                }

                var ivms = new List<InvoiceListItemViewModel>();
                foreach (InvoiceHeader i in invoices)
                {
                    var n = new InvoiceListItemViewModel
                    {
                        Id = i.Id,
                        Number = i.Number,
                        Series = i.Series,
                        AmountWithoutVat = InvoiceUtility.GetInvoiceAmountWithoutVat(i.Id),
                        AmountWithVat = InvoiceUtility.GetInvoiceTotalAmount(i.Id),
                        Vat = InvoiceUtility.GetInvoiceVat(i.Id),
                        Date = i.Date,
                        DueDate = i.DueDate,
                        PaymentState = InvoiceUtility.InvoiceState(i.Id),
                        State = i.State,
                        UserId = i.UserId
                    };

                    ivms.Add(n);
                }

                List<InvoiceListItemViewModel> sortedList;

                switch (sortBy)
                {
                    case "number":
                        sortedList = ascending
                            ? ivms.OrderBy(x => x.Number).ToList()
                            : ivms.OrderByDescending(x => x.Number).ToList();
                        break;

                    case "duedate":
                        sortedList = ascending
                            ? ivms.OrderBy(x => x.DueDate).ToList()
                            : ivms.OrderByDescending(x => x.DueDate).ToList();
                        break;

                    case "amountwithoutvat":
                        sortedList = ascending
                            ? ivms.OrderBy(x => x.AmountWithoutVat).ToList()
                            : ivms.OrderByDescending(x => x.AmountWithoutVat).ToList();
                        break;

                    case "amountincludingvat":
                        sortedList = ascending
                            ? ivms.OrderBy(x => x.AmountWithVat).ToList()
                            : ivms.OrderByDescending(x => x.AmountWithVat).ToList();
                        break;

                    case "vat":
                        sortedList = ascending
                            ? ivms.OrderBy(x => x.Vat).ToList()
                            : ivms.OrderByDescending(x => x.Vat).ToList();
                        break;

                    case "date":
                        sortedList = ascending
                            ? ivms.OrderBy(x => x.Date).ToList()
                            : ivms.OrderByDescending(x => x.Date).ToList();
                        break;

                    case "invoicestate":
                        sortedList = ascending
                            ? ivms.OrderBy(x => x.State).ToList()
                            : ivms.OrderByDescending(x => x.State).ToList();
                        break;

                    default:
                        sortedList = ivms.OrderByDescending(x => x.Date).ToList();
                        break;
                }

                ViewBag.SortBy = sortBy;
                ViewBag.FilterBy = filterBy;
                ViewBag.Ascending = ascending;
                ViewBag.Page = currentPageIndex;

                return View(sortedList.ToPagedList(currentPageIndex, 10));
            }
        }

        /// <summary>
        ///     Admin - the invoice list.
        /// </summary>
        /// 
        /// <param name="userId">User invoiced.</param>
        /// <param name="page">The page.</param>
        /// <param name="sortBy">The sort by.</param>
        /// <param name="filterBy">The filter by.</param>
        /// <param name="ascending">if set to <c>true</c> [ascending].</param>
        /// <returns></returns>
        [CustomAuthorize(Roles = "Admin, Agent, Accountant")]
        public virtual ActionResult AdminInvoiceList(string userId, int? page, string sortBy, string filterBy, bool ascending = true)
        {
            int currentPageIndex = page.HasValue ? page.Value - 1 : 0;

            int filterByInt;
            int.TryParse(filterBy, out filterByInt);

            decimal filterByDecimal;
            decimal.TryParse(filterBy, out filterByDecimal);

            using (var db = new AppDbContext())
            {
                IQueryable<InvoiceHeader> invoices;
                if (string.IsNullOrWhiteSpace(userId))
                {
                    invoices = from i in db.InvoiceHeaders
                        .Include(i => i.User.Addresses.Select(a => a.UserCity.CityCounty))
                        .Include(i => i.User.Addresses.Select(a => a.UserCountry))
                        .Include(i => i.InvoiceLines)
                        .Include(i => i.InvoicePayments)
                               select i;
                }
                else
                {
                    invoices = from i in db.InvoiceHeaders
                        .Include(i => i.User.Addresses.Select(a => a.UserCity.CityCounty))
                        .Include(i => i.User.Addresses.Select(a => a.UserCountry))
                        .Include(i => i.InvoiceLines)
                        .Include(i => i.InvoicePayments)
                               where i.UserId == userId
                               select i;
                }

                var ivms = new List<InvoiceListItemViewModel>();
                foreach (InvoiceHeader i in invoices)
                {
                    var n = new InvoiceListItemViewModel();
                    if (i.User.IsCompany)
                    {
                        n.UserName = i.User.CompanyName;
                    }
                    else if (i.User.FirstName.IsNullOrWhiteSpace() && i.User.LastName.IsNullOrWhiteSpace())
                    {
                        n.UserName = i.User.UserName;
                    }
                    else
                    {
                        n.UserName = i.User.FirstName + " " + i.User.LastName;
                    }
                    n.Id = i.Id;
                    n.Number = i.Number;
                    n.Series = i.Series;
                    n.AmountWithoutVat = InvoiceUtility.GetInvoiceAmountWithoutVat(i.Id);
                    n.AmountWithVat = InvoiceUtility.GetInvoiceTotalAmount(i.Id);
                    //n.Vat = InvoiceUtility.GetInvoiceVat(i.Id);
                    n.Date = i.Date;
                    n.DueDate = i.DueDate;
                    n.PaymentState = InvoiceUtility.InvoiceState(i.Id);
                    n.State = i.State;
                    n.UserId = i.UserId;
                    n.ReceiptNo = i.ReceiptNo;
                    ivms.Add(n);
                }

                List<InvoiceListItemViewModel> filteredList;
                List<InvoiceListItemViewModel> sortedList;

                if (!string.IsNullOrWhiteSpace(filterBy))
                {
                    filteredList = ivms.Where(x => x.Number == filterByInt ||
                                                   x.AmountWithVat == filterByDecimal ||
                                                   x.AmountWithoutVat == filterByDecimal ||
                                                   x.UserName.Contains(filterBy)).ToList();
                }
                else
                {
                    filteredList = ivms.ToList();
                }

                if (filteredList.Any())
                {
                    switch (sortBy)
                    {
                        case "name":
                            sortedList = ascending
                                ? filteredList.OrderBy(x => x.UserName).ToList()
                                : filteredList.OrderByDescending(x => x.UserName).ToList();
                            break;

                        case "number":
                            sortedList = ascending
                                ? filteredList.OrderBy(x => x.Number).ToList()
                                : filteredList.OrderByDescending(x => x.Number).ToList();
                            break;

                        case "duedate":
                            sortedList = ascending
                                ? filteredList.OrderBy(x => x.DueDate).ToList()
                                : filteredList.OrderByDescending(x => x.DueDate).ToList();
                            break;

                        case "amountwithoutvat":
                            sortedList = ascending
                                ? filteredList.OrderBy(x => x.AmountWithoutVat).ToList()
                                : filteredList.OrderByDescending(x => x.AmountWithoutVat).ToList();
                            break;
                        /*
    case "vat":
        sortedList = ascending
            ? filteredList.OrderBy(x => x.Vat).ToList()
            : filteredList.OrderByDescending(x => x.Vat).ToList();
        break;*/
                        case "amountincludingvat":
                            sortedList = ascending
                                ? filteredList.OrderBy(x => x.AmountWithVat).ToList()
                                : filteredList.OrderByDescending(x => x.AmountWithVat).ToList();
                            break;

                        case "date":
                            sortedList = ascending
                                ? filteredList.OrderBy(x => x.Date).ToList()
                                : filteredList.OrderByDescending(x => x.Date).ToList();
                            break;

                        case "paymentstate":
                            sortedList = ascending
                                ? filteredList.OrderBy(x => x.PaymentState).ToList()
                                : filteredList.OrderByDescending(x => x.PaymentState).ToList();
                            break;

                        default:
                            sortedList = filteredList.OrderByDescending(x => x.Date).ToList();
                            break;
                    }
                }
                else
                {
                    sortedList = filteredList;
                }
                ViewBag.SortBy = sortBy;
                ViewBag.FilterBy = filterBy;
                ViewBag.Ascending = ascending;
                ViewBag.Page = currentPageIndex;

                return View(sortedList.ToPagedList(currentPageIndex, 10));
            }
        }

        #endregion LIST

        #region PAYMENT

        /// <summary>
        ///     Adds the invoice payment.
        /// </summary>
        /// <param name="invoiceHeaderId">The invoice header identifier.</param>
        /// <returns></returns>
        [HttpGet]
        [CustomAuthorize(Roles = "Admin, Agent")]
        public virtual ActionResult AddInvoicePayment(int invoiceHeaderId)
        {
            using (var db = new AppDbContext())
            {
                ViewBag.PreviousPayments =
                    db.InvoicePayments.Where(x => x.InvoiceHeaderId == invoiceHeaderId).ToList();

                var header = db.InvoiceHeaders.Find(invoiceHeaderId);
                var payment = new InvoicePayment
                {
                    InvoiceHeader = header,
                    InvoiceHeaderId = invoiceHeaderId,
                    Date = DateTime.Now
                };

                return View(payment);
            }
        }

        /// <summary>
        ///     Adds the invoice payment.
        /// </summary>
        /// <param name="payment">The payment.</param>
        /// <returns></returns>
        [HttpPost]
        [CustomAuthorize(Roles = "Admin, Agent")]
        public virtual ActionResult AddInvoicePayment(InvoicePayment payment)
        {
            using (var db = new AppDbContext())
            {
                db.InvoicePayments.Add(payment);

                InvoiceHeader header = db.InvoiceHeaders.Find(payment.InvoiceHeaderId);

                if (payment.Amount == 0 || payment.PaymentMethod == 0)
                {
                    payment.InvoiceHeader = header;
                    ViewBag.PreviousPayments =
                        db.InvoicePayments.Where(x => x.InvoiceHeaderId == payment.InvoiceHeaderId).ToList();

                    ModelState.Clear();
                    ModelState.AddModelError("", Resources.Resource.Error_MandatoryFields);
                    return View(payment);
                }


                if (header.InvoicePayments == null)
                    header.InvoicePayments = new List<InvoicePayment> { payment };
                else header.InvoicePayments.Add(payment);
                db.InvoiceHeaders.Attach(header);
                db.Entry(header).State = EntityState.Modified;

                db.SaveChanges();

                return RedirectToAction(MVC.Invoice.AddInvoicePayment(payment.InvoiceHeaderId));
            }
        }

        /// <summary>
        ///     Edits the invoice payment.
        /// </summary>
        /// <param name="invoicePaymentId">The invoice payment identifier.</param>
        /// <returns></returns>
        [HttpGet]
        [CustomAuthorize(Roles = "Admin, Agent")]
        public virtual ActionResult EditInvoicePayment(int invoicePaymentId)
        {
            using (var db = new AppDbContext())
            {
                InvoicePayment payment = db.InvoicePayments.Include("InvoiceHeader")
                    .First(i => i.Id == invoicePaymentId);

                ViewBag.PreviousPayments =
                    db.InvoicePayments.Where(
                        x => x.InvoiceHeaderId == payment.InvoiceHeaderId && x.Id != payment.Id)
                        .ToList();

                return View(payment);
            }
        }

        /// <summary>
        ///     Edits the invoice payment.
        /// </summary>
        /// <param name="payment">The payment.</param>
        /// <returns></returns>
        [HttpPost]
        [CustomAuthorize(Roles = "Admin, Agent")]
        public virtual ActionResult EditInvoicePayment(InvoicePayment payment)
        {
            using (var db = new AppDbContext())
            {
                if (payment.Amount == 0 || payment.PaymentMethod == 0)
                {
                    payment.InvoiceHeader = db.InvoiceHeaders.Find(payment.InvoiceHeaderId);
                    ViewBag.PreviousPayments =
                        db.InvoicePayments.Where(
                            x => x.InvoiceHeaderId == payment.InvoiceHeaderId && x.Id != payment.Id)
                            .ToList();

                    ModelState.Clear();
                    ModelState.AddModelError("", Resources.Resource.Error_MandatoryFields);
                    return View(payment);
                }

                db.InvoicePayments.Attach(payment);
                db.Entry(payment).State = EntityState.Modified;

                db.SaveChanges();

                return RedirectToAction(MVC.Invoice.AddInvoicePayment(payment.InvoiceHeaderId));
            }
        }

        /// <summary>
        ///     Removes the invoice payment.
        /// </summary>
        /// <param name="invoicePaymentId">The invoice payment identifier.</param>
        /// <returns></returns>
        [HttpGet]
        [CustomAuthorize(Roles = "Admin, Agent")]
        public virtual ActionResult RemoveInvoicePayment(int invoicePaymentId)
        {
            using (var db = new AppDbContext())
            {
                InvoicePayment payment = db.InvoicePayments.Find(invoicePaymentId);
                InvoiceHeader header = db.InvoiceHeaders.Find(payment.InvoiceHeaderId);

                header.InvoicePayments.Remove(payment);
                db.InvoiceHeaders.Attach(header);
                db.Entry(header).State = EntityState.Modified;

                db.InvoicePayments.Remove(payment);
                db.SaveChanges();

                return RedirectToAction(MVC.Invoice.AddInvoicePayment(payment.InvoiceHeaderId));
            }
        }

        #endregion PAYMENT

        #region PDF

        /// <summary>
        /// Used to view the invoice without being logged in (from the links in email)
        /// </summary>
        /// <param name="invoiceHeaderId">The invoice header identifier.</param>
        /// <param name="userToken">The user id.</param>
        /// <returns></returns>
        [AllowAnonymous]
        public virtual ActionResult ViewInvoice(int invoiceHeaderId, string userToken)
        {
            using (var db = new AppDbContext())
            {
                var invoice = db.InvoiceHeaders
                    .Include(i => i.User.Addresses.Select(a => a.UserCity.CityCounty))
                    .Include(i => i.User.Addresses.Select(a => a.UserCountry))
                    .Include(i => i.InvoiceLines)
                    .Include(i => i.InvoicePayments)
                    .FirstOrDefault(i => i.Id == invoiceHeaderId);

                if (invoice == null || userToken != invoice.UserId)
                {
                    return RedirectToAction(MVC.Error.Forbidden());
                }

                var invoiceViewModel = new InvoiceViewModel(invoice);
                PdfResult pdfResult = invoiceViewModel.VatRate != 0
                    ? new PdfResult(invoiceViewModel, "InvoicePdf")
                    : new PdfResult(invoiceViewModel, "InvoicePdfVatZero");
                pdfResult.ViewBag.Title = "FACTURA FISCALA";
                return pdfResult;
            }
        }

        /// <summary>
        ///     Invoice PDF.
        /// </summary>
        /// <param name="invoiceHeaderId">The invoice header identifier.</param>
        /// <returns></returns>
        public virtual ActionResult InvoicePdf(int invoiceHeaderId)
        {
            using (var db = new AppDbContext())
            {
                InvoiceHeader invoice = db.InvoiceHeaders
                    .Include(i => i.User.Addresses.Select(a => a.UserCity.CityCounty))
                    .Include(i => i.User.Addresses.Select(a => a.UserCountry))
                    .Include(i => i.InvoiceLines)
                    .Include(i => i.InvoicePayments)
                    .FirstOrDefault(i => i.Id == invoiceHeaderId);

                if (!User.IsInRole("Admin") && !User.IsInRole("Agent") && !User.IsInRole("Accountant") && User.Identity.GetUserId() != invoice.UserId)
                    return RedirectToAction(MVC.Error.Forbidden());

                var invoiceViewModel = new InvoiceViewModel(invoice);
                PdfResult pdfResult = invoiceViewModel.VatRate != 0
                    ? new PdfResult(invoiceViewModel, "InvoicePdf")
                    : new PdfResult(invoiceViewModel, "InvoicePdfVatZero");
                pdfResult.ViewBag.Title = "FACTURA FISCALA";
                return pdfResult;
            }
        }

        public virtual ActionResult ReceiptPdf(int invoiceHeaderId)
        {
            using (var db = new AppDbContext())
            {
                InvoiceHeader invoice = db.InvoiceHeaders
                    .Include(i => i.User.Addresses.Select(a => a.UserCity.CityCounty))
                    .Include(i => i.User.Addresses.Select(a => a.UserCountry))
                    .Include(i => i.InvoiceLines)
                    .Include(i => i.InvoicePayments)
                    .FirstOrDefault(i => i.Id == invoiceHeaderId);

                if (!User.IsInRole("Admin") && !User.IsInRole("Agent") && !User.IsInRole("Accountant") && User.Identity.GetUserId() != invoice.UserId)
                    return RedirectToAction(MVC.Error.Forbidden());

                var invoiceViewModel = new InvoiceViewModel(invoice);
                var pdfResult = new PdfResult(invoiceViewModel, "ReceiptPdf");
                pdfResult.ViewBag.Title = "CHITANTA";
                return pdfResult;
            }
        }

        #endregion PDF

        #region LINE

        /// <summary>
        ///     Edits the invoice line.
        /// </summary>
        /// <param name="invoiceLineId">The invoice line identifier.</param>
        /// <returns></returns>
        [HttpGet]
        [CustomAuthorize(Roles = "Admin")]
        public virtual ActionResult EditInvoiceLine(int invoiceLineId)
        {
            using (var db = new AppDbContext())
            {
                InvoiceLine line = db.InvoiceLines.Include("InvoiceHeader").First(i => i.Id == invoiceLineId);
                ViewBag.Lines =
                    db.InvoiceLines.Where(
                        x => x.InvoiceHeaderId == line.InvoiceHeaderId && x.Id != line.Id).ToList();

                return View(line);
            }
        }

        /// <summary>
        ///     Edits the invoice line.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <returns></returns>
        [HttpPost]
        [CustomAuthorize(Roles = "Admin")]
        public virtual ActionResult EditInvoiceLine(InvoiceLine line)
        {
            using (var db = new AppDbContext())
            {
                if (line.Price == 0 || line.LineDescription == null || line.UnitOfMeasurement == null ||
                    line.Quantity == 0)
                {
                    line.InvoiceHeader = db.InvoiceHeaders.Find(line.InvoiceHeaderId);
                    ViewBag.Lines =
                        db.InvoiceLines.Where(
                            x => x.InvoiceHeaderId == line.InvoiceHeaderId && x.Id != line.Id).ToList();

                    ModelState.Clear();
                    ModelState.AddModelError("", Resource.Error_MandatoryFields);
                    return View(line);
                }

                db.InvoiceLines.Attach(line);
                db.Entry(line).State = EntityState.Modified;

                db.SaveChanges();

                return RedirectToAction(MVC.Invoice.AddInvoiceLine(line.InvoiceHeaderId));
            }
        }

        [HttpGet]
        [CustomAuthorize(Roles = "Admin")]
        public virtual ActionResult RemoveInvoiceLine(int invoiceLineId)
        {
            using (var db = new AppDbContext())
            {
                InvoiceLine line = db.InvoiceLines.Find(invoiceLineId);
                InvoiceHeader header = db.InvoiceHeaders.Find(line.InvoiceHeaderId);

                header.InvoiceLines.Remove(line);
                db.InvoiceHeaders.Attach(header);
                db.Entry(header).State = EntityState.Modified;

                db.InvoiceLines.Remove(line);
                db.SaveChanges();

                return RedirectToAction(MVC.Invoice.AddInvoiceLine(line.InvoiceHeaderId));
            }
        }

        [HttpGet]
        [CustomAuthorize(Roles = "Admin")]
        public virtual ActionResult AddInvoiceLine(int invoiceHeaderId)
        {
            using (var db = new AppDbContext())
            {
                InvoiceHeader header = db.InvoiceHeaders.Include("InvoiceLines").First(i => i.Id == invoiceHeaderId);
                //int lineNo = 0;

                var line = new InvoiceLine
                {
                    InvoiceHeaderId = invoiceHeaderId,
                    InvoiceHeader = header
                    //LineNumber = lineNo + 1,
                };

                return View(line);
            }
        }

        /// <summary>
        ///     Adds the invoice line.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <returns></returns>
        [HttpPost]
        [CustomAuthorize(Roles = "Admin")]
        public virtual ActionResult AddInvoiceLine(InvoiceLine line)
        {
            using (var db = new AppDbContext())
            {
                if (line.Price == 0 || line.LineDescription == null || line.UnitOfMeasurement == null ||
                    line.Quantity == 0)
                {
                    InvoiceHeader header = db.InvoiceHeaders.Include("InvoiceLines").First(i => i.Id == line.InvoiceHeaderId);
                    line.InvoiceHeader = header;

                    ModelState.Clear();
                    ModelState.AddModelError("", Resource.Error_MandatoryFields);
                    return View(line);
                }

                db.InvoiceLines.Add(line);
                db.SaveChanges();

                return RedirectToAction(MVC.Invoice.AddInvoiceLine(line.InvoiceHeaderId));
            }
        }

        public virtual ActionResult SendPaymentLink(string userId, int invoiceId)
        {
            var user = RetrieveOthers.GetUserById(userId);
            if (!user.Email.IsNullOrWhiteSpace())
            {
                MailAndSmsUtility.SendEmail(user.Email, "Link de plata abonament MaProgramez.net",
                    MvcUtility.RenderRazorViewToString(this,
                        MVC.Mail.Views.GenericMail,
                        new MailViewModel
                        {
                            Title = "Link de plata online securizat cu card bancar - abonament MaProgramez.net",
                            Content = "<a href=\"" +
                                      Common.GetDbConfig("SiteUrl") + "/ro/NetopiaMobilPay/PayByCard?invoiceId=" +
                                      invoiceId + "&userToken=" + user.Id +
                                      "\">Click aici</a> &nbsp pentru a va plati online abonamentul MaProgramez.net!"
                            ,
                            Footer = Resource.NoReply
                        }));
            }
            if (user.Agent != null && !user.Agent.Email.IsNullOrWhiteSpace())
            {
                MailAndSmsUtility.SendEmail(user.Agent.Email, "Link de plata abonament MaProgramez.net pentru " + user.CompanyName,
                   MvcUtility.RenderRazorViewToString(this,
                       MVC.Mail.Views.GenericMail,
                       new MailViewModel
                       {
                           Title = "Link de plata abonament MaProgramez.net",
                           Content = "<a href=\"" +
                                     Common.GetDbConfig("SiteUrl") + "/ro/NetopiaMobilPay/PayByCard?invoiceId=" +
                                     invoiceId + "&userToken=" + user.Id +
                                     "\">Click aici</a>pentru a va plati online abonamentul MaProgramez.net!"
                           ,
                           Footer = Resource.NoReply
                       }));
            }

            return RedirectToConfirmation(new ConfirmationViewModel
            {
                Title = "Link de plata trimis",
                Message = "Linkul de plata cu cardul online a fost trimis pe email.",
                Type = ConfirmationType.Success,
                Link = invoiceId > 0
                            ? Url.Action(MVC.Invoice.AdminInvoiceList(userId, 1, null, null))
                            : Url.Action(MVC.Home.Index()),
            });
        }

        public virtual ActionResult SendInvoiceLink(string userId, int invoiceId)
        {
            var user = RetrieveOthers.GetUserById(userId);
            if (!user.Email.IsNullOrWhiteSpace())
            {
                MailAndSmsUtility.SendEmail(user.Email, "Factura fiscala MaProgramez.net",
                    MvcUtility.RenderRazorViewToString(this,
                        MVC.Mail.Views.GenericMail,
                        new MailViewModel
                        {
                            Title = "Factura fiscala pentru abonamentul MaProgramez.net",
                            Content = "<a href=\"" +
                                      Common.GetDbConfig("SiteUrl") + "/ro/Invoice/ViewInvoice?invoiceHeaderId=" +
                                      invoiceId + "&userToken=" + user.Id +
                                      "\">Click aici</a> &nbsp pentru a descarca factura fiscala pentru abonamentul MaProgramez.net!"
                            ,
                            Footer = Resource.NoReply
                        }));
            }
            if (user.Agent != null && !user.Agent.Email.IsNullOrWhiteSpace())
            {
                MailAndSmsUtility.SendEmail(user.Agent.Email, "Factura fiscala MaProgramez.net pentru " + user.CompanyName,
                   MvcUtility.RenderRazorViewToString(this,
                       MVC.Mail.Views.GenericMail,
                       new MailViewModel
                       {
                           Title = "Factura fiscala pentru abonamentul MaProgramez.net",
                           Content = "<a href=\"" +
                                     Common.GetDbConfig("SiteUrl") + "/ro/Invoice/ViewInvoice?invoiceHeaderId=" +
                                     invoiceId + "&userToken=" + user.Id +
                                     "\">Click aici</a> &nbsp pentru a descarca factura fiscala pentru abonamentul MaProgramez.net!"
                           ,
                           Footer = Resource.NoReply
                       }));
            }

            return RedirectToConfirmation(new ConfirmationViewModel
            {
                Title = "Factura trimisa",
                Message = "Factura fiscala a fost trimisa pe email.",
                Type = ConfirmationType.Success,
                Link = invoiceId > 0
                    ? Url.Action(MVC.Invoice.AdminInvoiceList(userId, 1, null, null))
                    : Url.Action(MVC.Home.Index()),
            });
        }

        public virtual ActionResult SendReceiptLink(string userId, int invoiceId)
        {
            var user = RetrieveOthers.GetUserById(userId);
            var invoice = RetrieveOthers.GetInvoiceHeaderById(invoiceId);

            if (!user.Email.IsNullOrWhiteSpace() && invoice.ReceiptNo.HasValue)
            {
                MailAndSmsUtility.SendEmail(user.Email, "Chitanta MaProgramez.net",
                    MvcUtility.RenderRazorViewToString(this,
                        MVC.Mail.Views.GenericMail,
                        new MailViewModel
                        {
                            Title = "Chitanta aferenta platii abonamentulului MaProgramez.net",
                            Content = "<a href=\"" +
                                      Common.GetDbConfig("SiteUrl") + "/ro/Invoice/ReceiptPdf?invoiceHeaderId=" +
                                      invoiceId +
                                      "\">Click aici</a> &nbsp pentru a descarca chitanta aferenta platii abonamentulului MaProgramez.net!"
                            ,
                            Footer = Resource.NoReply
                        }));
            }
            if (user.Agent != null && !user.Agent.Email.IsNullOrWhiteSpace() && invoice.ReceiptNo.HasValue)
            {
                MailAndSmsUtility.SendEmail(user.Agent.Email, "Chitanta MaProgramez.net",
                   MvcUtility.RenderRazorViewToString(this,
                       MVC.Mail.Views.GenericMail,
                       new MailViewModel
                       {
                           Title = "Chitanta aferenta platii abonamentulului MaProgramez.net",
                           Content = "<a href=\"" +
                                     Common.GetDbConfig("SiteUrl") + "/ro/Invoice/ReceiptPdf?invoiceHeaderId=" +
                                     invoiceId +
                                     "\">Click aici</a> &nbsp pentru a descarca chitanta aferenta platii abonamentulului MaProgramez.net!"
                           ,
                           Footer = Resource.NoReply
                       }));
            }

            if (invoice.ReceiptNo.HasValue)
            {
                return RedirectToConfirmation(new ConfirmationViewModel
                {
                    Title = "Chitanta trimisa",
                    Message = "Chitanta a fost trimisa pe email.",
                    Type = ConfirmationType.Success,
                    Link = invoiceId > 0
                        ? Url.Action(MVC.Invoice.AdminInvoiceList(userId, 1, null, null))
                        : Url.Action(MVC.Home.Index()),
                });
            }
            else
            {
                return RedirectToConfirmation(new ConfirmationViewModel
                {
                    Title = "Chitanta inexistanta",
                    Message = "Nu s-a emis chitanta pe aceasta factura",
                    Type = ConfirmationType.Error,
                    Link = invoiceId > 0
                        ? Url.Action(MVC.Invoice.AdminInvoiceList(userId, 1, null, null))
                        : Url.Action(MVC.Home.Index()),
                });
            }
        }

        #endregion

        #region JOBS

        [CustomAuthorize(Roles = "Admin")]
        [HttpGet]
        public virtual ActionResult GenerateCommisionInvoices(DateTime? dateTime = null)
        {
            using (var db = new AppDbContext())
            {
                var date = dateTime ?? DateTime.Now;
                var startDate = new DateTime(date.Year, date.Month, 1);
                date = date.AddMonths(1);
                var endDate = new DateTime(date.Year, date.Month, 1);

                var providers = db.Users.Where(x => x.Commision > 0).ToList();

                foreach (var provider in providers)
                {
                    var schedules =
                        db.Schedules.Where(
                            s =>
                                s.Slot.ProviderId == provider.Id && s.AddedByProvider == false &&
                                s.State == ScheduleState.Valid && s.ScheduleDateTimeStart >= startDate &&
                                s.ScheduleDateTimeStart < endDate).ToList();

                    if (schedules.Any())
                    {
                        var invoiceHeader = InvoiceUtility.GenerateBlankInvoice(provider.Id);
                        foreach (var schedule in schedules)
                        {
                            var slotOperations =
                                db.ScheduleSlotOperations.Include("SlotOperation").Include("SlotOperation.Operation")
                                    .Where(so => so.ScheduleId == schedule.Id)
                                    .Select(y => y.SlotOperation)
                                    .ToList();
                            var totalPrice = slotOperations.Sum(x => x.Price).ToDecimal();
                            var operationIds = slotOperations.Select(y => y.OperationId).ToList();
                            var operations =
                                db.Operations.Where(x => operationIds.Contains(x.Id)).ToList();

                            var ops = operations.Aggregate(string.Empty, (current, op) => current + (op.Description + " "));

                            var invoiceLine = new InvoiceLine()
                            {
                                InvoiceHeaderId = invoiceHeader,
                                LineDescription =
                                    schedule.ScheduleDateTimeStart.ToShortDateString() + " (" +
                                    schedule.ScheduleDateTimeStart.ToShortTimeString() + "-"
                                    + schedule.ScheduleDateTimeEnd.ToShortTimeString() + ") " + provider.Commision +
                                    "% from " + totalPrice + " (" + ops + ")",
                                UnitOfMeasurement = "Valid Appointment",
                                Quantity = 1,
                                Price = totalPrice * provider.Commision / 100,
                            };
                            db.InvoiceLines.Add(invoiceLine);
                        }
                    }
                }
                try
                {
                    db.SaveChanges();
                }
                catch (DbEntityValidationException dbEx)
                {
                    foreach (var validationErrors in dbEx.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            Trace.TraceInformation("Property: {0} Error: {1}",
                                                    validationError.PropertyName,
                                                    validationError.ErrorMessage);
                        }
                    }
                }
            }
            return RedirectToAction(MVC.Invoice.AdminInvoiceList());
        }

        #endregion
    }
}