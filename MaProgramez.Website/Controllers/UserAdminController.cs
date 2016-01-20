using System.Data;
using System.Globalization;
using GoogleMaps.LocationServices;
using MaProgramez.Repository.BusinessLogic;
using MaProgramez.Repository.DbContexts;
using MaProgramez.Repository.Entities;
using Org.BouncyCastle.Asn1.Cms;

namespace MaProgramez.Website.Controllers
{
    using Extensions;
    using Helpers;
    using Microsoft.Ajax.Utilities;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Microsoft.AspNet.Identity.Owin;
    using Models;
    using MvcPaging;
    using Resources;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;
    using Utility;
    using ViewModels;
    using WebGrease.Css.Extensions;


    public partial class UsersAdminController : BaseController
    {
        #region PRIVATE FIELDS

        private ApplicationRoleManager _roleManager;
        private ApplicationUserManager _userManager;

        #endregion PRIVATE FIELDS

        #region PROPERTIES

        public ApplicationUserManager UserManager
        {
            get { return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
            private set { _userManager = value; }
        }

        public ApplicationRoleManager RoleManager
        {
            get { return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>(); }
            private set { _roleManager = value; }
        }

        #endregion PROPERTIES

        #region CONTRUCTORS

        /// <summary>
        ///     Initializes a new instance of the <see cref="UsersAdminController" /> class.
        /// </summary>
        public UsersAdminController()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="UsersAdminController" /> class.
        /// </summary>
        /// <param name="userManager">The user manager.</param>
        /// <param name="roleManager">The role manager.</param>
        public UsersAdminController(ApplicationUserManager userManager, ApplicationRoleManager roleManager)
        {
            UserManager = userManager;
            RoleManager = roleManager;
        }

        #endregion CONTRUCTORS

        #region ACTIONS

        #region USERS
        //
        // GET: /Users/

        [CustomAuthorize(Roles = "Admin, Agent")]
        public virtual async Task<ActionResult> Index(int? page, string role, string sortBy, string filterBy,
           int? categoryId, bool ascending = true)
        {
            using (var db = new AppDbContext())
            {

                int currentPageIndex = page.HasValue ? page.Value - 1 : 0;
                IOrderedQueryable<ApplicationUser> users =
                    UserManager.Users.Include("Agent").OrderBy(x => x.Email);

                IQueryable<ApplicationUser> filteredUsers = null;
                if (User.IsInRole("Admin"))
                {
                    filteredUsers = users;
                }
                else
                {
                    IdentityRole adminRole = RoleManager.Roles.FirstOrDefault(x => x.Name == "Admin");

                    if (adminRole != null)
                    {
                        filteredUsers = users.Where(u => u.Roles.All(r => r.RoleId != adminRole.Id) && !u.IsAnonymous);
                    }
                }

                ViewBag.RoleName = role;

                List<IdentityRole> roleList = await RoleManager.Roles.ToListAsync();

                if (!string.IsNullOrWhiteSpace(role))
                {
                    IdentityRole selectedRole = roleList.FirstOrDefault(rl => rl.Name == role);
                    string roleId = string.Empty;
                    if (selectedRole != null)
                    {
                        roleId = selectedRole.Id;
                    }
                    filteredUsers =
                        filteredUsers.Where(u => u.Roles.Any(r => r.RoleId == roleId));
                }

                var categories = db.Slots.Include("Category.ParentCategory")
                                        .Select(s => s.Category)
                                        .Distinct()
                                        .OrderBy(c => c.ParentCategory.Name)
                                        .ThenBy(c => c.Name)
                                        .ToList();

                ViewBag.Categories = new SelectList(categories, "Id", "Name");
                ViewBag.CategoryId = categoryId;

                if (categoryId.HasValue)
                {
                    var providerIdsForCategory =
                        db.Slots
                            .Where(s => s.CategoryId == categoryId.Value)
                            .Select(s => s.ProviderId)
                            .Distinct()
                            .ToList();

                    filteredUsers = filteredUsers.Where(u => providerIdsForCategory.Contains(u.Id));
                }

                if (!string.IsNullOrWhiteSpace(filterBy))
                {
                    filteredUsers =
                        filteredUsers.Where(x => x.FirstName.Contains(filterBy) || x.LastName.Contains(filterBy)
                                                 || x.Agent.FirstName.Contains(filterBy) ||
                                                 x.Agent.LastName.Contains(filterBy)
                                                 || x.CompanyName.Contains(filterBy)
                                                 || x.Alias.Contains(filterBy)
                                                 || x.Email.Contains(filterBy)
                                                 || x.Id == filterBy
                            );
                }

                switch (sortBy)
                {
                    case "email":
                        filteredUsers = ascending
                            ? filteredUsers.OrderBy(x => x.Email)
                            : filteredUsers.OrderByDescending(x => x.Email);
                        break;

                    case "name":
                        filteredUsers = ascending
                            ? filteredUsers.OrderBy(x => x.FirstName)
                            : filteredUsers.OrderByDescending(x => x.FirstName);
                        break;

                    case "company":
                        filteredUsers = ascending
                            ? filteredUsers.OrderBy(x => x.CompanyName)
                            : filteredUsers.OrderByDescending(x => x.CompanyName);
                        break;

                    case "active":
                        filteredUsers = ascending
                            ? filteredUsers.OrderBy(x => x.EmailConfirmed)
                            : filteredUsers.OrderByDescending(x => x.EmailConfirmed);
                        break;

                    case "agent":
                        filteredUsers = ascending
                            ? filteredUsers.OrderBy(x => x.Agent.FirstName)
                            : filteredUsers.OrderByDescending(x => x.Agent.FirstName);
                        break;

                    case "date":
                        filteredUsers = ascending
                            ? filteredUsers.OrderBy(x => x.CreatedDate)
                            : filteredUsers.OrderByDescending(x => x.CreatedDate);
                        break;

                    default:
                        filteredUsers = filteredUsers.OrderByDescending(x => x.CreatedDate);
                        break;
                }

                ViewBag.RoleNames = User.IsInRole("Admin")
                    ? new SelectList(roleList, "Name", "Name")
                    : new SelectList(roleList.Where(r => r.Name.In("Provider", "Client", "Employee")), "Name", "Name");

                ViewBag.Page = currentPageIndex;
                ViewBag.SortBy = sortBy;
                ViewBag.FilterBy = filterBy;
                ViewBag.Ascending = ascending;

                return View(filteredUsers.ToPagedList(currentPageIndex, 10));
            }
        }

        [CustomAuthorize(Roles = "Agent")]
        public virtual ActionResult Users(string status, int? page)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return RedirectToAction(MVC.Home.Index(null, string.Empty));
            }

            string userId = User.Identity.GetUserId();
            IEnumerable<ApplicationUser> users = null;

            using (var db = new AppDbContext())
            {
                switch (status.ToLower())
                {
                    case "supplier":
                        {
                            var supplierIds = new List<string>();
                            IdentityRole suppliersRole = RoleManager.Roles.FirstOrDefault(x => x.Name == "Provider");
                            if (suppliersRole != null)
                            {
                                supplierIds = suppliersRole.Users.Select(x => x.UserId).ToList();
                            }

                            users = db.Users.Where(x => x.AgentId == userId
                                                        && supplierIds.Any(supplier => supplier == x.Id))
                                .OrderByDescending(x => x.CreatedDate);
                        }
                        break;

                    default:
                        return RedirectToAction(MVC.Home.Index(null, string.Empty));
                }

                int currentPageIndex = page.HasValue && page.Value > 0 ? page.Value - 1 : 0;

                var model = new GenericViewModel
                {
                    Page = page.HasValue && page.Value > 0 ? page.Value : 0,
                    PageSize = PageSize,
                    Total = users.Count(),
                    Status = status.ToLower(),
                    Users = users.Skip(PageSize * currentPageIndex).Take(PageSize).ToPagedList(0, PageSize)
                };

                return View(model);
            }
        }

        [CustomAuthorize(Roles = "Provider, Employee")]
        public virtual ActionResult ClientsList(int? slotId, int? page, string filterBy)
        {
            IEnumerable<ApplicationUser> users = RetrieveLists.GetClientsByProvider(User.Identity.GetUserId(), slotId);

            if (!string.IsNullOrWhiteSpace(filterBy))
            {
                users = users.Where(x => (x.FirstName != null && x.FirstName.Contains(filterBy))
                    || (x.LastName != null && x.LastName.Contains(filterBy))
                                         || (x.Email != null && x.Email.Contains(filterBy))
                                         || (x.PhoneNumber != null && x.PhoneNumber.Contains(filterBy))
                    ).ToList();
            }

            var currentPageIndex = page.HasValue && page.Value > 0 ? page.Value - 1 : 0;

            ViewBag.Page = currentPageIndex;
            ViewBag.SlotId = slotId;
            ViewBag.FilterBy = filterBy;

            return View(users.ToPagedList(currentPageIndex, 10));
        }

