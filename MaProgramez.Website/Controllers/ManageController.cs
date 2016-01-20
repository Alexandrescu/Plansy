using MaProgramez.Repository.BusinessLogic;
using MaProgramez.Repository.DbContexts;
using MaProgramez.Repository.Entities;

namespace MaProgramez.Website.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;
    using Extensions;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.Owin;
    using Microsoft.Owin.Security;
    using Models;
    using MvcPaging;
    using Resources;
    using Utility;
    using ViewModels;

    [CustomAuthorize]
    public partial class ManageController : BaseController
    {
        #region Private Fields

        private ApplicationUserManager _userManager;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ManageController" /> class.
        /// </summary>
        public ManageController()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ManageController" /> class.
        /// </summary>
        /// <param name="userManager">The user manager.</param>
        public ManageController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        ///     Gets the user manager.
        /// </summary>
        /// <value>
        ///     The user manager.
        /// </value>
        public ApplicationUserManager UserManager
        {
            get { return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
            private set { _userManager = value; }
        }

        #endregion Public Properties

        #region Public Methods

        //
        // GET: /Account/AddPhoneNumber
        public virtual ActionResult AddPhoneNumber()
        {
            ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());
            return View(new AddPhoneNumberViewModel {Number = user.PhoneNumber});
        }

        //
        // POST: /Account/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Generate the token and send it
            string code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), model.Number);
            if (UserManager.SmsService != null)
            {
                var message = new IdentityMessage
                {
                    Destination = model.Number,
                    Body = Resource.YourSecurityCodeIs + code
                };
                await UserManager.SmsService.SendAsync(message);
            }
            return RedirectToAction("VerifyPhoneNumber", new {PhoneNumber = model.Number});
        }

        //
        // GET: /Manage/ChangePassword
        public virtual ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Account/Manage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            IdentityResult result =
                await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                ApplicationUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInAsync(user, false);
                }
                return RedirectToAction("Index", new {Message = ManageMessageId.ChangePasswordSuccess});
            }
            AddErrors(result);
            return View(model);
        }

        [CustomAuthorize(Roles = "Admin, Agent")]
        public virtual ActionResult ContractsList(int? page, string sortBy, string filterBy, bool ascending = true)
        {
            int currentPageIndex = page.HasValue ? page.Value - 1 : 0;

            int filterByInt;
            int.TryParse(filterBy, out filterByInt);

            using (var db = new AppDbContext())
            {
               var contracts =
                    db.Users.Include("Addresses")
                        .Include("Agent")
                        .Where(ih => ih.ContractNo != null && ih.ContractDate != null && ih.AgentId != null).ToList();

                if (!string.IsNullOrWhiteSpace(filterBy))
                {
                    contracts = contracts.Where(x => x.ContractNo == filterByInt ||
                                                     x.CompanyName.Contains(filterBy) || 
                                                     x.Alias.Contains(filterBy) || 
                                                     x.FirstName.Contains(filterBy) ||
                                                     x.LastName.Contains(filterBy) ||
                                                     x.Agent.FirstName.Contains(filterBy) ||
                                                     x.Agent.LastName.Contains(filterBy)).ToList();
                }

                var ivms = new List<ContractListItemViewModel>();
                foreach (ApplicationUser c in contracts)
                {
                    var n = new ContractListItemViewModel();
                    int id = c.Addresses.Any(m => m.AddressType == AddressType.InvoiceAddress)
                        ? c.Addresses.First(m => m.AddressType == AddressType.InvoiceAddress).Id
                        : 0;

                    n.Address = id != 0
                        ? db.Addresses.Include("UserCity")
                            .Include("UserCity.CityCounty")
                            .Include("UserCountry")
                            .First(m => m.Id == id)
                            .ToString()
                        : "-";

                    n.ContractNo = (int) c.ContractNo;
                    n.ClientName = c.CompanyName ?? c.FirstName + " " + c.LastName;
                    n.ContractDate = (DateTime) c.ContractDate;
                    n.Agent = c.Agent.FirstName + " " + c.Agent.LastName;

                    ivms.Add(n);
                }

                List<ContractListItemViewModel> sortedList = null;

                switch (sortBy)
                {
                    case "number":
                        sortedList = ascending
                            ? ivms.OrderBy(x => x.ContractNo).ToList()
                            : ivms.OrderByDescending(x => x.ContractNo).ToList();
                        break;

                    case "date":
                        sortedList = ascending
                            ? ivms.OrderBy(x => x.ContractDate).ToList()
                            : ivms.OrderByDescending(x => x.ContractDate).ToList();
                        break;

                    case "clientName":
                        sortedList = ascending
                            ? ivms.OrderBy(x => x.ClientName).ToList()
                            : ivms.OrderByDescending(x => x.ClientName).ToList();
                        break;

                    case "agentName":
                        sortedList = ascending
                            ? ivms.OrderBy(x => x.Agent).ToList()
                            : ivms.OrderByDescending(x => x.Agent).ToList();
                        break;

                    case "address":
                        sortedList = ascending
                            ? ivms.OrderBy(x => x.Address).ToList()
                            : ivms.OrderByDescending(x => x.Address).ToList();
                        break;

                    default:
                        sortedList = ivms.OrderByDescending(x => x.ContractDate).ToList();
                        break;
                }

                ViewBag.SortBy = sortBy;
                ViewBag.FilterBy = filterBy;
                ViewBag.Ascending = ascending;
                ViewBag.Page = currentPageIndex;

                return View(sortedList.ToPagedList(currentPageIndex, 10));
            }
        }

        //
        // POST: /Manage/DisableTFA
        [HttpPost]
        public virtual async Task<ActionResult> DisableTFA()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), false);
            ApplicationUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInAsync(user, false);
            }
            return RedirectToAction(MVC.Manage.Index());
        }

        //
        // POST: /Manage/EnableTFA
        [HttpPost]
        public virtual async Task<ActionResult> EnableTFA()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), true);
            ApplicationUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInAsync(user, false);
            }
            return RedirectToAction(MVC.Manage.Index());
        }

        //
        // POST: /Manage/ForgetBrowser
        [HttpPost]
        public virtual ActionResult ForgetBrowser()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);
            return RedirectToAction(MVC.Manage.Index());
        }

        // GET: /Account/Index
        public virtual ActionResult Index()
        {
            using (var db = new AppDbContext())
            {
                var userId = User.Identity.GetUserId();
                var user =
                    db.Users.Include("Addresses")
                        .Include("Addresses.UserCity")
                        .Include("Addresses.UserCity.CityCounty")
                        .First(x => x.Id == userId);
                ViewBag.Counties = db.Counties.OrderBy(x => x.Name).ToList();

                return View(user);
            }
        }

        //
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new AccountController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"),
                User.Identity.GetUserId());
        }

        //
        // GET: /Manage/LinkLoginCallback
        public virtual async Task<ActionResult> LinkLoginCallback()
        {
            ExternalLoginInfo loginInfo =
                await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("ManageLogins", new {Message = ManageMessageId.Error});
            }
            IdentityResult result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            return result.Succeeded
                ? RedirectToAction("ManageLogins")
                : RedirectToAction("ManageLogins", new {Message = ManageMessageId.Error});
        }

        //
        // GET: /Account/Manage
        public virtual async Task<ActionResult> ManageLogins(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.RemoveLoginSuccess
                    ? Resource.RemoveLoginSuccess
                    : message == ManageMessageId.Error
                        ? Resource.Error
                        : "";
            ApplicationUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                return View("Error");
            }
            IList<UserLoginInfo> userLogins = await UserManager.GetLoginsAsync(User.Identity.GetUserId());
            List<AuthenticationDescription> otherLogins =
                AuthenticationManager.GetExternalAuthenticationTypes()
                    .Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider))
                    .ToList();
            ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }
        
        //
        // POST: /Manage/RememberBrowser
        [HttpPost]
        public virtual ActionResult RememberBrowser()
        {
            ClaimsIdentity rememberBrowserIdentity =
                AuthenticationManager.CreateTwoFactorRememberBrowserIdentity(User.Identity.GetUserId());
            AuthenticationManager.SignIn(new AuthenticationProperties {IsPersistent = true}, rememberBrowserIdentity);
            return RedirectToAction(MVC.Manage.Index());
        }

        //
        // GET: /Account/RemoveLogin
        public virtual ActionResult RemoveLogin()
        {
            IList<UserLoginInfo> linkedAccounts = UserManager.GetLogins(User.Identity.GetUserId());
            ViewBag.ShowRemoveButton = HasPassword() || linkedAccounts.Count > 1;
            return View(linkedAccounts);
        }

        //
        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            ManageMessageId? message;
            IdentityResult result =
                await
                    UserManager.RemoveLoginAsync(User.Identity.GetUserId(),
                        new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                ApplicationUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInAsync(user, false);
                }
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("ManageLogins", new {Message = message});
        }

        //
        // GET: /Account/RemovePhoneNumber
        public virtual async Task<ActionResult> RemovePhoneNumber()
        {
            IdentityResult result = await UserManager.SetPhoneNumberAsync(User.Identity.GetUserId(), null);
            if (!result.Succeeded)
            {
                return RedirectToAction("Index", new {Message = ManageMessageId.Error});
            }
            ApplicationUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInAsync(user, false);
            }
            return RedirectToAction("Index", new {Message = ManageMessageId.RemovePhoneSuccess});
        }

        //
        // GET: /Manage/SetPassword
        public virtual ActionResult SetPassword()
        {
            return View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                if (result.Succeeded)
                {
                    ApplicationUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if (user != null)
                    {
                        await SignInAsync(user, false);
                    }
                    return RedirectToAction("Index", new {Message = ManageMessageId.SetPasswordSuccess});
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public virtual async Task<ActionResult> Settings(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess
                    ? Resource.ChangePasswordSuccess
                    : message == ManageMessageId.SetPasswordSuccess
                        ? Resource.SetPasswordSuccess
                        : message == ManageMessageId.SetTwoFactorSuccess
                            ? Resource.SetTwoFactorSuccess
                            : message == ManageMessageId.Error
                                ? Resource.Error
                                : message == ManageMessageId.AddPhoneSuccess
                                    ? Resource.AddPhoneSuccess
                                    : message == ManageMessageId.RemovePhoneSuccess
                                        ? Resource.RemovePhoneSuccess
                                        : message == ManageMessageId.SaveUserDetailsSuccess
                                            ? Resource.SaveUserDetailsSuccess
                                            : "";

            ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());
            var addresses = RetrieveOthers.GetUserAddresses(user.Id);

            var model = new IndexViewModel
            {
                HasPassword = HasPassword(),
                PhoneNumber = await UserManager.GetPhoneNumberAsync(User.Identity.GetUserId()),
                TwoFactor = await UserManager.GetTwoFactorEnabledAsync(User.Identity.GetUserId()),
                Logins = await UserManager.GetLoginsAsync(User.Identity.GetUserId()),
                BrowserRemembered =
                    await AuthenticationManager.TwoFactorBrowserRememberedAsync(User.Identity.GetUserId()),
                LastName = user.LastName,
                FirstName = user.FirstName,
                Addresses = addresses,
               
            };

            return View(model);
        }

        [HttpPost]
        public virtual ActionResult UpdateUserDetails(IndexViewModel model)
        {
            ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());

            user.LastName = model.LastName;
            user.FirstName = model.FirstName;
            user.Addresses = model.Addresses;
           
            UserManager.UpdateAsync(user);

            // TODO: check if this is enough -  I think it should attach the user to the context first
            using (var db = new AppDbContext())
            {
                db.SaveChanges();
            }
            ViewBag.StatusMessage = Resource.User_Saved;

            return RedirectToAction("Index", new {Message = ManageMessageId.SaveUserDetailsSuccess});
        }

        //
        // GET: /Account/VerifyPhoneNumber
        public virtual ActionResult VerifyPhoneNumber(string phoneNumber)
        {
            // This code allows you exercise the flow without actually sending codes
            // For production use please register a SMS provider in IdentityConfig and generate a code here.
            //var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), phoneNumber);
            //ViewBag.Status = "For DEMO purposes only, the current code is " + code;
            return phoneNumber == null
                ? View("Error")
                : View(new VerifyPhoneNumberViewModel {PhoneNumber = phoneNumber});
        }

        //
        // POST: /Account/VerifyPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            IdentityResult result =
                await UserManager.ChangePhoneNumberAsync(User.Identity.GetUserId(), model.PhoneNumber, model.Code);
            if (result.Succeeded)
            {
                ApplicationUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInAsync(user, false);
                }
                return RedirectToAction("Index", new {Message = ManageMessageId.AddPhoneSuccess});
            }
            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", Resource.FailedToVerifyCode);
            return View(model);
        }

        #endregion Public Methods

        #region Protected Methods

        /// <summary>
        ///     Called before the action method is invoked.
        /// </summary>
        /// <param name="filterContext">Information about the current request and action.</param>
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var menuLinks = new List<MenuLink>
            {
                new MenuLink("Manage", "Index", Resource.User_Personaldetails, "index", "fa fa-user"),
                new MenuLink("Manage", "ChangePassword", Resource.ChangePassword, "changepassword", "fa fa-lock"),
                new MenuLink("Manage", "AddPhoneNumber", Resource.User_ChangePhone, "addphonenumber,verifyphonenumber",
                    "fa fa-phone"),
                new MenuLink("Manage", "ManageLogins", Resource.User_Connect, "managelogins", "fa fa-link"),
            };

            if (User.IsInRole("Provider") || User.IsInRole("Slot"))
            {
                menuLinks.Add(new MenuLink("Manage", "InvoiceList", Resource.MyInvoices, "invoicelist",
                    "fa fa-file-text-o"));
            }

            /*
            if (User.IsInRole("Admin"))
            {
                menuLinks.Add(new MenuLink("Manage", "Settings", Resource.User_Settings, "settings", "fa fa-cog"));
            }*/

            SetSubMenu(menuLinks);

            base.OnActionExecuting(filterContext);
        }

        #endregion Protected Methods

        #region INVOICE_ACTIONS

        #region LISTS

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
                ApplicationUser user = db.Users.Find(userId);
                ViewBag.UserName = user.FirstName + " " + user.LastName;

                int filterByInt;
                int.TryParse(filterBy, out filterByInt);

                decimal filterByDecimal;
                decimal.TryParse(filterBy, out filterByDecimal);
                IQueryable<InvoiceHeader> invoices = db.InvoiceHeaders.Where(ih => ih.UserId == userId);

                if (!string.IsNullOrWhiteSpace(filterBy))
                {
                    invoices = invoices.Where(x => x.Number == filterByInt ||
                                                   InvoiceUtility.GetInvoiceAmountWithoutVat(x.Id) == filterByDecimal ||
                                                   x.InvoiceLines.Any(il => il.LineDescription.Contains(filterBy)));
                }

                var ivms = new List<InvoiceListItemViewModel>();
                foreach (InvoiceHeader i in invoices)
                {
                    var n = new InvoiceListItemViewModel();

                    n.Id = i.Id;
                    n.Number = i.Number;
                    n.Series = i.Series;
                    n.AmountWithoutVat = InvoiceUtility.GetInvoiceAmountWithoutVat(i.Id);
                    n.AmountWithVat = InvoiceUtility.GetInvoiceTotalAmount(i.Id);
                    n.Vat = InvoiceUtility.GetInvoiceVat(i.Id);
                    n.Date = i.Date;
                    n.DueDate = i.DueDate;
                    n.PaymentState = InvoiceUtility.InvoiceState(i.Id);
                    n.State = i.State;
                    n.ReceiptNo = i.ReceiptNo;
                    ivms.Add(n);
                }

                List<InvoiceListItemViewModel> sortedList = null;

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

                return View(MVC.Manage.Views.InvoiceList, sortedList.ToPagedList(currentPageIndex, 10));
            }
        }

      
        #endregion LISTS

        #endregion INVOICE_ACTIONS
        
        #region Helpers

        // Used for XSRF protection when adding external logins

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error,
            SaveUserDetailsSuccess
        }

        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get { return HttpContext.GetOwinContext().Authentication; }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (string error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasPhoneNumber()
        {
            ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        private async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie,
                DefaultAuthenticationTypes.TwoFactorCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties {IsPersistent = isPersistent},
                await user.GenerateUserIdentityAsync(UserManager));
        }

        #endregion Helpers
    }
}