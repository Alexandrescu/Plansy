using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.WebPages;
using Elmah.ContentSyndication;
using iTextSharp.text;
using MaProgramez.Repository.BusinessLogic;
using MaProgramez.Resources;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using MaProgramez.Website.Extensions;
using MaProgramez.Website.Utility;
using MaProgramez.Website.ViewModels;
using RazorPDF;
using PdfResult = RazorPDF.PdfResult;

namespace MaProgramez.Website.Controllers
{

    public partial class ReportController : BaseController
    {
        #region PRIVATE FIELDS

        private ApplicationUserManager _userManager;

        #endregion

        #region PROPERTIES

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        #endregion

        #region ACTIONS

        [HttpGet]
        [CustomAuthorize(Roles = "Employee, Provider")]
        public virtual ActionResult DatePicker()
        {
            return
                View(new ReportViewModel()
                {
                    Date = DateTime.Now,
                    SlotId = User.IsInRole("Provider") ? 0 : RetrieveOthers.GetSlotIdByUserId(User.Identity.GetUserId()),
                    Slots = RetrieveLists.GetSlotsByProvider(User.IsInRole("Provider")
                        ? User.Identity.GetUserId()
                        : RetrieveOthers.GetSlotBySlotUserId(User.Identity.GetUserId()).ProviderId)
                });
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Employee, Provider")]
        public virtual ActionResult DatePicker(ReportViewModel model)
        {
            if (model.SlotId <= 0) return RedirectToAction(MVC.Report.DatePicker());

            var pdfResult = new PdfResult(RetrieveLists.GetSchedulesByDateAndSlot(model.SlotId, model.Date), "DaySchedule");
            pdfResult.ViewBag.SlotName = RetrieveOthers.GetSlotById(model.SlotId).Name;
            pdfResult.ViewBag.ShowPhoneNumber = model.ShowPhoneNumber;
            return pdfResult;
        }

        [HttpGet]
        [CustomAuthorize(Roles = "Employee, Provider")]
        public virtual ActionResult TodaySchedulesPdf(int? slotId)
        {
            if (!slotId.HasValue || slotId == 0) return RedirectToAction(MVC.Report.DatePicker());

            var pdfResult = new PdfResult(RetrieveLists.GetSchedulesByDateAndSlot((int)slotId, DateTime.Now), "DaySchedule");
            pdfResult.ViewBag.SlotName = RetrieveOthers.GetSlotById((int)slotId).Name;
            pdfResult.ViewBag.ShowPhoneNumber = true;
            return pdfResult;
        }


        #endregion
        //public virtual ActionResult DatePicker()
        //{
        //    var d = new Dictionary<int, string>();

        //    if (User.IsInRole("Admin"))
        //    {
        //        d.Add(1, Resource.Report_AgentsActivity);
        //        d.Add(2, Resource.Report_FinancialStatus);
        //        d.Add(3, Resource.Report_PaymentsStatus);
        //        d.Add(4, Resource.Report_AcceptedRequests);
        //    }

        //    if (User.IsInRole("Dezmembrator"))
        //    {
        //        d.Add(5, Resource.Report_FinancialStatus);
        //        d.Add(6, Resource.HiddenRequests);
        //    }

        //    if (User.IsInRole("Agent"))
        //    {
        //        d.Add(7, Resource.Report_ContractedClientsActivity);
        //        d.Add(8, Resource.Report_AcceptedRequests);
        //    }

        //    return View(new ReportViewModel(d));
        //}

        //[HttpPost]
        //public virtual ActionResult DatePicker(ReportViewModel model)
        //{       
        //        switch (model.Result)
        //        {
        //            case 1:
        //                return AgentsForAdmin(model);
        //            case 2:
        //                return InvoicesForAdmin(new Tuple<DateTime, DateTime>(model.StartDate, model.EndDate));
        //            case 3:
        //                return PaymentsForAdmin(new Tuple<DateTime, DateTime>(model.StartDate, model.EndDate));
        //            case 4:
        //                return AcceptedRequests(new Tuple<DateTime, DateTime>(model.StartDate, model.EndDate));
        //            case 5:
        //                return InvoicesForSeller(new Tuple<DateTime, DateTime>(model.StartDate, model.EndDate));
        //            case 6:
        //                return HiddenRequests(new Tuple<DateTime, DateTime>(model.StartDate, model.EndDate));
        //            case 7:
        //                return ContractedClientsActivity(new Tuple<DateTime, DateTime>(model.StartDate, model.EndDate));
        //            case 8:
        //                return AcceptedRequests(new Tuple<DateTime, DateTime>(model.StartDate, model.EndDate));
        //    }

        //    return RedirectToAction(MVC.Report.DatePicker());
        //}

        //[HttpGet]
        //[CustomAuthorize(Roles = "Dezmembrator, Admin, Agent")]
        //public virtual ActionResult AdminHiddenRequests(DateTime d1, DateTime d2, string userId)
        //{
        //    return HiddenRequests(new Tuple<DateTime,DateTime>(d1, d2), userId);
        //}

        //[HttpGet]
        //[CustomAuthorize(Roles = "Dezmembrator, Admin, Agent")]
        //public virtual ActionResult AdminInvoicesForSeller(DateTime d1, DateTime d2, string userId)
        //{
        //    return InvoicesForSeller(new Tuple<DateTime, DateTime>(d1, d2), userId);
        //}

        //[HttpPost]
        //[CustomAuthorize(Roles = "Dezmembrator, Admin, Agent")]
        //public virtual RazorPDF.PdfResult HiddenRequests(Tuple<DateTime, DateTime> tuple, string userId = null)
        //{
        //    var model = new HidderRequestsReportViewModel()
        //    {
        //        StartDate = tuple.Item1,
        //        EndDate = tuple.Item2
        //    };
        //    using (var db = new AppDbContext())
        //    {
        //        model.CarList = new List<Tuple<CarBrand, List<Tuple<string, string>>>>();
        //        var endDate = tuple.Item2.AddDays(1);

        //        if (userId == null)
        //            userId = User.Identity.GetUserId();

        //        var hiddenReq =
        //            (from r in db.Requests
        //             where
        //                 (from h in db.HiddenRequests where h.UserId == userId select h.RequestId).Contains(r.Id) &&
        //                 r.RequestDateTime < endDate && r.RequestDateTime >= tuple.Item1
        //             select r).ToList();
        //        model.Counter = hiddenReq.Count;

        //        foreach (var brand in Enum.GetValues(typeof(CarBrand)))
        //        {
        //            var brandFiltered = hiddenReq.Where(h => h.Brand == (CarBrand)brand).ToList();
        //            if (brandFiltered.Any())
        //            {
        //                var insideList =
        //                    brandFiltered.Select(bf => new Tuple<string, string>(bf.Model, bf.Title)).ToList();
        //                model.CarList.Add(new Tuple<CarBrand, List<Tuple<string, string>>>((CarBrand)brand, insideList));
        //            }
        //        }
        //    }
        //    return new PdfResult(model, "HiddenRequests");
        //}

        //[HttpPost]
        //[CustomAuthorize(Roles = "Agent")]
        //public virtual PdfResult ContractedClientsActivity(Tuple<DateTime, DateTime> tuple)
        //{
        //    var model = new ClientsActivityViewModel { StartDate = tuple.Item1, EndDate = tuple.Item2 };
        //    var userId = User.Identity.GetUserId();
        //    var endDate = tuple.Item2.AddDays(1);

        //    using (var db = new AppDbContext())
        //    {
        //        var allClients = db.Users.ToList().Where(u => UserManager.IsInRole(u.Id, "Client") || UserManager.IsInRole(u.Id, "Provider")).ToList();
        //        var clients = allClients.Where(c => c.CreatedDate >= tuple.Item1 && c.CreatedDate < endDate && c.AgentId == userId).ToList();
        //        if (clients.Any())
        //        {
        //            model.Clients = (from client in clients
        //                             let req =
        //                                 db.Requests.Where(
        //                                     r =>
        //                                         r.ClientId == client.Id && r.RequestDateTime >= tuple.Item1 &&
        //                                         r.RequestDateTime < endDate).ToList()
        //                             let noCompleted = req.Count(r => r.State == RequestState.Completed)
        //                             let noReturned = req.Count(r => r.State == RequestState.Returned)
        //                             select new Tuple<ApplicationUser, int, int, int>(client, req.Count, noCompleted, noReturned)).ToList();

        //            model.Clients.Sort((firstPair, nextPair) => firstPair.Item2.CompareTo(nextPair.Item2));
        //        }

        //        var allSuppliers = db.Users.ToList().Where(u => UserManager.IsInRole(u.Id, "Dezmembrator")).ToList();
        //        var suppliers = allSuppliers.Where(c => c.CreatedDate >= tuple.Item1 && c.CreatedDate < endDate && c.AgentId == userId).ToList();
        //        if (suppliers.Any())
        //        {
        //            model.Suppliers = (from supplier in suppliers
        //                               let offers =
        //                                   db.Offers.Where(
        //                                       r =>
        //                                           r.SupplierId == supplier.Id && r.Date >= tuple.Item1 &&
        //                                           r.Date < endDate && r.State != OfferState.Canceled).ToList()
        //                               let offAccepted = offers.Count(o => o.State == OfferState.Accepted)
        //                               select new Tuple<ApplicationUser, int, int>(supplier, offers.Count, offAccepted)).ToList();

        //            model.Suppliers.Sort((firstPair, nextPair) => firstPair.Item2.CompareTo(nextPair.Item2));
        //        }
        //    }
        //    return new PdfResult(model, "ClientsActivity");
        //}

        //[HttpPost]
        //[CustomAuthorize(Roles = "Admin")]
        //public virtual PdfResult PaymentsForAdmin(Tuple<DateTime, DateTime> tuple)
        //{
        //    var model = new List<PaymentReportViewModel>();
        //    var endDate = tuple.Item2.AddDays(1);

        //    using (var db = new AppDbContext())
        //    {
        //        //provider
        //        var providerPayments =
        //            db.ProviderInvoicePayments.Where(p => p.PaymentDate >= tuple.Item1 && p.PaymentDate < endDate).ToList();
        //        model.AddRange(providerPayments.Select(payment => new PaymentReportViewModel()
        //        {
        //            StartDate = tuple.Item1,
        //            EndDate = tuple.Item2,
        //            InvoiceSeries = payment.ProviderInvoiceHeader.Series,
        //            InvoiceNo = payment.ProviderInvoiceHeader.Number,
        //            IsProviderInvoice = true,
        //            Amount = payment.Amount,
        //            VatRate = payment.PaymentVatRate,
        //            PaymentDate = payment.PaymentDate,
        //            InvoiceType = payment.ProviderInvoiceHeader.InvoiceType,
        //            UserName =
        //                payment.ProviderInvoiceHeader.User.FirstName.IsEmpty() &&
        //                payment.ProviderInvoiceHeader.User.LastName.IsEmpty()
        //                    ? payment.ProviderInvoiceHeader.User.UserName
        //                    : payment.ProviderInvoiceHeader.User.FirstName + " " +
        //                      payment.ProviderInvoiceHeader.User.LastName
        //        }));

        //        //client
        //        var clientPayments =
        //            db.InvoicePayments.Where(p => p.PaymentDate >= tuple.Item1 && p.PaymentDate < endDate).ToList();
        //        model.AddRange(clientPayments.Select(payment => new PaymentReportViewModel()
        //        {
        //            StartDate = tuple.Item1,
        //            EndDate = tuple.Item2,
        //            PaymentMethod = payment.PaymentMethod,
        //            InvoiceSeries = payment.InvoiceHeader.Series,
        //            InvoiceNo = payment.InvoiceHeader.Number,
        //            IsProviderInvoice = false,
        //            Amount = payment.Amount,
        //            VatRate = payment.PaymentVatRate,
        //            PaymentDate = payment.PaymentDate,
        //            InvoiceType = InvoiceType.FacturaFiscala,
        //            UserName =
        //                payment.InvoiceHeader.User.FirstName.IsEmpty() &&
        //                payment.InvoiceHeader.User.LastName.IsEmpty()
        //                    ? payment.InvoiceHeader.User.UserName
        //                    : payment.InvoiceHeader.User.FirstName + " " +
        //                      payment.InvoiceHeader.User.LastName
        //        }));
        //    }

        //    return new PdfResult(model, "PaymentsForAdmin");
        //}

        //[HttpPost]
        //[CustomAuthorize(Roles = "Admin")]
        //public virtual PdfResult AgentsForAdmin(ReportViewModel model)
        //{
        //    var newModel = new List<AgentReportViewModel>();
        //    var endDate = model.EndDate.AddDays(1);

        //    using (var db = new AppDbContext())
        //    {
        //        var agents = db.Users.ToList().Where(u => UserManager.IsInRole(u.Id, "Agent")).ToList();

        //        newModel.AddRange(from agent in agents
        //                          let sNo = db.Users.Where(u => u.AgentId == agent.Id && u.CreatedDate < endDate && u.CreatedDate >= model.StartDate).ToList().Count(u => UserManager.IsInRole(u.Id, "Dezmembrator"))
        //                          let cNo = db.Users.Where(u => u.AgentId == agent.Id && u.CreatedDate < endDate && u.CreatedDate >= model.StartDate).ToList().Count(u => UserManager.IsInRole(u.Id, "Provider"))
        //                          select new AgentReportViewModel
        //                          {
        //                              StartDate = model.StartDate,
        //                              EndDate = model.EndDate,
        //                              AgentName = agent.FirstName + " " + agent.LastName,
        //                              SellersNo = sNo,
        //                              CustomersNo = cNo
        //                          });
        //    }

        //    return new PdfResult(newModel, "AgentsForAdmin");
        //}

        //[HttpPost]
        //[CustomAuthorize(Roles = "Admin, Agent")]
        //public virtual ActionResult AcceptedRequests(Tuple<DateTime, DateTime> tuple)
        //{
        //    var model = new List<AcceptedRequestsViewModel>();
        //    var startDate = tuple.Item1;
        //    var endDate = tuple.Item2.AddDays(1);

        //    using (var db = new AppDbContext())
        //    {
        //        var offers =
        //            db.Offers.Include("Request")
        //                .Where(m => m.Date >= startDate && m.Date < endDate && m.State == OfferState.Accepted)
        //                .ToList();

        //        foreach (var o in offers)
        //        {
        //            var arvm = new AcceptedRequestsViewModel(startDate, endDate)
        //            {
        //                TitleRequest = o.Request.Title,
        //                State = o.Request.State.ToString(),
        //                RequestDateTime = o.Request.RequestDateTime,
        //                OfferDateTime = o.Date,
        //                Price = o.Price
        //            };

        //            if (o.Request.Client.IsCompany)
        //            {
        //                arvm.ClientName = o.Request.Client.CompanyName;
        //            }
        //            else if (o.Request.Client.FirstName.IsNullOrWhiteSpace() && o.Request.Client.LastName.IsNullOrWhiteSpace())
        //            {
        //                arvm.ClientName = o.Request.Client.UserName;
        //            }
        //            else
        //            {
        //                arvm.ClientName = o.Request.Client.FirstName + " " + o.Request.Client.LastName;
        //            }

        //            if (o.Supplier.IsCompany)
        //            {
        //                arvm.SupplierName = o.Supplier.CompanyName;
        //            }
        //            else if (o.Supplier.FirstName.IsNullOrWhiteSpace() && o.Supplier.LastName.IsNullOrWhiteSpace())
        //            {
        //                arvm.SupplierName = o.Supplier.UserName;
        //            }
        //            else
        //            {
        //                arvm.SupplierName = o.Supplier.FirstName + " " + o.Supplier.LastName;
        //            }

        //            model.Add(arvm);
        //        }

        //        model = model.OrderBy(m => m.SupplierName).ToList();
        //    }
        //    return new PdfResult(model, "AcceptedRequests");
        //}

        //[HttpPost]
        //[CustomAuthorize(Roles = "Admin")]
        //public virtual ActionResult InvoicesForAdmin(Tuple<DateTime, DateTime> tuple)
        //{
        //    var model = new InvoicesReportViewModel(tuple.Item1, tuple.Item2);
        //    var endDate = tuple.Item2.AddDays(1);

        //    using (var db = new AppDbContext())
        //    {
        //        model.AllRequests =
        //            db.Requests.Count(m => m.RequestDateTime >= model.StartDate && m.RequestDateTime <= model.EndDate);
        //        model.AllBids = (from o in db.Offers
        //                         where
        //                             o.Date >= model.StartDate && o.Date <= endDate && o.State != OfferState.Canceled
        //                         select o.RequestId).Distinct().Count();

        //        var allWonBids = (from o in db.Offers.Include("Request")
        //                         where
        //                             o.Date >= model.StartDate && o.Date <= endDate && o.State == OfferState.Accepted
        //                         select o).ToList();
        //        model.AllWonBids = allWonBids.Count();

        //        model.AmountWon = 0;
        //        foreach (var bid in allWonBids)
        //        {
        //            if(bid.Request.DeliveryAddressId == null) continue;

        //            //var address = db.Addresses.FirstOrDefault(a => a.Id == bid.Request.DeliveryAddressId);
        //            //decimal transp = bid.Request.PriceForTransport ?? 0;
        //            model.AmountWon += bid.Request.Price ?? 0 - CommissionEngine.GetSupplierPriceForOffer(bid.Id, true);

        //        }

        //        var allFinalizedBids = allWonBids.Where(m => m.Request.State == RequestState.Completed).ToList();
        //        model.AllFinalizedBids = allFinalizedBids.Count();

        //        model.FinalizedAmountWon = 0;
        //        foreach (var bid in allFinalizedBids)
        //        {
        //            //var address = db.Addresses.FirstOrDefault(a => a.Id == bid.Request.DeliveryAddressId);
        //            //decimal transp = bid.Request.PriceForTransport ?? 0;
        //            model.FinalizedAmountWon += bid.Price - CommissionEngine.GetSupplierPriceForOffer(bid.Id, true);
        //        }

        //        //PROVIDER
        //        var allGeneratedBills =
        //           (from i in db.ProviderInvoiceHeaders
        //            where i.Date >= model.StartDate && i.Date <= endDate
        //            select i).ToList();

        //        model.AllGeneratedBills = allGeneratedBills.Count();
        //        model.TotalAmountGeneratedBillsWithoutVat = model.AllGeneratedBills == 0
        //            ? 0
        //            : allGeneratedBills.Sum(m => InvoiceUtility.GetProviderInvoiceAmountWithoutVat(m.Id));
        //        model.TotalAmountGeneratedBillsIncludingVat = model.AllGeneratedBills == 0
        //            ? 0
        //            : allGeneratedBills.Sum(m => InvoiceUtility.GetProviderInvoiceTotalAmount(m.Id));

        //        var allPayedBills = (from i in db.ProviderInvoiceHeaders
        //                             where
        //                                 i.Date >= model.StartDate && i.Date <= endDate &&
        //                                 i.ProviderInvoicePayments.FirstOrDefault().PaymentDate < endDate
        //                             select i).ToList();
        //        allPayedBills = allPayedBills.FindAll(i => InvoiceUtility.IsProviderInvoicePayed(i.Id));

        //        model.AllPayedBills = allPayedBills.Count();
        //        model.TotalAmountPayedBillsWithoutVat = model.AllPayedBills == 0
        //            ? 0
        //            : allPayedBills.Sum(m => InvoiceUtility.GetProviderInvoiceAmountWithoutVat(m.Id));
        //        model.TotalAmountPayedBillsIncludingVat = model.AllPayedBills == 0
        //            ? 0
        //            : allPayedBills.Sum(m => InvoiceUtility.GetProviderInvoiceTotalAmount(m.Id));

        //        //CLIENT
        //        var allClientGeneratedBills =
        //           (from i in db.InvoiceHeaders
        //            where i.Date >= model.StartDate && i.Date <= endDate
        //            select i).ToList();

        //        model.AllClientGeneratedBills = allClientGeneratedBills.Count();
        //        model.TotalAmountClientGeneratedBillsWithoutVat = model.AllClientGeneratedBills == 0
        //            ? 0
        //            : allClientGeneratedBills.Sum(m => InvoiceUtility.GetInvoiceAmountWithoutVat(m.Id));
        //        model.TotalAmountClientGeneratedBillsIncludingVat = model.AllClientGeneratedBills == 0
        //            ? 0
        //            : allClientGeneratedBills.Sum(m => InvoiceUtility.GetInvoiceTotalAmount(m.Id));

        //        var allClientPayedBills = (from i in db.InvoiceHeaders
        //                                   where
        //                                       i.Date >= model.StartDate && i.Date <= endDate &&
        //                                       i.InvoicePayments.FirstOrDefault().PaymentDate < endDate
        //                                   select i).ToList();
        //        allClientPayedBills = allClientPayedBills.FindAll(i => InvoiceUtility.IsInvoicePayed(i.Id));

        //        model.AllClientPayedBills = allClientPayedBills.Count();
        //        model.TotalAmountClientPayedBillsWithoutVat = model.AllClientPayedBills == 0
        //            ? 0
        //            : allClientPayedBills.Sum(m => InvoiceUtility.GetInvoiceAmountWithoutVat(m.Id));
        //        model.TotalAmountClientPayedBillsIncludingVat = model.AllClientPayedBills == 0
        //            ? 0
        //            : allClientPayedBills.Sum(m => InvoiceUtility.GetInvoiceTotalAmount(m.Id));
        //    }

        //    return new PdfResult(model, "InvoicesForAdmin");
        //}

        //[HttpGet]
        //[CustomAuthorize(Roles = "Dezmembrator, Admin, Agent")]
        //public virtual ActionResult InvoicesForSeller(Tuple<DateTime, DateTime> tuple, string userId = null)
        //{
        //    var model = new InvoicesReportViewModel(tuple.Item1, tuple.Item2);
        //    var endDate = tuple.Item2.AddDays(1);

        //    using (var db = new AppDbContext())
        //    {               
        //        if(userId == null)
        //            userId = User.Identity.GetUserId();
        //        var brands = from pb in db.PreferentialBrands where pb.UserId == userId select pb.Brand;

        //        model.AllBids = (from o in db.Offers
        //                         where
        //                             o.SupplierId == userId && o.Date >= model.StartDate && o.Date < endDate && o.State != OfferState.Canceled
        //                         select o).Count();

        //        var allWonBids = from o in db.Offers
        //                         where
        //                             o.SupplierId == userId && o.Date >= model.StartDate && o.Date < endDate &&
        //                             o.State == OfferState.Accepted
        //                         select o;

        //        model.AllWonBids = allWonBids.Count();
        //        model.AmountWon = model.AllWonBids == 0 ? 0 : allWonBids.Sum(m => m.Price);

        //        model.AllRequests = brands.Any()
        //            ? (from r in db.Requests
        //               where
        //                   brands.Contains(r.Brand) && r.RequestDateTime >= model.StartDate && r.RequestDateTime < endDate
        //               select r).Count()
        //            : (from r in db.Requests
        //               where
        //                   r.RequestDateTime >= model.StartDate && r.RequestDateTime < endDate
        //               select r).Count();

        //        var allGeneratedBills =
        //            (from i in db.ProviderInvoiceHeaders
        //             where i.UserId == userId && i.Date >= model.StartDate && i.Date < endDate
        //             select i).ToList();
        //        model.AllGeneratedBills = allGeneratedBills.Count();
        //        model.TotalAmountGeneratedBillsWithoutVat = model.AllGeneratedBills == 0
        //            ? 0
        //            : allGeneratedBills.Sum(m => InvoiceUtility.GetProviderInvoiceAmountWithoutVat(m.Id));
        //        model.TotalAmountGeneratedBillsIncludingVat = model.AllGeneratedBills == 0
        //            ? 0
        //            : allGeneratedBills.Sum(m => InvoiceUtility.GetProviderInvoiceTotalAmount(m.Id));

        //        var allPayedBills = (from i in db.ProviderInvoiceHeaders
        //                             where
        //                                 i.UserId == userId && i.Date >= model.StartDate && i.Date < endDate &&
        //                                 i.ProviderInvoicePayments.FirstOrDefault().PaymentDate < endDate
        //                             select i).ToList();
        //        allPayedBills = allPayedBills.FindAll(i => InvoiceUtility.IsProviderInvoicePayed(i.Id));

        //        model.AllPayedBills = allPayedBills.Count();
        //        model.TotalAmountPayedBillsWithoutVat = model.AllPayedBills == 0
        //            ? 0
        //            : allPayedBills.Sum(m => InvoiceUtility.GetProviderInvoiceAmountWithoutVat(m.Id));
        //        model.TotalAmountPayedBillsIncludingVat = model.AllPayedBills == 0
        //            ? 0
        //            : allPayedBills.Sum(m => InvoiceUtility.GetProviderInvoiceTotalAmount(m.Id));

        //        return new PdfResult(model, "InvoicesForSeller");
        //    }
        //}

        //#endregion

    }
}