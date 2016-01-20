using MaProgramez.Repository.BusinessLogic;
using MaProgramez.Repository.DbContexts;
using MaProgramez.Repository.Entities;

namespace MaProgramez.Website.Controllers
{
    using Helpers;
    using Microsoft.AspNet.Identity;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Web;
    using System.Web.Mvc;
    using Utility;
    using ViewModels;

    public partial class BaseController : Controller
    {
        #region PROPERTIES

        public int PageSize = 10;

        public string CurrentCulture { get; set; }

        #endregion PROPERTIES

        #region OVERRIDE METHODS

        protected override IAsyncResult BeginExecuteCore(AsyncCallback callback, object state)
        {
            if (!GetCulture())
            {
                // TODO - Real redirect not only RESPONSE.REDIRECT (IMPORANT FOR SERVICE CONTROLLER)
                Response.RedirectToRoute(RouteData.Values);
            }

            ViewBag.Environment = Common.GetDbConfig("Environment").ToUpper();

            var userId = User.Identity.GetUserId();

            if (User.IsInRole("Provider"))
            {
                
                var slots = RetrieveLists.GetSlotsByProvider(userId);
                var slotIds = slots.Select(s => s.Id).ToList();
                var slotSchedulesCount = RetrieveLists.GetSlotSchedulesCount(slotIds);
                var slotPendingSchedulesCount = RetrieveLists.GetSlotSchedulesCount(slotIds, true);
                var slotClientsCount = RetrieveLists.GetSlotClientsCount(slotIds, userId);

                ViewBag.Slots = slots;
                ViewBag.SlotSchedulesCount = slotSchedulesCount;
                ViewBag.SlotPendingSchedulesCount = slotPendingSchedulesCount;
                ViewBag.SlotClientsCount = slotClientsCount;
            }
            else if (User.IsInRole("Employee"))
            {
                ViewBag.SlotId = RetrieveOthers.GetSlotIdByUserId(userId);
            }

            GetNotifications();
            GetPageAttributes();
            ViewBag.AcceptCookies = GetCookiesFlag();

            return base.BeginExecuteCore(callback, state);
        }

        #endregion OVERRIDE METHODS

        #region PUBLIC METHODS

        public virtual ActionResult RedirectToConfirmation(ConfirmationViewModel viewModel)
        {
            TempData["ConfirmationViewModel"] = viewModel;
            return RedirectToAction(MVC.Home.Confirmation());
        }

        public void SetSubMenu(List<MenuLink> menuLinks)
        {
            if (menuLinks != null && menuLinks.Any())
            {
                ViewBag.SubMenu = menuLinks;
            }
        }

        public string RenderRazorViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(ControllerContext,
                    viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View,
                    ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }

        #endregion PUBLIC METHODS

        #region PRIVATE METHODS

        private bool GetCulture()
        {
            var cultureName = RouteData.Values["culture"] as string;
            HttpCookie cookie = Request.Cookies["_culture"];

            //Read culture from cookie if no culture in route
            if (cookie != null && string.IsNullOrWhiteSpace(cultureName))
                cultureName = cookie.Value;

            // Attempt to read the culture cookie from Request
            // SERGIU - DON'T TAKE CULTURE FROM BROWSER
            /*
            if (string.IsNullOrWhiteSpace(cultureName))
            {
                cultureName = Request.UserLanguages != null && Request.UserLanguages.Length > 0
                    ? Request.UserLanguages[0]
                    : null; // obtain it from HTTP header AcceptLanguages
            }
            */

            // Save culture in a cookie
            if (cookie != null)
                cookie.Value = cultureName; // update cookie value
            else
            {
                cookie = new HttpCookie("_culture");
                cookie.Value = cultureName;
                cookie.Expires = DateTime.Now.AddYears(1);
            }
            Response.Cookies.Add(cookie);

            // Validate culture name
            cultureName = CultureHelper.GetImplementedCulture(cultureName); // This is safe

            // Modify current thread's cultures
            Thread.CurrentThread.CurrentCulture = new CultureInfo(cultureName);
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

            if (RouteData.Values["culture"] as string != cultureName)
            {
                // Force a valid culture in the URL
                RouteData.Values["culture"] = cultureName.ToLowerInvariant(); // lower case too

                // NOT REDIRECT IF IS SERVICE CONTROLLER
                var controller = RouteData.Values["controller"] as string;
                if (controller != null && controller.ToLower() == "service")
                {
                    return true;
                }

                // Redirect user
                //Response.RedirectToRoute(RouteData.Values);
                ViewBag.Culture = cultureName;
                CurrentCulture = cultureName;

                return false;
            }

            ViewBag.Culture = cultureName;
            CurrentCulture = cultureName;

            return true;
        }

        private void GetNotifications()
        {
            using (var db = new AppDbContext())
            {
                string userId = User.Identity.GetUserId();
                List<Notification> notifications =
                    db.Notifications.Where(n => n.UserId == userId && n.IsRead == false && n.IsDeleted == false)
                        .OrderByDescending(n => n.CreatedDate)
                        .ToList();

                ViewBag.Notifications = notifications;
            }
        }

        private void GetPageAttributes()
        {
            ViewBag.CurrentAction = RouteData.Values["action"] as string;
            ViewBag.CurrentController = RouteData.Values["controller"] as string;
        }

        private bool GetCookiesFlag()
        {
            if (Request != null && Request.Cookies != null && Request.Cookies.Count > 0)
            {
                HttpCookie existingCookie = Request.Cookies["accept-cookies"];
                if (existingCookie != null)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion PRIVATE METHODS
    }
}