        [CustomAuthorize(Roles = "Client, Admin")]
        [HttpGet]
        public virtual ActionResult FavouritesProviders(int? page, string filterBy)
        {
            IEnumerable<Favorite> users = RetrieveLists.GetFavorites(User.Identity.GetUserId());

            if (!string.IsNullOrWhiteSpace(filterBy))
            {
                users =
                    users.Where(x => (x.FavoriteUser.FirstName != null && x.FavoriteUser.FirstName.Contains(filterBy))
                          || (x.FavoriteUser.CompanyName != null && x.FavoriteUser.CompanyName.Contains(filterBy))
                          || (x.FavoriteUser.Alias != null && x.FavoriteUser.Alias.Contains(filterBy))
                          || (x.FavoriteUser.LastName != null && x.FavoriteUser.LastName.Contains(filterBy))
                          || (x.FavoriteUser.Email != null && x.FavoriteUser.Email.Contains(filterBy))
                          || (x.FavoriteUser.PhoneNumber != null && x.FavoriteUser.PhoneNumber.Contains(filterBy))
                          || (x.FavoriteSlot != null && x.FavoriteSlot.Name != null && x.FavoriteSlot.Name.Contains(filterBy))
                          || (x.FavoriteSlot != null && x.FavoriteSlot.Phone != null && x.FavoriteSlot.Phone.Contains(filterBy))
                ).ToList();
            }

            var currentPageIndex = page.HasValue && page.Value > 0 ? page.Value - 1 : 0;

            ViewBag.Page = currentPageIndex;
            ViewBag.FilterBy = filterBy;

            return View(users.ToPagedList(currentPageIndex, 10));
        }

        //
        // GET: /Users/Details/5

        [CustomAuthorize(Roles = "Admin, Agent")]
        public virtual async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser user = await UserManager.FindByIdAsync(id);

            ViewBag.RoleNames = await UserManager.GetRolesAsync(user.Id);

