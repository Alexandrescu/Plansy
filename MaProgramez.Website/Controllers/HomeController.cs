using MaProgramez.Repository.BusinessLogic;
using MaProgramez.Repository.DbContexts;
using System;
using System.Linq;

namespace MaProgramez.Website.Controllers
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.Owin;
    using MaProgramez.Website.Extensions;
    using MaProgramez.Website.ViewModels;
    using System.Web;
    using System.Web.Mvc;

    [RequireHttps]
    [CustomAuthorize]
    public partial class HomeController : BaseController
    {
        #region Private Fields

        private AppDbContext _db;
        private ApplicationRoleManager _roleManager;
        private ApplicationUserManager _userManager;

        #endregion Private Fields

        #region Public Properties

        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }

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

        #endregion Public Properties

        #region Public Methods
        
        /// <summary>
        /// Confirmations this instance.
        /// </summary>
        /// <returns></returns>
        public virtual ActionResult Confirmation()
        {
            var viewModel = TempData["ConfirmationViewModel"] as ConfirmationViewModel;
            if (viewModel != null)
            {
                TempData.Remove("ConfirmationViewModel");
                return this.View(viewModel);
            }

            return this.RedirectToAction(MVC.Home.Index(null, string.Empty));
        }

        public DashboardViewModel GetDashboardForAdmin()
        {         
            var viewModel = new DashboardViewModel()
            {
                TotalAppointmentsToday = RetrieveOthers.NoSchedulesByDate(DateTime.Now),
                TotalClients = RetrieveLists.GetUsersInRole("Client").Count(),
                TotalProviders = RetrieveLists.GetUsersInRole("Provider").Count()
            };

            return viewModel;
        }

        public DashboardViewModel GetDashboardForClient()
        {
            var userId = User.Identity.GetUserId();
            var viewModel = new DashboardViewModel()
            {
                FavouriteProvidersList = RetrieveLists.GetFavorites(userId),
                NewProvidersInTown = RetrieveLists.GetNewProvidersInTown(userId),
                TotalAppointmentsToday = RetrieveLists.GetClientSchedulesByDate(userId, DateTime.Now).Count,
                TotalAppointmentsThisWeek = RetrieveLists.GetClientSchedulesCountByWeek(userId, DateTime.Now),
                FavouriteProviders = RetrieveLists.GetFavoriteProviders(userId).Count,
                LastFiveSchedules = RetrieveLists.GetClientLastFiveSchedules(userId),
                NextFiveSchedules = RetrieveLists.GetClientNextFiveSchedules(userId)
            };

            return viewModel;
        }

        public DashboardViewModel GetDashboardForProvider()
        {
            var userId = User.Identity.GetUserId();
           
            var viewModel = new DashboardViewModel()
            {
                TotalAppointmentsToday = RetrieveLists.GetSchedulesCountByProvider(userId, DateTime.Now),
                PendingAppointments = RetrieveLists.GetSchedulesCountByProvider(userId, DateTime.Now, true),
                TotalClients = RetrieveLists.GetClientsByProvider(userId, null).Count(),
                NextFiveSchedules = RetrieveLists.GetProvidersNextFiveSchedules(userId, null),
                LastFiveSchedules = RetrieveLists.GetProvidersLastFiveSchedules(userId, null),
                UnpayedInvoices = RetrieveLists.GetUnpaidInvoices(userId)
            };

            return viewModel;
        }

        public DashboardViewModel GetDashboardForSlot()
        {
            var userId = User.Identity.GetUserId();
            var slotId = RetrieveOthers.GetSlotIdByUserId(userId);

            var viewModel = new DashboardViewModel()
            {
                TotalAppointmentsToday = RetrieveLists.GetSchedulesByDateAndSlot(slotId, DateTime.Now).Count,
                PendingAppointments = RetrieveLists.GetSchedulesByProviderSlot(slotId, true).Count,
                TotalClients = RetrieveLists.GetClientsByProvider(null, slotId).Count(),
                NextFiveSchedules = RetrieveLists.GetProvidersNextFiveSchedules(null, slotId),
                LastFiveSchedules = RetrieveLists.GetProvidersLastFiveSchedules(null, slotId)
            };

            return viewModel;
        }

        [AllowAnonymous]
        public virtual ActionResult New(LandingPageViewModel model = null)
        {
            return this.View(MVC.Shared.Views.NewLandingPage, model);
        }

        [AllowAnonymous]
        [HttpPost]
        public virtual ActionResult SendContactMessage(LandingPageViewModel model)
        {
            Utility.MailAndSmsUtility.SendEmail("codrin@fellan-soft.com", "Plansy Contact Form", "Name: " + model.Name + "\n\r Email: " + model.Email + "\n\r Message: " + model.Message);
            model.Name = null; model.Email = null; model.Message = "MESSAGE SUCCESSFULLY SENT !!!";

            return RedirectToAction(MVC.Home.New(model));
        }

        /// <summary>
        /// Indexes the specified page.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="sortBy">The sort by.</param>
        /// <returns></returns>
        [AllowAnonymous]
        public virtual ActionResult Index(int? page, string sortBy)
        {
            if (_db == null)
            {
                _db = new AppDbContext();
            }

            if (!User.Identity.IsAuthenticated)
            {
                var model = new LandingPageViewModel
                {
                    SuppliersTotal = 0,
                    ClientsTotal = 0,                 
                    OffersTotal = 0,
                    Logos = null,               
                    Suppliers = null
                };

                return this.View(MVC.Shared.Views.NewLandingPage, model);
            }
            else
            {               
                DashboardViewModel dashboard = null;
                if (User.IsInRole("Admin") || User.IsInRole("Agent"))
                {
                    dashboard = GetDashboardForAdmin();
                }
                else if (User.IsInRole("Client"))
                {
                    dashboard = GetDashboardForClient();
                }
                else if (User.IsInRole("Employee"))
                {
                    dashboard = GetDashboardForSlot();
                }
                else if (User.IsInRole("Provider"))
                {
                    dashboard = GetDashboardForProvider();
                }
             
                this.ViewBag.Page = page.HasValue ? page : 0;
                this.ViewBag.PageSize = this.PageSize;
                this.ViewBag.SortBy = sortBy;
                
                return View(dashboard);
            }
        }
        
        #endregion Public Methods
        
        #region Protected Methods

        protected override void Dispose(bool disposing)
        {
            if (_db != null)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion Protected Methods
    }
}