            return View(user);
        }

        [HttpGet]
        [CustomAuthorize(Roles = "Admin, Agent")]
        public virtual ActionResult UnblockAccount(string id)
        {
            using (var db = new AppDbContext())
            {
                ApplicationUser user = db.Users.Find(id);

                user.LockoutEndDateUtc = null;
                user.AccessFailedCount = 0;

                db.Users.Attach(user);
                db.Entry(user).State = EntityState.Modified;

                if (db.SaveChanges() < 0)
                {
                    return RedirectToAction(MVC.Error.Index());
                }
            }

            return RedirectToAction(MVC.UsersAdmin.Index());
        }

        [HttpGet]
        [CustomAuthorize(Roles = "Admin, Agent")]
        public virtual ActionResult ActivateAccount(string id)
        {
            using (var db = new AppDbContext())
            {
                ApplicationUser user = db.Users.Find(id);

                user.EmailConfirmed = true;

                db.Users.Attach(user);
                db.Entry(user).State = EntityState.Modified;

                if (db.SaveChanges() < 0)
                {
                    return RedirectToAction(MVC.Error.Index());
                }
            }

            return RedirectToAction(MVC.UsersAdmin.Index());
        }

        [HttpGet]
        public virtual ActionResult SendNotification(string id)
        {
            var model = new NotificationViewModel(id);

            return View(model);
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin, Agent, Provider, Employee")]
        public virtual ActionResult SendNotification(NotificationViewModel model)
        {
            if (model.Text.IsNullOrWhiteSpace())
            {
                ModelState.AddModelError("", Resource.Error_Description);

                return View(new NotificationViewModel(model.User));
            }
            using (var db = new AppDbContext())
            {
                if (User.IsInRole("Admin") || User.IsInRole("Agent"))
                {
                    if (model.User.Equals("broadcast"))
                    {
                        IDbSet<ApplicationUser> users = db.Users;
                        NotificationType type = model.Result == 1
                            ? NotificationType.Advertisement
                            : NotificationType.SystemAlert;

                        foreach (ApplicationUser user in users)
                        {
                            var notification = new Notification(user.Id, type, model.Text);
                            db.Notifications.Add(notification);

                            NotificationCenter.SendNotificationToUser(user.Id,
                                RenderRazorViewToString("~/Views/Shared/PartialViews/_Notification.cshtml",
                                    notification));

                            //send sms
                            if (user.PhoneNumber != null && user.AcceptsNotificationOnSms &&
                                user.PhoneNumberConfirmed)
                            {
                                MailAndSmsUtility.SendSms(user.PhoneNumber,
                                    "Plansy: " + type.DisplayDescription() + "\n" + model.Text);
                            }
                            //send email
                            if (user.Email != null && user.AcceptsNotificationOnEmail &&
                                user.EmailConfirmed)
                            {
                                string body = MvcUtility.RenderRazorViewToString(this, MVC.Mail.Views.GenericMail,
                                    new MailViewModel
                                    {
                                        Title = type.DisplayDescription(),
                                        Content = model.Text,
                                        Footer = Resource.NoReply
                                    });
                                MailAndSmsUtility.SendEmail(user.Email, "Plansy: " + type.DisplayDescription(),
                                    body);
                            }
                        }

                        if (db.SaveChanges() < 0)
                        {
                            return RedirectToAction(MVC.Error.Index());
                        }
                        return RedirectToAction(MVC.Home.Index());
                    }
                    else
                    {
                        NotificationType type = model.Result == 1
                            ? NotificationType.Advertisement
                            : NotificationType.SystemAlert;

                        var notification = new Notification(model.User, type, model.Text);
                        db.Notifications.Add(notification);

                        if (db.SaveChanges() < 0)
                        {
                            return RedirectToAction(MVC.Error.Index());
                        }

                        NotificationCenter.SendNotificationToUser(model.User,
                            RenderRazorViewToString("~/Views/Shared/PartialViews/_Notification.cshtml",
                                notification));

                        ApplicationUser beneficiary = db.Users.Find(model.User);
                        //send sms
                        if (beneficiary.PhoneNumber != null && beneficiary.AcceptsNotificationOnSms &&
                            beneficiary.PhoneNumberConfirmed)
                        {
                            MailAndSmsUtility.SendSms(beneficiary.PhoneNumber,
                                "Plansy: " + type.DisplayDescription() + "\n" + model.Text);
                        }
                        //send email
                        if (beneficiary.Email != null && beneficiary.AcceptsNotificationOnEmail &&
                            beneficiary.EmailConfirmed)
                        {
                            string body = MvcUtility.RenderRazorViewToString(this, MVC.Mail.Views.GenericMail,
                                new MailViewModel
                                {
                                    Title = type.DisplayDescription(),
                                    Content = model.Text,
                                    Footer = Resource.NoReply
                                });
                            MailAndSmsUtility.SendEmail(beneficiary.Email,
                                "Plansy: " + type.DisplayDescription(),
                                body);
                        }

                        return RedirectToAction(MVC.UsersAdmin.Index());
                    }
                }
                else // PROVIDERS & SLOTS
                {
                    if (model.User.Equals("broadcast"))
                    {
                        var users = RetrieveLists.GetClientsByProvider(User.Identity.GetUserId(), null);
                        foreach (var applicationUser in users)
                        {
                            var n = RetrieveOthers.AddNotification(0, applicationUser.Id, NotificationType.Advertisement, model.Text);

                            NotificationCenter.SendNotificationToUser(applicationUser.Id,
                                RenderRazorViewToString("~/Views/Shared/PartialViews/_Notification.cshtml",
                                    n));
                            if (applicationUser.AcceptsNotificationOnSms)
                            {
                                MailAndSmsUtility.SendSms(applicationUser.PhoneNumber,
                                    "Plansy - " + model.Text);
                            }
                        }
                    }
                    else
                    {
                        var n = RetrieveOthers.AddNotification(0, model.User, NotificationType.Advertisement, model.Text);

                        NotificationCenter.SendNotificationToUser(model.User,
                            RenderRazorViewToString("~/Views/Shared/PartialViews/_Notification.cshtml",
                                n));

                        var user = RetrieveOthers.GetUserById(model.User);
                        if (user.AcceptsNotificationOnSms)
                        {
                            MailAndSmsUtility.SendSms(user.PhoneNumber,
                                "Plansy - " + model.Text);
                        }
                    }

                    return RedirectToAction(MVC.UsersAdmin.ClientsList(null, null, null));
                }
            }
        }

        public virtual ActionResult ResendActivationEmail(string userId)
        {
            using (var db = new AppDbContext())
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    IQueryable<ApplicationUser> inactiveUsers = db.Users.Where(u => !u.EmailConfirmed);

                    foreach (ApplicationUser user in inactiveUsers)
                    {
                        string activationCode = UserManager.GenerateEmailConfirmationToken(user.Id);
                        string callbackUrl = Url.Action("ActivateAccount", "Account", new { code = activationCode }, Request.Url.Scheme);
                        string mailContent = string.Concat(Resource.ActivationEmail_Content,
                            " <a href=\"", callbackUrl, "\">", Resource.Activate, "</a>");

                        string body = MvcUtility.RenderRazorViewToString(this, MVC.Mail.Views.GenericMail,
                            new MailViewModel
                            {
                                Title = Resource.ActivateAccount_Title,
                                Content = mailContent,
                                Footer = Resource.NoReply
                            });

                        //UserManager.SendEmail(user.Id, Resource.EmailConfirmation_Subject, body);
                        MailAndSmsUtility.SendEmail(user.Email, Resource.EmailConfirmation_Subject, body);
                    }

                    return RedirectToConfirmation(new
                        ConfirmationViewModel
                    {
                        Title = "Retrimitere emailuri",
                        Type = ConfirmationType.Success,
                        Message = "Email-urile au fost trimise cu success",
                        Link = Url.Action(MVC.UsersAdmin.Index(null, string.Empty, string.Empty, string.Empty, null))
                    });
                }
                else
                {
                    ApplicationUser user = db.Users.FirstOrDefault(x => x.Id == userId);
                    if (user != null && user.EmailConfirmed == false)
                    {
                        string activationCode = UserManager.GenerateEmailConfirmationToken(userId);
                        string callbackUrl = Url.Action("ActivateAccount", "Account", new { userId = user.Id, code = activationCode }, Request.Url.Scheme);
                        string mailContent = string.Concat(Resource.ActivationEmail_Content,
                            " <a href=\"", callbackUrl, "\">", Resource.Activate, "</a>");

                        string body = MvcUtility.RenderRazorViewToString(this, MVC.Mail.Views.GenericMail,
                            new MailViewModel
                            {
                                Title = Resource.ActivateAccount_Title,
                                Content = mailContent,
                                Footer = Resource.NoReply
                            });

                        //UserManager.SendEmail(user.Id, Resource.EmailConfirmation_Subject, body);
                        MailAndSmsUtility.SendEmail(user.Email, Resource.EmailConfirmation_Subject, body);

                        return RedirectToConfirmation(new
                            ConfirmationViewModel
                        {
                            Title = "Retrimitere email de confirmare",
                            Type = ConfirmationType.Success,
                            Message = "Emailul a fost trimis cu success",
                            Link = Url.Action(MVC.UsersAdmin.Index(null, string.Empty, string.Empty, string.Empty, null))
                        });
                    }
                }
            }

            return RedirectToConfirmation(new
                ConfirmationViewModel
            {
                Title = "Email netrimis",
                Type = ConfirmationType.Warning,
                Message = "Nu a fost trimis niciun email",
                Link = Url.Action(MVC.UsersAdmin.Index(null, string.Empty, string.Empty, string.Empty, null))
            });
        }

        //
        // GET: /Users/Create
        public virtual ActionResult Create(string role)
        {
            IdentityRole selectedRole = null;
            if (!string.IsNullOrWhiteSpace(role) && RoleManager.RoleExists(role))
            {
                selectedRole = RoleManager.Roles.FirstOrDefault(x => x.Name == role);
            }

            //Get the list of Roles
            SelectList roles = User.IsInRole("Agent")
                ? new SelectList(
                   RoleManager.Roles.Where(r => r.Name == "Client" || r.Name == "Provider").ToList(),
                    "Name", "Name", selectedRole != null ? selectedRole.Name : string.Empty)
                : new SelectList(RoleManager.Roles.ToList(), "Name", "Name",
                    selectedRole != null ? selectedRole.Name : string.Empty);

            ViewBag.RoleId = roles;

            var createUserViewModel = new CreateUserViewModel()
            {
                ContractDate = DateTime.Now,
                AcceptsNotificationOnEmail = true,
                AcceptsNotificationOnSms = true,
                IsCompany = false,
                SubscriptionPeriod = 6,
                Categories = RetrieveLists.GetAllSubCategoriesDictionary()
            };

            SessionUtility.ClearAddresses();
            createUserViewModel.Addresses = SessionUtility.GetAddresses();

            using (var db = new AppDbContext())
            {
                ViewBag.Counties = db.Counties.OrderBy(x => x.Name).ToList();
                //ViewBag.Banks = db.Banks.OrderBy(x => x.Name).ToList();
            }

            return View(createUserViewModel);
        }

        //
        // POST: /Users/Create
        [HttpPost]
        public virtual ActionResult Create(CreateUserViewModel userViewModel, HttpPostedFileBase file,
            params string[] selectedRoles)
        {
            //Get the list of Roles
            List<IdentityRole> roles = RoleManager.Roles.ToList();

            IEnumerable<SelectListItem> availableRoles = User.IsInRole("Admin")
               ? (roles.Select(x => new SelectListItem
               {
                   Text = x.Name,
                   Value = x.Name,
                   Selected = selectedRoles != null && selectedRoles.Contains(x.Name)
               }))
               : (roles.Where(r => r.Name == "Provider" || r.Name == "Client")
                   .Select(x => new SelectListItem
                   {
                       Text = x.Name,
                       Value = x.Name,
                       Selected = selectedRoles != null && selectedRoles.Contains(x.Name)
                   }));

            userViewModel.Addresses = SessionUtility.GetAddresses();
            using (var db = new AppDbContext())
            {
                ViewBag.Counties = db.Counties.OrderBy(x => x.Name).ToList();
                //ViewBag.Banks = db.Banks.OrderBy(x => x.Name).ToList();

                //var bank = db.Banks.Where(m => m.Code == userViewModel.Bank);
                List<Address> addresses = SessionUtility.GetAddresses();

                if (ModelState.IsValid && addresses != null && addresses.Any())
                {
                    var user = new ApplicationUser
                    {
                        UserName = userViewModel.Email,
                        Email = userViewModel.Email,
                        LastName = userViewModel.LastName,
                        FirstName = userViewModel.FirstName,
                        AcceptsNotificationOnEmail = userViewModel.AcceptsNotificationOnEmail,
                        AcceptsNotificationOnSms = userViewModel.AcceptsNotificationOnSms,
                        AccountNumber = userViewModel.AccountNumber,
                        //Bank = userViewModel.Bank,
                        Cui = userViewModel.Cui,
                        Jno = userViewModel.Jno,
                        IsCompany = userViewModel.IsCompany,
                        CompanyName = userViewModel.CompanyName,
                        LogoPath = userViewModel.LogoPath,
                        IdCardNo = userViewModel.IdCardNo,
                        Addresses = new List<Address>(),
                        TwoFactorEnabled = false,
                        CreatedDate = DateTime.Now,
                        AcceptedTermsFlag = true,
                        ContractDate = userViewModel.ContractDate,
                        ContractNo = userViewModel.ContractNo,
                        ProgrammingPerSlot = userViewModel.ProgrammingPerSlot,
                        FullDescription = userViewModel.FullDescription,
                        Alias = string.IsNullOrWhiteSpace(userViewModel.Alias) ? userViewModel.CompanyName : userViewModel.Alias,
                        ComplaintsNumber = 0,
                        EndOfSubscription = DateTime.Now.AddMonths(userViewModel.SubscriptionPeriod)
                    };

                    if (User.IsInRole("Agent"))
                    {
                        user.AgentId = User.Identity.GetUserId();
                    }

                    if (file != null && ImageUtility.CheckFileSize(file))
                    {
                        string fileName = string.Format("{0}-{1}", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff"),
                            Regex.Replace(Path.GetFileName(file.FileName), "[^a-zA-Z0-9%._]", string.Empty));
                        ImageUtility.SaveImage(file, fileName, "Logos", "medium", false);
                        user.LogoPath = fileName;
                    }
                    string password = PasswordGenerator.Generate(6);
                    IdentityResult adminresult = UserManager.Create(user, password);

                    //Add User to the selected Roles
                    if (adminresult.Succeeded)
                    {
                        /// SEND ACTIVATION LINK

                        string activationCode = user.Id; //await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                        string callbackUrl = Url.Action("ActivateAccount", "Account", new { userId = user.Id, code = activationCode },
                            Request.Url.Scheme);
                        string mailContent = string.Concat(Resource.ActivationEmail_Content,
                            " <a href=\"", callbackUrl, "\">", Resource.Activate, "</a>");

                        string body = MvcUtility.RenderRazorViewToString(this, MVC.Mail.Views.GenericMail,
                            new MailViewModel
                            {
                                Title = Resource.ActivateAccount_Title,
                                Content = mailContent,
                                Footer = Resource.NoReply
                            });

                        MailAndSmsUtility.SendEmail(user.Email, Resource.ActivateAccount_Title, body);

                        if (selectedRoles != null)
                        {
                            IdentityResult result = UserManager.AddToRoles(user.Id, selectedRoles);
                            if (!result.Succeeded)
                            {
                                ListExtensions.ForEach(result.Errors, e => ModelState.AddModelError("", e));
                                ViewBag.RoleId = availableRoles;
                                return View(userViewModel);
                            }

                        }

                        if (!string.IsNullOrWhiteSpace(userViewModel.PhoneNumber))
                        {
                            string code =

                                    UserManager.GenerateChangePhoneNumberToken(user.Id, userViewModel.PhoneNumber);
                            IdentityResult result =
                                UserManager.ChangePhoneNumber(user.Id, userViewModel.PhoneNumber, code);
                            if (!result.Succeeded)
                            {
                                ListExtensions.ForEach(result.Errors, e => ModelState.AddModelError("", e));
                                ViewBag.RoleId = availableRoles;
                                return View(userViewModel);
                            }
                        }

                        foreach (Address address in addresses)
                        {
                            address.UserId = user.Id;

                            //Get Latitude and Longitude
                            var locationService = new GoogleLocationService();

                            if (address.UserCity == null)
                            {
                                address.UserCity = db.Cities.Include("CityCounty").FirstOrDefault(c => c.Id == address.CityId);
                            }

                            if (address.UserCountry == null)
                            {
                                address.UserCountry = db.Countries.FirstOrDefault(c => c.Id == address.CountryId);
                            }

                            var point = locationService.GetLatLongFromAddress(address.ToString());
                            if (point != null)
                            {
                                address.Latitude = point.Latitude;
                                address.Longitude = point.Longitude;
                            }

                            address.Id = 0;
                            address.UserCity = null;
                            address.UserCountry = null;

                            db.Addresses.Add(address);
                        }

                        if (db.SaveChanges() < 0)
                        {
                            return RedirectToAction(MVC.Error.Index());
                        }

                        Address a = addresses.Any(m => m.AddressType == AddressType.PlaceOfBusinessAddress)
                            ? addresses.First(m => m.AddressType == AddressType.PlaceOfBusinessAddress)
                            : addresses.First(m => m.AddressType == AddressType.InvoiceAddress);
                        Address aDb = db.Addresses.Include("UserCity").Include("UserCountry").First(m => m.Id == a.Id);
                    }
                    else
                    {
                        ListExtensions.ForEach(adminresult.Errors, e => ModelState.AddModelError("", e));
                        ViewBag.RoleId = availableRoles;
                        return View(userViewModel);
                    }

                    if (selectedRoles != null && selectedRoles.Any(r => r == "Provider") && userViewModel.InvoiceSum > 0)
                    {
                        /*INVOICE*/
                        var invoice = InvoiceUtility.GenerateInvoice(user.Id, userViewModel.SubscriptionPeriod, userViewModel.InvoiceSum,
                            userViewModel.WithReceipt);

                        var link = Url.Action("InvoicePdf", "Invoice", new { invoiceHeaderId = invoice },
                            Request.Url.Scheme);
                        MailAndSmsUtility.SendEmail(user.Email, "Plansy - Factura fiscala",
                            "Factura fiscala aferenta abonamentului Plansy in valoare de " + userViewModel.InvoiceSum +
                            " RON, achizitionat pentru perioada de " + userViewModel.SubscriptionPeriod +
                            " luni, poate fi gasita <a href=\"" + link + "\">aici.</a>");
                        if (userViewModel.WithReceipt)
                        {
                            InvoiceUtility.AddInvoicePayment(invoice, DateTime.Now, PaymentMethod.Numerar);

                            link = Url.Action("ReceiptPdf", "Invoice", new { invoiceHeaderId = invoice },
                                Request.Url.Scheme);
                            MailAndSmsUtility.SendEmail(user.Email, "Plansy - Chitanta",
                                "Chitanta pentru plata facturii trimise anterior poate fi gasita <a href=\"" + link +
                                "\">aici.</a>");
                        }
                    }

                    return RedirectToAction("Index");
                }
                else if (addresses == null || !addresses.Any())
                {
                    ModelState.AddModelError("", Resource.Error_MinimumOneAddress);
                }

                userViewModel.Categories = Repository.BusinessLogic.RetrieveLists.GetAllSubCategoriesDictionary();
                ViewBag.RoleId = availableRoles;
                return View(userViewModel);
            }
        }

        //
        // GET: /Users/Edit/1
        public virtual async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user =
                UserManager.Users.Include("Addresses")
                    .Include("Addresses.UserCity")
                    .Include("Addresses.UserCity.CityCounty")
                    .First(u => u.Id == id);
            if (user == null)
            {
                return HttpNotFound();
            }
            //Get the list of Roles
            List<IdentityRole> roles = await RoleManager.Roles.ToListAsync();
            IList<string> userRoles = await UserManager.GetRolesAsync(user.Id);

            IEnumerable<SelectListItem> availableRoles = User.IsInRole("Admin")
                ? (roles.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Name,
                    Selected = userRoles.Contains(x.Name)
                }))
                : (roles.Where(r => r.Name == "Provider" || r.Name == "Client" || r.Name == "Employee")
                    .Select(x => new SelectListItem
                    {
                        Text = x.Name,
                        Value = x.Name,
                        Selected = userRoles.Contains(x.Name)
                    }));

            ViewBag.RoleId = availableRoles;
            using (var db = new AppDbContext())
            {
                ViewBag.Counties = db.Counties.OrderBy(x => x.Name).ToList();
                //ViewBag.Banks = db.Banks.OrderBy(x => x.Name).ToList();
            }

            return View(new EditUserViewModel()
            {
                Id = user.Id,
                Email = user.Email,
                LastName = user.LastName,
                FirstName = user.FirstName,
                AcceptsNotificationOnEmail = user.AcceptsNotificationOnEmail,
                AcceptsNotificationOnSms = user.AcceptsNotificationOnSms,
                AccountNumber = user.AccountNumber,
                //Bank = user.Bank,
                Cui = user.Cui,
                Jno = user.Jno,
                IsCompany = user.IsCompany,
                LogoPath = user.LogoPath,
                PhoneNumber = user.PhoneNumber,
                VatRate = user.VatRate,
                IdCardNo = user.IdCardNo,
                CompanyName = user.CompanyName,
                Addresses = user.Addresses,
                ContractDate = user.ContractDate,
                ContractNo = user.ContractNo,
                Categories = RetrieveLists.GetAllSubCategoriesDictionary(),
                ProgrammingPerSlot = user.ProgrammingPerSlot,
                FullDescription = user.FullDescription,
                Alias = user.Alias,
            });
        }

        //
        // POST: /Users/Edit/5
        [HttpPost]
        public virtual async Task<ActionResult> Edit(EditUserViewModel editUserViewModel, HttpPostedFileBase file,
            params string[] selectedRole)
        {
            if (editUserViewModel.Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser user = await UserManager.FindByIdAsync(editUserViewModel.Id);
            if (user == null)
            {
                return HttpNotFound();
            }

            //Get the list of Roles
            List<IdentityRole> roles = await RoleManager.Roles.ToListAsync();
            IList<string> userRoles = await UserManager.GetRolesAsync(user.Id);


            IEnumerable<SelectListItem> availableRoles = User.IsInRole("Agent")
                ? (roles.Where(r => r.Name == "Client" || r.Name == "Provider" || r.Name == "Employee").Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Name,
                    Selected = selectedRole.Contains(x.Name)
                }))
                : (roles.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Name,
                    Selected = selectedRole.Contains(x.Name)
                }));

            ViewBag.RoleId = availableRoles;
            using (var db = new AppDbContext())
            {
                ViewBag.Counties = db.Counties.OrderBy(x => x.Name).ToList();
                //ViewBag.Banks = db.Banks.OrderBy(x => x.Name).ToList();
                editUserViewModel.Addresses = db.Addresses
                    .Include("UserCity")
                    .Include("UserCity.CityCounty")
                    .Where(x => x.UserId == editUserViewModel.Id).ToList();
            }

            if (ModelState.IsValid)
            {
                if (file != null && ImageUtility.CheckFileSize(file))
                {
                    if (!string.IsNullOrWhiteSpace(user.LogoPath))
                    {
                        ImageUtility.RemoveImage("Logos", user.LogoPath);
                    }

                    string fileName = string.Format("{0}-{1}", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff"),
                        Regex.Replace(Path.GetFileName(file.FileName), "[^a-zA-Z0-9%._]", string.Empty));
                    ImageUtility.SaveImage(file, fileName, "Logos", "medium", false);
                    user.LogoPath = fileName;
                }

                user.Email = editUserViewModel.Email;
                user.LastName = editUserViewModel.LastName;
                user.FirstName = editUserViewModel.FirstName;
                user.AcceptsNotificationOnEmail = editUserViewModel.AcceptsNotificationOnEmail;
                user.AcceptsNotificationOnSms = editUserViewModel.AcceptsNotificationOnSms;
                user.AccountNumber = editUserViewModel.AccountNumber;
                //user.Bank = editUserViewModel.Bank;
                user.Cui = editUserViewModel.Cui;
                user.Jno = editUserViewModel.Jno;
                user.IsCompany = editUserViewModel.IsCompany;
                user.PhoneNumber = editUserViewModel.PhoneNumber;
                user.VatRate = editUserViewModel.VatRate;
                user.IdCardNo = editUserViewModel.IdCardNo;
                user.CompanyName = editUserViewModel.CompanyName;
                user.ContractDate = editUserViewModel.ContractDate;
                user.ContractNo = editUserViewModel.ContractNo;
                user.ProgrammingPerSlot = editUserViewModel.ProgrammingPerSlot;
                user.FullDescription = editUserViewModel.FullDescription;
                user.Alias = editUserViewModel.Alias;

                UserManager.Update(user);

                selectedRole = selectedRole ?? new string[] { };

                IdentityResult result =
                    await UserManager.AddToRolesAsync(user.Id, selectedRole.Except(userRoles).ToArray<string>());

                if (!result.Succeeded)
                {
                    ListExtensions.ForEach(result.Errors, e => ModelState.AddModelError("", e));
                    return View(editUserViewModel);
                }
                result =
                    await UserManager.RemoveFromRolesAsync(user.Id, userRoles.Except(selectedRole).ToArray<string>());

                if (!result.Succeeded)
                {
                    ListExtensions.ForEach(result.Errors, e => ModelState.AddModelError("", e));
                    return View(editUserViewModel);
                }

                //Change password if needed
                if (!string.IsNullOrWhiteSpace(editUserViewModel.Password))
                {
                    string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                    IdentityResult resetPasswordResult =
                        await UserManager.ResetPasswordAsync(user.Id, code, editUserViewModel.Password);
                    if (!resetPasswordResult.Succeeded)
                    {
                        ModelState.AddModelError("", @"Error changing password");
                        return View(editUserViewModel);
                    }
                }

                //Change phone number if needed
                if (!string.IsNullOrWhiteSpace(editUserViewModel.PhoneNumber) &&
                    editUserViewModel.PhoneNumber != user.PhoneNumber)
                {
                    string code =
                        await UserManager.GenerateChangePhoneNumberTokenAsync(user.Id, editUserViewModel.PhoneNumber);
                    IdentityResult changePhoneResult =
                        await UserManager.ChangePhoneNumberAsync(user.Id, editUserViewModel.PhoneNumber, code);
                    if (!changePhoneResult.Succeeded)
                    {
                        ListExtensions.ForEach(changePhoneResult.Errors, e => ModelState.AddModelError("", e));
                        ViewBag.RoleId = availableRoles;
                        return View(editUserViewModel);
                    }
                }

                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", @"Something failed.");
            editUserViewModel.Categories = RetrieveLists.GetAllSubCategoriesDictionary();

            return View(editUserViewModel);
        }

        //
        // GET: /Users/Delete/5
        [HttpGet]
        [CustomAuthorize(Roles = "Admin")]
        public virtual async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        //
        // POST: /Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [CustomAuthorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> DeleteConfirmed(string id)
        {
            if (ModelState.IsValid)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                ApplicationUser user = await UserManager.FindByIdAsync(id);
                if (user == null)
                {
                    return HttpNotFound();
                }

                //Try to delete dependencies
                using (var db = new AppDbContext())
                {
                    var slot = db.Slots.FirstOrDefault(s => s.UserId == user.Id);
                    if (slot != null)
                    {
                        slot.UserId = null;
                        db.Entry(slot).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    var addresses = db.Addresses.Where(a => a.UserId == user.Id);
                    foreach (var address in addresses)
                    {
                        db.Entry(address).State = EntityState.Deleted;
                        db.SaveChanges();
                    }
                }

                var result = await UserManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", result.Errors.First());
                    return View();
                }
                return RedirectToAction(MVC.UsersAdmin.Index());
            }
            return View();
        }

        #endregion USERS

        #region INVOICES_AND_PAYMENTS

        [CustomAuthorize(Roles = "Admin, Agent")]
        public virtual ActionResult AdminUserInvoiceList(int? page, string userId, string sortBy, string filterBy,
            bool ascending = true)
        {
            using (var db = new AppDbContext())
            {
                ApplicationUser user = db.Users.Find(userId);
                ViewBag.Name = user.FirstName + " " + user.LastName;
                ViewBag.UserId = userId;
                int currentPageIndex = page.HasValue ? page.Value - 1 : 0;

                int filterByInt;
                int.TryParse(filterBy, out filterByInt);

                decimal filterByDecimal;
                decimal.TryParse(filterBy, out filterByDecimal);
                IQueryable<InvoiceHeader> invoices = from i in db.InvoiceHeaders.Include("User")
                                                     where i.UserId == userId
                                                     select i;

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
                    n.FirstName = i.User.FirstName;
                    n.LastName = i.User.LastName;
                    n.UserName = i.User.UserName;
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

                return View(sortedList.ToPagedList(currentPageIndex, 10));
            }
        }


        [CustomAuthorize(Roles = "Admin, Agent, Accountant")]
        public virtual ActionResult AdminInvoicePaymentsList(int? page, string userId, string sortBy, string filterBy,
            bool ascending = true)
        {
            using (var db = new AppDbContext())
            {
                ApplicationUser user = db.Users.Find(userId);
                ViewBag.Name = user.FirstName + " " + user.LastName;
                ViewBag.UserId = userId;

                int currentPageIndex = page.HasValue ? page.Value - 1 : 0;

                decimal filterByDecimal;
                decimal.TryParse(filterBy, out filterByDecimal);
                IQueryable<InvoicePayment> payments =
                    from i in db.InvoicePayments.Include("InvoiceHeader").Include("InvoiceHeader.Request")
                    where i.InvoiceHeader.UserId == userId
                    select i;

                if (!string.IsNullOrWhiteSpace(filterBy))
                {
                    payments = payments.Where(x => x.Amount == filterByDecimal || x.Details.Contains(filterBy)
                                                   || x.PaymentMethod.DisplayDescription().Contains(filterBy));
                }

                List<InvoicePayment> ivms = payments.ToList();

                List<InvoicePayment> sortedList = null;

                switch (sortBy)
                {

                    case "amount":
                        sortedList = ascending
                            ? ivms.OrderBy(x => x.Amount).ToList()
                            : ivms.OrderByDescending(x => x.Amount).ToList();
                        break;

                    case "date":
                        sortedList = ascending
                            ? ivms.OrderBy(x => x.Date).ToList()
                            : ivms.OrderByDescending(x => x.Date).ToList();
                        break;

                    case "method":
                        sortedList = ascending
                            ? ivms.OrderBy(x => x.PaymentMethod.DisplayDescription()).ToList()
                            : ivms.OrderByDescending(x => x.PaymentMethod.DisplayDescription()).ToList();
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
        #endregion INVOICES_AND_PAYMENTS

        #region NON_WORKING_DAYS

        [HttpGet]
        [CustomAuthorize(Roles = "Admin, Agent, Provider, Employee")]
        public virtual ActionResult CreateSlotNonWorkingDays(int slotId)
        {
            var slot = RetrieveOthers.GetSlotById(slotId);
            var model = new AddNonWorkingDayViewModel() { StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(1), SlotId = slot.Id, Slot = slot, IsWorkingDay = false };
            ViewBag.DefaultNonWorkingDays = RetrieveLists.GetDefaultNonWorkingDaysForSlot(slotId);
            ViewBag.SlotNonWorkingDays = RetrieveLists.GetSlotNonWorkingDays(slotId);

            return View(model);
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin, Agent, Provider, Employee")]
        public virtual ActionResult CreateSlotNonWorkingDays(AddNonWorkingDayViewModel model)
        {
            if (!model.Description.IsNullOrWhiteSpace())
            {
                using (var db = new AppDbContext())
                {
                    var nwd = new SlotNonWorkingDay()
                    {
                        Description = model.Description,
                        SlotId = model.SlotId,
                        StartDateTime = new DateTime(model.StartDate.Year, model.StartDate.Month, model.StartDate.Day, model.StartHour, model.StartMinute, 0),
                        EndDateTime = new DateTime(model.EndDate.Year, model.EndDate.Month, model.EndDate.Day, model.EndHour, model.EndMinute, 0),
                        IsWorkingDay = model.IsWorkingDay
                    };
                    db.SlotNonWorkingDays.Add(nwd);
                    db.SaveChanges();

                    return RedirectToAction(MVC.UsersAdmin.CreateSlotNonWorkingDays(model.SlotId));
                }
            }

            ViewBag.DefaultNonWorkingDays = RetrieveLists.GetDefaultNonWorkingDaysForSlot(model.SlotId);
            ViewBag.SlotNonWorkingDays = RetrieveLists.GetSlotNonWorkingDays(model.SlotId);
            ModelState.AddModelError("", Resource.Error_MandatoryFields);

            return View(model);
        }

        [HttpGet]
        [CustomAuthorize(Roles = "Admin, Agent, Provider, Employee")]
        public virtual ActionResult DeleteSlotNonWorkingDay(int id)
        {
            using (var db = new AppDbContext())
            {
                var timetable = db.SlotNonWorkingDays.Find(id);
                var slotId = timetable.SlotId;

                db.SlotNonWorkingDays.Remove(timetable);
                db.SaveChanges();

                return RedirectToAction(MVC.UsersAdmin.CreateSlotNonWorkingDays(slotId));
            }
        }

        [HttpGet]
        [CustomAuthorize(Roles = "Admin, Agent, Provider, Employee")]
        public virtual ActionResult MarkDefaultNonWorkingDayAsWorkingDay(int defaultNonWorkingDayId, int slotId)
        {
            using (var db = new AppDbContext())
            {
                var defaultNonWorkingDay = db.DefaultNonWorkingDays.Find(defaultNonWorkingDayId);

                var slotWorkingDay = new SlotNonWorkingDay()
                {
                    StartDateTime = defaultNonWorkingDay.StartDateTime,
                    EndDateTime = defaultNonWorkingDay.StartDateTime.AddDays(1),
                    IsWorkingDay = true,
                    SlotId = slotId
                };

                db.SlotNonWorkingDays.Add(slotWorkingDay);
                db.SaveChanges();

                return RedirectToAction(MVC.UsersAdmin.CreateSlotNonWorkingDays(slotId));
            }
        }

        [HttpGet]
        [CustomAuthorize(Roles = "Admin, Agent")]
        public virtual ActionResult CreateDefaultNonWorkingDays()
        {
            var model = new DefaultNonWorkingDay() { StartDateTime = DateTime.Now };
            ViewBag.DefaultNonWorkingDays = RetrieveLists.GetDefaultNonWorkingDays();

            return View(model);
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin, Agent")]
        public virtual ActionResult CreateDefaultNonWorkingDays(DefaultNonWorkingDay model)
        {
            if (!model.Description.IsNullOrWhiteSpace())
            {
                using (var db = new AppDbContext())
                {
                    model.EndDateTime = model.StartDateTime.AddDays(1);
                    db.DefaultNonWorkingDays.Add(model);
                    db.SaveChanges();

                    return RedirectToAction(MVC.UsersAdmin.CreateDefaultNonWorkingDays());
                }
            }

            ViewBag.DefaultNonWorkingDays = RetrieveLists.GetDefaultNonWorkingDays();
            ModelState.AddModelError("", Resource.Error_MandatoryFields);
            return View(model);
        }

        [HttpGet]
        [CustomAuthorize(Roles = "Admin, Agent")]
        public virtual ActionResult DeleteDefaultNonWorkingDay(int id)
        {
            using (var db = new AppDbContext())
            {
                var timetable = db.DefaultNonWorkingDays.Find(id);

                db.DefaultNonWorkingDays.Remove(timetable);
                db.SaveChanges();

                return RedirectToAction(MVC.UsersAdmin.CreateDefaultNonWorkingDays());
            }
        }

        #endregion NON_WORKING_DAYS

        #region TIMETABLE

        [HttpGet]
        [CustomAuthorize(Roles = "Admin, Agent, Provider, Employee")]
        public virtual ActionResult CreateTimetable(int slotId)
        {
            using (var db = new AppDbContext())
            {
                var model = new TimeTableViewModel
                {
                    Timetable = RetrieveLists.GetTimetablesBySlot(slotId),
                    Slot = db.Slots.Find(slotId),
                    StartTime = "09:00",
                    EndTime = "17:00",
                    WorkingWeekStartDate = DateTime.Today,
                };

                return View(model);
            }
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin, Agent, Provider, Employee")]
        public virtual ActionResult CreateTimetable(TimeTableViewModel model)
        {
            using (var db = new AppDbContext())
            {
                if (model.WorkingDays.Any() && !model.StartTime.IsNullOrWhiteSpace() &&
                    !model.EndTime.IsNullOrWhiteSpace())
                {
                    try
                    {
                        var startTime = DateTime.ParseExact(model.StartTime, "HH:mm", CultureInfo.InvariantCulture);
                        var endTime = DateTime.ParseExact(model.EndTime, "HH:mm", CultureInfo.InvariantCulture);

                        if (startTime > endTime)
                        {
                            var aux = startTime;
                            startTime = endTime;
                            endTime = aux;
                        }

                        foreach (var day in model.WorkingDays)
                        {
                            var slotTimeTable = new SlotTimeTable()
                            {
                                SlotId = model.Slot.Id,
                                StarTime = startTime,
                                EndTime = endTime,
                                DayOfWeek = day,
                                WorkingWeekStartDate = model.IsWorkingByWeekly ? model.WorkingWeekStartDate : (DateTime?)null,
                            };

                            db.SlotTimeTables.Add(slotTimeTable);
                        }
                        db.SaveChanges();

                        return RedirectToAction(MVC.UsersAdmin.CreateTimetable(model.Slot.Id));
                    }
                    catch
                    {
                        model.Timetable = RetrieveLists.GetTimetablesBySlot(model.Slot.Id);
                        ModelState.AddModelError("", Resource.Error_TimeFormat);
                        return View(model);
                    }
                }

                model.Timetable = RetrieveLists.GetTimetablesBySlot(model.Slot.Id);
                ModelState.AddModelError("", Resource.Error_MandatoryFields);
                return View(model);
            }
        }

        [HttpGet]
        [CustomAuthorize(Roles = "Admin, Agent, Provider, Employee")]
        public virtual ActionResult DeleteTimetable(int id)
        {
            using (var db = new AppDbContext())
            {
                var timetable = db.SlotTimeTables.Find(id);
                var slotId = timetable.SlotId;

                db.Entry(timetable).State = EntityState.Deleted;
                db.SaveChanges();

                return RedirectToAction(MVC.UsersAdmin.CreateTimetable(slotId));
            }
        }
        #endregion TIMETABLE

        #region DEFAULT_OPERATIONS

        public virtual ActionResult DefaultOperationsList(int? page, string category)
        {
            var currentPageIndex = page.HasValue ? page.Value - 1 : 0;

            var defaultCategoryOperations = category.IsNullOrWhiteSpace()
                ? RetrieveLists.GetAllDefaultCategoryOperations()
                : RetrieveLists.GetDefaultCategoryOperations(category);

            var categories = RetrieveLists.GetAllSubCategories();
            ViewBag.Categories = new SelectList(categories, "Name", "Name");
            ViewBag.CategoryName = category;
            ViewBag.Page = currentPageIndex;

            return View(defaultCategoryOperations.ToPagedList(currentPageIndex, 10));
        }

        [HttpGet]
        [CustomAuthorize(Roles = "Admin, Agent")]
        public virtual ActionResult CreateDefaultOperation(string categoryName)
        {
            using (var db = new AppDbContext())
            {
                var operation = new SlotOperationViewModel();
                ViewBag.Categories = RetrieveLists.GetAllSubCategoriesDictionary();

                if (!string.IsNullOrEmpty(categoryName))
                {
                    var category = db.Categories.First(s => s.Name == categoryName);
                    operation.CategoryId = category.Id;
                    operation.CategoryName = category.Name;
                };

                return View(operation);
            }
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin, Agent")]
        public virtual ActionResult CreateDefaultOperation(SlotOperationViewModel model)
        {
            using (var db = new AppDbContext())
            {
                if (!string.IsNullOrEmpty(model.Description) && model.DurationMinutes > 0 && model.Price > 0 && model.CategoryId > 0)
                {
                    int operationId;

                    if (db.Operations.Any(o => o.CategoryId == model.CategoryId && o.Description == model.Description))
                        operationId =
                            db.Operations.First(
                                o => o.CategoryId == model.CategoryId && o.Description == model.Description).Id;
                    else
                    {
                        var operation = new Operation()
                        {
                            CategoryId = model.CategoryId,
                            Description = model.Description
                        };

                        db.Operations.Add(operation);
                        db.SaveChanges();

                        operationId = operation.Id;
                    }

                    var defaultOperation = new DefaultCategoryOperation()
                    {
                        OperationId = operationId,
                        DefaultDurationMinutes = model.DurationMinutes,
                        DefaultPrice = model.Price,
                        CategoryId = model.CategoryId
                    };
                    db.DefaultCategoryOperations.Add(defaultOperation);
                    db.SaveChanges();

                    return RedirectToAction(MVC.UsersAdmin.DefaultOperationsList(null, model.CategoryName));
                }

                ViewBag.Categories = RetrieveLists.GetAllSubCategoriesDictionary();
                ModelState.AddModelError("", Resource.Error_MandatoryFields);

                return View(model);
            }
        }

        [HttpGet]
        [CustomAuthorize(Roles = "Admin, Agent")]
        public virtual ActionResult EditDefaultOperation(int slotOperationId)
        {
            using (var db = new AppDbContext())
            {
                var defaultOperation = db.DefaultCategoryOperations.Include("Operation").Include("Operation.Category").First(s => s.Id == slotOperationId);
                ViewBag.Categories = RetrieveLists.GetAllSubCategoriesDictionary();

                return View(new SlotOperationViewModel(defaultOperation));
            }
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin, Agent")]
        public virtual ActionResult EditDefaultOperation(SlotOperationViewModel model)
        {
            using (var db = new AppDbContext())
            {
                if (!string.IsNullOrEmpty(model.Description) && model.DurationMinutes > 0 && model.Price > 0 && model.CategoryId > 0)
                {
                    var defaultOperation = db.DefaultCategoryOperations.Include("Category").Include("Operation").First(s => s.Id == model.Id);

                    defaultOperation.Operation.Description = model.Description;
                    db.Operations.Attach(defaultOperation.Operation);
                    db.Entry(defaultOperation.Operation).State = EntityState.Modified;

                    defaultOperation.DefaultDurationMinutes = model.DurationMinutes;
                    defaultOperation.DefaultPrice = model.Price;
                    db.DefaultCategoryOperations.Attach(defaultOperation);
                    db.Entry(defaultOperation).State = EntityState.Modified;

                    db.SaveChanges();

                    return RedirectToAction(MVC.UsersAdmin.DefaultOperationsList(null, defaultOperation.Category.Name));
                }

                ViewBag.Categories = RetrieveLists.GetAllSubCategoriesDictionary();
                ModelState.AddModelError("", Resource.Error_MandatoryFields);

                return View(model);
            }
        }

        [HttpGet]
        [CustomAuthorize(Roles = "Admin, Agent")]
        public virtual ActionResult DeleteDefaultOperation(int defaultOperationId)
        {
            using (var db = new AppDbContext())
            {
                var defaultOperation = db.DefaultCategoryOperations.Include("Category").First(c => c.Id == defaultOperationId);
                var categoryName = defaultOperation.Category.Name;

                db.DefaultCategoryOperations.Remove(defaultOperation);
                db.SaveChanges();

                return RedirectToAction(MVC.UsersAdmin.DefaultOperationsList(null, categoryName));
            }
        }

        #endregion DEFAULT_OPERATIONS

        #region SLOT_OPERATIONS

        [CustomAuthorize(Roles = "Admin, Agent, Provider, Employee")]
        public virtual ActionResult SlotOperationsList(int slotId)
        {
            ViewBag.Name = RetrieveOthers.GetSlotName(slotId);
            ViewBag.ProviderId = RetrieveOthers.GetProviderIdBySlot(slotId);
            ViewBag.SlotId = slotId;

            return View(RetrieveLists.GetSlotOperationsBySlot(slotId));
        }

        [HttpGet]
        [CustomAuthorize(Roles = "Admin, Agent, Provider, Employee")]
        public virtual ActionResult AddSlotOperation(int slotId)
        {
            using (var db = new AppDbContext())
            {
                var slot = db.Slots.Include("Category").Include("SlotOperations").First(s => s.Id == slotId);
                var existingSlotOperationIds = slot.SlotOperations.Select(so => so.OperationId).ToList();

                var slotOperation = new SlotOperationViewModel()
                {
                    SlotId = slotId,
                    CategoryId = slot.CategoryId,
                    CategoryName = slot.Category.Name,
                    Operations = db.Operations.Where(o => o.CategoryId == slot.CategoryId &&
                                                        !existingSlotOperationIds.Contains(o.Id)).ToList()
                };

                return View(slotOperation);
            }
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin, Agent, Provider, Employee")]
        public virtual ActionResult AddSlotOperation(SlotOperationViewModel model)
        {
            using (var db = new AppDbContext())
            {
                if (model.OperationId > 0 && model.DurationMinutes > 0)
                {
                    var slotOperation = new SlotOperation()
                    {
                        OperationId = model.OperationId,
                        DurationMinutes = model.DurationMinutes,
                        Price = model.Price,
                        SlotId = model.SlotId
                    };
                    db.SlotOperations.Add(slotOperation);
                    db.SaveChanges();

                    return RedirectToAction(MVC.UsersAdmin.SlotOperationsList(model.SlotId));
                }

                ModelState.AddModelError("", Resource.Error_MandatoryFields);

                return View(model);
            }
        }

        [HttpGet]
        [CustomAuthorize(Roles = "Admin, Agent")]
        public virtual ActionResult CreateOperation(int slotId)
        {
            using (var db = new AppDbContext())
            {
                var slot = db.Slots.Include("Category").First(s => s.Id == slotId);
                var operationViewModel = new OperationViewModel()
                {
                    CategoryId = slot.CategoryId,
                    CategoryName = slot.Category.Name,
                    SlotId = slotId
                };

                return View(operationViewModel);
            }
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin, Agent")]
        public virtual ActionResult CreateOperation(OperationViewModel model)
        {
            using (var db = new AppDbContext())
            {
                if (!string.IsNullOrEmpty(model.Description))
                {
                    if (db.Operations.Any(o => o.CategoryId == model.CategoryId && o.Description == model.Description))
                    {
                        ModelState.AddModelError("", @"Mai exista o operatiune cu acelasi nume pe aceasta categorie !");
                        return View(model);
                    }

                    var operation = new Operation()
                         {
                             CategoryId = model.CategoryId,
                             Description = model.Description
                         };

                    db.Operations.Add(operation);
                    db.SaveChanges();

                    return RedirectToAction(MVC.UsersAdmin.AddSlotOperation(model.SlotId));
                }

                ModelState.AddModelError("", Resource.Error_MandatoryFields);

                return View(model);
            }
        }

        [HttpGet]
        [CustomAuthorize(Roles = "Admin, Agent, Provider, Employee")]
        public virtual ActionResult EditSlotOperation(int slotOperationId)
        {
            using (var db = new AppDbContext())
            {
                var slotOperation = db.SlotOperations.Include("Operation").Include("Operation.Category").First(s => s.Id == slotOperationId);

                return View(new SlotOperationViewModel(slotOperation));
            }
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin, Agent, Provider, Employee")]
        public virtual ActionResult EditSlotOperation(SlotOperationViewModel model)
        {
            using (var db = new AppDbContext())
            {
                if (!string.IsNullOrEmpty(model.Description) && model.DurationMinutes > 0)
                {
                    var slotOperation = db.SlotOperations.Include("Operation").First(s => s.Id == model.Id);

                    slotOperation.DurationMinutes = model.DurationMinutes;
                    slotOperation.Price = model.Price;
                    db.SlotOperations.Attach(slotOperation);
                    db.Entry(slotOperation).State = EntityState.Modified;

                    db.SaveChanges();

                    return RedirectToAction(MVC.UsersAdmin.SlotOperationsList(model.SlotId));
                }

                ModelState.AddModelError("", Resource.Error_MandatoryFields);

                return View(model);
            }
        }

        [HttpGet]
        [CustomAuthorize(Roles = "Admin, Agent, Provider, Employee")]
        public virtual ActionResult DeleteSlotOperation(int slotOperationId)
        {
            using (var db = new AppDbContext())
            {
                var slotOperation = db.SlotOperations.Find(slotOperationId);
                var slotId = slotOperation.SlotId;

                try
                {
                    db.SlotOperations.Remove(slotOperation);
                    db.SaveChanges();
                    return RedirectToAction(MVC.UsersAdmin.SlotOperationsList(slotId));
                }
                catch (Exception ex)
                {
                    TempData["ConfirmationViewModel"] = new ConfirmationViewModel()
                    {
                        Link = Url.Action(MVC.UsersAdmin.SlotOperationsList(slotId)),
                        Message = "Nu se poate sterge operatiunea.",
                        Title = "Eroare stergere",
                        Type = ConfirmationType.Error
                    };
                    return RedirectToAction(MVC.Home.Confirmation());
                }
                
            }
        }

        #endregion SLOT_OPERATIONS

        #region SLOTS

        [CustomAuthorize(Roles = "Admin, Agent, Provider")]
        public virtual ActionResult SlotsList(string providerId)
        {
            if (providerId.IsNullOrWhiteSpace())
                providerId = User.Identity.GetUserId();
            ViewBag.Name = RetrieveOthers.GetUserName(providerId);
            ViewBag.ProviderId = providerId;

            return View(RetrieveLists.GetSlotsByProvider(providerId));
        }

        [HttpGet]
        [CustomAuthorize(Roles = "Admin, Agent")]
        public virtual ActionResult CreateSlot(string providerId)
        {
            using (var db = new AppDbContext())
            {
                var provider = db.Users.Find(providerId);
                var slot = new Slot(provider);
                ViewBag.Categories = RetrieveLists.GetAllSubCategoriesDictionary();

                return View(new SlotViewModel { Slot = slot });
            }
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin, Agent")]
        public virtual async Task<ActionResult> CreateSlot(SlotViewModel model)
        {
            using (var db = new AppDbContext())
            {
                if (!string.IsNullOrEmpty(model.Slot.Name) && !string.IsNullOrEmpty(model.Slot.Email) &&
                    !string.IsNullOrEmpty(model.Slot.Phone) && model.Slot.CategoryId > 0)
                {
                    if (model.CreateSlotUser)
                    {
                        var user = new ApplicationUser()
                        {
                            UserName = model.Slot.Email,
                            Email = model.Slot.Email,
                            LastName = model.Slot.Name,
                            PhoneNumber = model.Slot.Phone,
                            AcceptsNotificationOnEmail = model.Slot.AcceptsNotificationOnEmail,
                            AcceptsNotificationOnSms = model.Slot.AcceptsNotificationOnSms,
                            IsCompany = false,
                            TwoFactorEnabled = false,
                            CreatedDate = DateTime.Now,
                            AcceptedTermsFlag = true,
                            ComplaintsNumber = 0
                        };

                        IdentityResult result = await UserManager.CreateAsync(user);
                        if (!result.Succeeded)
                        {
                            ListExtensions.ForEach(result.Errors, e => ModelState.AddModelError("", e));
                            ViewBag.Categories = RetrieveLists.GetAllSubCategoriesDictionary();
                            return View(model);
                        }

                        result = await UserManager.AddToRoleAsync(user.Id, "Employee");
                        if (!result.Succeeded)
                        {
                            ListExtensions.ForEach(result.Errors, e => ModelState.AddModelError("", e));
                            ViewBag.Categories = RetrieveLists.GetAllSubCategoriesDictionary();
                            return View(model);
                        }

                        model.Slot.UserId = user.Id;

                        /* send activation mail */
                        string activationCode = UserManager.GenerateEmailConfirmationToken(user.Id);
                        string callbackUrl = Url.Action("ActivateAccount", "Account", new { userId = user.Id, code = activationCode }, Request.Url.Scheme);
                        string mailContent = string.Concat(Resource.ActivationEmail_Content,
                            " <a href=\"", callbackUrl, "\">", Resource.Activate, "</a>");

                        string body = MvcUtility.RenderRazorViewToString(this, MVC.Mail.Views.GenericMail,
                            new MailViewModel
                            {
                                Title = Resource.ActivateAccount_Title,
                                Content = mailContent,
                                Footer = Resource.NoReply
                            });

                        MailAndSmsUtility.SendEmail(user.Email, Resource.EmailConfirmation_Subject, body);
                    }

                    db.Slots.Add(model.Slot);

                    /** Add default operations to slot **/
                    var defaultOperations = db.DefaultCategoryOperations.Where(d => d.CategoryId == model.Slot.CategoryId).ToList();
                    foreach (var slotOperation in defaultOperations.Select(defaultOperation => new SlotOperation(defaultOperation) { SlotId = model.Slot.Id }))
                    {
                        db.SlotOperations.Add(slotOperation);
                    }

                    db.SaveChanges();

                    return RedirectToAction(MVC.UsersAdmin.SlotsList(model.Slot.ProviderId));
                }

                ViewBag.Categories = RetrieveLists.GetAllSubCategoriesDictionary();
                ModelState.AddModelError("", Resource.Error_MandatoryFields);

                return View(model);
            }
        }

        [HttpGet]
        [CustomAuthorize(Roles = "Admin, Agent")]
        public async virtual Task<ActionResult> CreateSlotUser(int slotId)
        {
            using (var db = new AppDbContext())
            {
                var slot = db.Slots.Include("Provider").Include("Category").FirstOrDefault(s => s.Id == slotId);
                ViewBag.Categories = RetrieveLists.GetAllSubCategoriesDictionary();

                if (string.IsNullOrWhiteSpace(slot.UserId))
                {
                    using (var dbContextTransaction = db.Database.BeginTransaction(IsolationLevel.Serializable))
                    {
                        var email = "Slot_" + slot.Provider.CompanyDisplayName.Replace(" ", "_") + Common.GetDbConfig("AnonymousCount") + "@Plansy.nl";
                        var user = new ApplicationUser()
                        {
                            UserName = email,
                            Email = email,
                            FirstName = slot.Provider.CompanyDisplayName,
                            LastName = slot.Name,
                            PhoneNumber = slot.Phone,
                            AcceptsNotificationOnEmail = false,
                            AcceptsNotificationOnSms = false,
                            IsCompany = false,
                            TwoFactorEnabled = false,
                            CreatedDate = DateTime.Now,
                            AcceptedTermsFlag = true,
                            ComplaintsNumber = 0,
                            IsAnonymous = true,
                        };

                        IdentityResult result = await UserManager.CreateAsync(user);
                        if (!result.Succeeded)
                        {
                            ListExtensions.ForEach(result.Errors, e => ModelState.AddModelError("", e));
                            ViewBag.Categories = RetrieveLists.GetAllSubCategoriesDictionary();
                            return View("EditSlot", slot);
                        }

                        var no = db.Configs.Find("AnonymousCount");
                        no.Value = ((Convert.ToInt32(no.Value) + 1)).ToString(CultureInfo.InvariantCulture);
                        db.Configs.Attach(no);
                        db.Entry(no).State = EntityState.Modified;

                        result = await UserManager.AddToRoleAsync(user.Id, "Employee");
                        if (!result.Succeeded)
                        {
                            ListExtensions.ForEach(result.Errors, e => ModelState.AddModelError("", e));
                            ViewBag.Categories = RetrieveLists.GetAllSubCategoriesDictionary();
                            return View("EditSlot", slot);
                        }

                        slot.UserId = user.Id;

                        db.SaveChanges();
                        dbContextTransaction.Commit();
                    }
                    return RedirectToAction(MVC.UsersAdmin.Edit(slot.UserId));
                }

                return RedirectToAction(MVC.Error.Index());
            }
        }

        [HttpGet]
        [CustomAuthorize(Roles = "Admin, Agent")]
        public virtual ActionResult CopySlot(int slotId)
        {
            using (var db = new AppDbContext())
            {
                var sourceSlot = db.Slots.Include("Provider").Include("SlotOperations").Include("SlotTimeTables").First(s => s.Id == slotId);

                var newSlot = new Slot(sourceSlot.Provider);
                newSlot.Name = sourceSlot.Name + "(COPY)";
                newSlot.Phone = sourceSlot.Phone;
                newSlot.ProviderId = sourceSlot.ProviderId;
                newSlot.CategoryId = sourceSlot.CategoryId;
                newSlot.Email = sourceSlot.Email;

                db.Slots.Add(newSlot);
                db.SaveChanges();

                foreach (var sourceSlotOperation in sourceSlot.SlotOperations)
                {
                    db.SlotOperations.Add(new SlotOperation
                    {
                        SlotId = newSlot.Id,
                        DurationMinutes = sourceSlotOperation.DurationMinutes,
                        OperationId = sourceSlotOperation.OperationId,
                        Price = sourceSlotOperation.Price,
                    });
                }

                foreach (var sourceslotTimeTable in sourceSlot.SlotTimeTables)
                {
                    db.SlotTimeTables.Add(new SlotTimeTable
                    {
                        SlotId = newSlot.Id,
                        DayOfWeek = sourceslotTimeTable.DayOfWeek,
                        StarTime = sourceslotTimeTable.StarTime,
                        EndTime = sourceslotTimeTable.EndTime,
                    });
                }

                db.SaveChanges();

                return RedirectToAction(MVC.UsersAdmin.SlotsList(sourceSlot.ProviderId));
            }
        }

        [HttpGet]
        [CustomAuthorize(Roles = "Admin, Agent, Provider, Employee")]
        public virtual ActionResult EditSlot(int slotId)
        {
            using (var db = new AppDbContext())
            {
                var slot = db.Slots.Include("Category").FirstOrDefault(s => s.Id == slotId);
                ViewBag.Categories = RetrieveLists.GetAllSubCategoriesDictionary();

                return View(slot);
            }
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin, Agent, Provider, Employee")]
        public virtual ActionResult EditSlot(Slot model)
        {
            using (var db = new AppDbContext())
            {
                if (!string.IsNullOrEmpty(model.Name) && !string.IsNullOrEmpty(model.Email) &&
                    !string.IsNullOrEmpty(model.Phone) && model.CategoryId > 0)
                {
                    db.Slots.Attach(model);
                    db.Entry(model).State = EntityState.Modified;
                    db.SaveChanges();

                    return RedirectToAction(MVC.UsersAdmin.SlotsList(model.ProviderId));
                }

                ViewBag.Categories = RetrieveLists.GetAllSubCategoriesDictionary();
                ModelState.AddModelError("", Resource.Error_MandatoryFields);

                return View(model);
            }
        }

        [HttpGet]
        [CustomAuthorize(Roles = "Admin, Agent")]
        public virtual ActionResult DeleteSlot(int slotId)
        {
            using (var db = new AppDbContext())
            {
                var slot = db.Slots.Find(slotId);
                var op = db.SlotOperations.Where(so => so.SlotId == slotId);
                var timeTables = db.SlotTimeTables.Where(tt => tt.SlotId == slotId);
                var nonWorkingDays = db.SlotNonWorkingDays.Where(x => x.SlotId == slotId);
                var providerId = slot.ProviderId;

                if (!slot.UserId.IsNullOrWhiteSpace())
                {
                    var slotUser = db.Users.Find(slot.UserId);
                    db.Users.Remove(slotUser);
                }

                db.SlotOperations.RemoveRange(op);
                db.SlotTimeTables.RemoveRange(timeTables);
                db.SlotNonWorkingDays.RemoveRange(nonWorkingDays);
                db.Slots.Remove(slot);
                db.SaveChanges();

                return RedirectToAction(MVC.UsersAdmin.SlotsList(providerId));
            }
        }

        #endregion SLOTS

        #endregion ACTIONS
    }
}