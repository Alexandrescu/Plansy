using MaProgramez.Repository.BusinessLogic;
using MaProgramez.Repository.Entities;
using MaProgramez.Resources;
using MaProgramez.Website.Extensions;
using MaProgramez.Website.Models;
using MaProgramez.Website.Utility;
using MaProgramez.Website.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using MaProgramez.Repository.DbContexts;

namespace MaProgramez.Website.Controllers
{
    [CustomAuthorize]
    public partial class AccountController : BaseController
    {
        #region PRIVATE FIELDS

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        #endregion PRIVATE FIELDS

        #region PROPERTIES

        public ApplicationUserManager UserManager
        {
            get { return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
            private set { _userManager = value; }
        }

        public ApplicationSignInManager SignInManager
        {
            get { return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>(); }
            private set { _signInManager = value; }
        }

        #endregion PROPERTIES

        #region CONTRUCTORS

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        #endregion CONTRUCTORS

        #region ACTIONS

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public virtual ActionResult Login(string returnUrl)
        {
            ViewBag.Type = "login";
            ViewBag.ReturnUrl = returnUrl;
            return View("LoginLayout");
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> Login(RegisterViewModel model, string returnUrl)
        {
            ViewBag.Type = "login";
            ModelState.Clear();

            if (model.Email == null)
            {
                ModelState.AddModelError("", Resource.Email_ErrorMessage);
                return View("LoginLayout", model);
            }
            else if (model.Password == null)
            {
                ModelState.AddModelError("", Resource.IncorrectPassword);
                return View("LoginLayout", model);
            }

            ApplicationUser user = UserManager.FindByName(model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", Resource.Login_InvalidLoginAttempt);
                ViewBag.ErrorMessage = Resource.Login_InvalidLoginAttempt;
                return View("LoginLayout", model);
            }

            if (!user.EmailConfirmed)
            {
                string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                string callbackUrl = Url.Action(MVC.Account.ConfirmEmail(user.Id, code), Request.Url.Scheme);

                string mailContent = string.Concat(Resource.EmailConfirmation_Body,
                    " <a href=\"", callbackUrl, "\">CONFIRM</a>");

                string body = MvcUtility.RenderRazorViewToString(this, MVC.Mail.Views.GenericMail,
                    new MailViewModel
                    {
                        Title = Resource.ConfirmEmailView_Title,
                        Content = mailContent,
                        Footer = Resource.NoReply
                    });

                await UserManager.SendEmailAsync(user.Id, Resource.EmailConfirmation_Subject, body);
                ViewBag.Link = callbackUrl;
                return View("DisplayEmail");
            }

            // This doesn't count login failures towards lockout only two factor authentication
            // To enable password failures to trigger lockout, change to shouldLockout: true
            SignInStatus result =
                await SignInManager.PasswordSignInAsync(model.Email, model.Password, true, true);
            ViewBag.ErrorMessage = string.Empty;

            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);

                case SignInStatus.LockedOut:
                    return View("Lockout");

                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl });

                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", Resource.Login_InvalidLoginAttempt);
                    ViewBag.ErrorMessage = Resource.Login_InvalidLoginAttempt;
                    return View("LoginLayout", model);
            }
        }

        [AllowAnonymous]
        public virtual ActionResult ProviderRegister()
        {
            var model = new CreateUserViewModel()
            {
                ContractDate = DateTime.Now,
                AcceptsNotificationOnEmail = true,
                AcceptsNotificationOnSms = true,
                Commission = Common.GetDbConfig("DefaultSupplierCommissionPercentage").ToDecimal(),
                PaymentDelayDays = Common.GetDbConfig("DefaultPaymentDelayDays").ToInteger(),
                IsCompany = true,
            };

            SessionUtility.ClearAddresses();
            model.Addresses = SessionUtility.GetAddresses();

            using (var db = new AppDbContext())
            {
                ViewBag.Counties = db.Counties.OrderBy(x => x.Name).ToList();
                //ViewBag.Banks = db.Banks.OrderBy(x => x.Name).ToList();
            }

            return View("_ProviderRegister", model);
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> ProviderRegister(CreateUserViewModel model, HttpPostedFileBase file)
        {
            model.Addresses = SessionUtility.GetAddresses();

            if (model.Addresses == null || !model.Addresses.Any())
            {
                ModelState.AddModelError("", Resource.Error_MinimumOneAddress);
            }

            if (model.AcceptedTermsFlag == false)
            {
                ModelState.AddModelError("", Resource.Error_Terms);
            }

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    LastName = model.LastName,
                    FirstName = model.FirstName,
                    PhoneNumber = model.PhoneNumber,
                    IsCompany = model.IsCompany,
                    CompanyName = model.CompanyName,
                    Cui = model.Cui,
                    Jno = model.Jno,
                    //Bank = model.Bank,
                    AccountNumber = model.AccountNumber,
                    IdCardNo = model.IdCardNo,
                    Addresses = new List<Address>(),
                    TwoFactorEnabled = false,
                    CreatedDate = DateTime.Now,
                    AcceptedTermsFlag = true,
                    AcceptsNotificationOnEmail = true,
                    AcceptsNotificationOnSms = true
                };

                IdentityResult result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await UserManager.AddToRoleAsync(user.Id, "Provider");

                    string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    string callbackUrl = Url.Action(MVC.Account.ConfirmEmail(user.Id, code), Request.Url.Scheme);

                    string mailContent = string.Concat(Resource.EmailConfirmation_Body,
                        " <a href=\"", callbackUrl, "\">CONFIRM</a>");

                    string body = MvcUtility.RenderRazorViewToString(this, MVC.Mail.Views.GenericMail,
                        new MailViewModel
                        {
                            Title = Resource.ConfirmEmailView_Title,
                            Content = mailContent,
                            Footer = Resource.NoReply
                        });

                    ViewBag.Link = callbackUrl;

                    using (var db = new AppDbContext())
                    {
                        foreach (var address in model.Addresses)
                        {
                            address.Id = 0;
                            address.UserId = user.Id;
                            address.UserCity = null;
                            address.UserCountry = null;
                            db.Addresses.Add(address);
                        }
                        db.SaveChanges();
                    }

                    return View("DisplayEmail");
                }
                AddErrors(result, user);
            }
            else
            {
                using (var db = new AppDbContext())
                {
                    ViewBag.Counties = db.Counties.OrderBy(x => x.Name).ToList();
                    //ViewBag.Banks = db.Banks.OrderBy(x => x.Name).ToList();
                }
            }

            // If we got this far, something failed, redisplay form
            return View("_ProviderRegister", model);
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public virtual async Task<ActionResult> VerifyCode(string provider, string returnUrl)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            ApplicationUser user = await UserManager.FindByIdAsync(await SignInManager.GetVerifiedUserIdAsync());
            if (user != null)
            {
                //ViewBag.Status = "For DEMO purposes the current " + provider + " code is: " + await UserManager.GenerateTwoFactorTokenAsync(user.Id, provider);
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            SignInStatus result =
                await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, false, model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);

                case SignInStatus.LockedOut:
                    return View("Lockout");

                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public virtual ActionResult Register()
        {
            ViewBag.Type = "register";
            return View("LoginLayout");
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> Register(RegisterViewModel model)
        {
            model.AcceptedTermsFlag = true;
            ViewBag.Type = "register";
            ModelState.Clear();

            if (model.Email == null || model.PhoneNumber == null || model.FirstName == null || model.LastName == null || model.Password == null || model.ConfirmPassword == null)
            {
                ModelState.AddModelError("", Resource.Error_MandatoryFields);
                return View("LoginLayout", model);
            }
            else if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("", Resource.User_Password_ConfirmErrorMessage);
                return View("LoginLayout", model);
            }

            if (ModelState.IsValid)
            {
                ApplicationUser anonymous = RetrieveOthers.GetUserByPhoneNumber(model.PhoneNumber);
                if (anonymous == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = model.Email,
                        Email = model.Email,
                        PhoneNumber = model.PhoneNumber,
                        LastName = model.LastName,
                        FirstName = model.FirstName,
                        TwoFactorEnabled = false,
                        CreatedDate = DateTime.Now,
                        AcceptedTermsFlag = model.AcceptedTermsFlag,
                        AcceptsNotificationOnEmail = true,
                        AcceptsNotificationOnSms = true,
                    };

                    IdentityResult result = await UserManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        await UserManager.AddToRoleAsync(user.Id, "Client");

                        string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                        string callbackUrl = Url.Action(MVC.Account.ConfirmEmail(user.Id, code), Request.Url.Scheme);

                        string mailContent = string.Concat(Resource.EmailConfirmation_Body,
                            " <a href=\"", callbackUrl, "\">CONFIRM</a>");

                        string body = MvcUtility.RenderRazorViewToString(this, MVC.Mail.Views.GenericMail,
                            new MailViewModel
                            {
                                Title = Resource.ConfirmEmailView_Title,
                                Content = mailContent,
                                Footer = Resource.NoReply
                            });

                        MailAndSmsUtility.SendEmail(user.Email, Resource.EmailConfirmation_Subject, body);
                        //await UserManager.SendEmailAsync(user.Id, Resources.Resource.EmailConfirmation_Subject, body);
                        ViewBag.Link = callbackUrl;
                        return View("DisplayEmail");
                    }
                    AddErrors(result, user);
                    ModelState.AddModelError("", result.Errors.ToString());
                }
                else if (anonymous.IsAnonymous)
                {
                    anonymous.UserName = model.Email;
                    anonymous.Email = model.Email;
                    anonymous.LastName = model.LastName;
                    anonymous.FirstName = model.FirstName;
                    anonymous.TwoFactorEnabled = false;
                    anonymous.CreatedDate = DateTime.Now;
                    anonymous.AcceptedTermsFlag = true;
                    anonymous.AcceptsNotificationOnEmail = true;
                    anonymous.AcceptsNotificationOnSms = true;
                    UserManager.Update(anonymous);

                    IdentityResult result = UserManager.AddToRole(anonymous.Id, "Client");
                    if (result.Succeeded)
                    {
                        string code1 = await UserManager.GeneratePasswordResetTokenAsync(anonymous.Id);
                        IdentityResult resetPasswordResult =
                            await UserManager.ResetPasswordAsync(anonymous.Id, code1, model.Password);
                        if (resetPasswordResult.Succeeded)
                        {
                            string code2 = await UserManager.GenerateEmailConfirmationTokenAsync(anonymous.Id);
                            string callbackUrl = Url.Action(MVC.Account.ConfirmEmail(anonymous.Id, code2),
                                Request.Url.Scheme);

                            string mailContent = string.Concat(Resource.EmailConfirmation_Body,
                                " <a href=\"", callbackUrl, "\">CONFIRM</a>");

                            string body = MvcUtility.RenderRazorViewToString(this, MVC.Mail.Views.GenericMail,
                                new MailViewModel
                                {
                                    Title = Resource.ConfirmEmailView_Title,
                                    Content = mailContent,
                                    Footer = Resource.NoReply
                                });

                            MailAndSmsUtility.SendEmail(anonymous.Email, Resource.EmailConfirmation_Subject, body);
                            //await UserManager.SendEmailAsync(anonymous.Id, Resources.Resource.EmailConfirmation_Subject, body);
                            ViewBag.Link = callbackUrl;

                            return View("DisplayEmail");
                        }
                        ModelState.AddModelError("", result.Errors.ToString());
                    }
                    else
                    {
                        ModelState.AddModelError("", Resource.Invalid_Phone_Number);
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View("LoginLayout", model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public virtual async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            IdentityResult result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public virtual ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                string callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code },
                    Request.Url.Scheme);
                string mailContent = string.Concat(Resource.ForgotPasswordEmail_Content,
                    " <a href=\"", callbackUrl, "\">Reset</a>");

                string body = MvcUtility.RenderRazorViewToString(this, MVC.Mail.Views.GenericMail,
                    new MailViewModel
                    {
                        Title = Resource.ForgotPasswordView_Title,
                        Content = mailContent,
                        Footer = Resource.NoReply
                    });

                await UserManager.SendEmailAsync(user.Id, Resource.ResetPassword_Subject, body);
                ViewBag.Link = callbackUrl;
                return View("ForgotPasswordConfirmation");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public virtual ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public virtual ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            ApplicationUser user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            IdentityResult result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result, user);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public virtual ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [AllowAnonymous]
        public virtual ActionResult ActivateAccount(string userId, string code)
        {
            var random = new Random();
            var model = new ResetPasswordViewModel
            {
                Code = code,
                MobileCode = random.Next(1000, 9999).ToString()
            };

            var user = RetrieveOthers.GetUserById(userId);
            model.Email = user.Email;
            MailAndSmsUtility.SendSms(user.PhoneNumber, "Codul de validare Plansy.nl este " + model.MobileCode);

            return code == null ? View("Error") : View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> ActivateAccount(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            ApplicationUser user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ActivateAccountConfirmation", "Account");
            }

            bool emailConfirmed = user.EmailConfirmed;

            if (!emailConfirmed)
            {
                string token = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);

                IdentityResult confirmResult = await UserManager.ConfirmEmailAsync(user.Id, token);

                if (model.MobileCode != model.MobileVerifyCode)
                {
                    ModelState.AddModelError("", "Codul este incorect!");
                    return View(model);
                }

                RetrieveOthers.ActivatePhoneNumber(user.Id);

                if (!confirmResult.Succeeded)
                {
                    AddErrors(confirmResult, user);
                    return View(model);
                }
            }

            string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
            IdentityResult resetResult = await UserManager.ResetPasswordAsync(user.Id, code, model.Password);

            if (resetResult.Succeeded)
            {
                SignInStatus loginResult =
                    await SignInManager.PasswordSignInAsync(model.Email, model.Password, true, true);
                if (loginResult == SignInStatus.Success)
                {
                    return RedirectToAction("ActivateAccountConfirmation", "Account");
                }
            }

            AddErrors(resetResult, user);
            return View(model);
        }

        [AllowAnonymous]
        public virtual ActionResult ActivateAccountConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public virtual ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider,
                Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public virtual async Task<ActionResult> SendCode(string returnUrl)
        {
            string userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            IList<string> userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            List<SelectListItem> factorOptions =
                userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, model.ReturnUrl });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public virtual async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            ExternalLoginInfo loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            SignInStatus result = await SignInManager.ExternalSignInAsync(loginInfo, false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);

                case SignInStatus.LockedOut:
                    return View("Lockout");

                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl });

                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation",
                        new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model,
            string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                ExternalLoginInfo info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    AcceptedTermsFlag = true,
                    AcceptsNotificationOnEmail = true,
                    AcceptsNotificationOnSms = true,
                    CreatedDate = DateTime.Now,
                    EmailConfirmed = true
                };
                IdentityResult result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    UserManager.AddToRole(user.Id, "Client");
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, false, false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result, user);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            return RedirectToAction("Login", "Account");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public virtual ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [AllowAnonymous]
        public virtual ActionResult Terms()
        {
            return View();
        }

        [AllowAnonymous]
        public virtual ActionResult Cookies()
        {
            return View();
        }

        [AllowAnonymous]
        public virtual ActionResult Disclaimer()
        {
            return View();
        }


        [AllowAnonymous]
        public virtual ActionResult PrivacyStatement()
        {
            return View();
        }

        #endregion ACTIONS

        #region HELPERS

        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get { return HttpContext.GetOwinContext().Authentication; }
        }

        private void AddErrors(IdentityResult result, ApplicationUser user)
        {
            foreach (string error in result.Errors)
            {
                ModelState.AddModelError("", LocalizeIdentityError(error, user));
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        private string LocalizeIdentityError(string error, ApplicationUser user)
        {
            error = error.Replace("Incorrect password.", Resource.IncorrectPassword);
            error = error.Replace("Passwords must have at least one non letter or digit character.",
                Resource.PasswordNonLetterOrDigit);
            error = error.Replace("Passwords must have at least one uppercase ('A'-'Z').", Resource.PasswordUpperCase);
            error = error.Replace("Passwords must have at least one digit ('0'-'9').", Resource.PasswordDigit);
            error = error.Replace("Passwords must have at least one lowercase ('a'-'z').", Resource.PasswordLowerCase);
            error = error.Replace(" is already taken.", Resource.IsAlreadyTaken);

            return error;

            //if (error == "User already in role.") return "De gebruiker zit reeds in deze rol.";
            //else if (error == "User is not in role.") return "De gebruiker zit niet in deze rol.";
            ////else if (error == "Role {0} does not exist.") return "De rol bestaat nog niet";
            ////else if (error == "Store does not implement IUserClaimStore&lt;TUser&gt;.") return "";
            ////else if (error == "No IUserTwoFactorProvider for '{0}' is registered.") return "";
            ////else if (error == "Store does not implement IUserEmailStore&lt;TUser&gt;.") return "";
            //else if (error == "Incorrect password.") return "Ongeldig wachtwoord";
            ////else if (error == "Store does not implement IUserLockoutStore&lt;TUser&gt;.") return "";
            ////else if (error == "No IUserTokenProvider is registered.") return "";
            ////else if (error == "Store does not implement IUserRoleStore&lt;TUser&gt;.") return "";
            ////else if (error == "Store does not implement IUserLoginStore&lt;TUser&gt;.") return "";
            //else if (error == "User name {0} is invalid, can only contain letters or digits.") return "De gebruikersnaam '" + user.UserName + "' kan alleen letters of cijfers bevatten.";
            ////else if (error == "Store does not implement IUserPhoneNumberStore&lt;TUser&gt;.") return "";
            ////else if (error == "Store does not implement IUserConfirmationStore&lt;TUser&gt;.") return "";
            //else if (error.StartsWith("Passwords must be at least ")) return "Een wachtwoord moet minstens {0} karakters bevatten.";
            ////else if (error == "{0} cannot be null or empty.") return "";
            //else if (user != null && error == "Name " + user.UserName + " is already taken.") return "De gebruikersnaam '" + user.UserName + "' is reeds in gebruik.";
            //else if (error == "User already has a password set.") return "Deze gebruiker heeft reeds een wachtwoord ingesteld.";
            ////else if (error == "Store does not implement IUserPasswordStore&lt;TUser&gt;.") return "";
            //else if (error == "Passwords must have at least one non letter or digit character.") return "Wachtwoorden moeten minstens een ander karakter dan een letter of cijfer bevatten.";
            //else if (error == "UserId not found.") return "De gebruiker kon niet gevonden worden.";
            //else if (error == "Invalid token.") return "Ongeldig token.";
            //else if (user != null && error == "Email '" + user.Email + "' is invalid.") return "Het emailadres '" + user.Email + "' is ongeldig.";
            //else if (user != null && error == "User " + user.UserName + " does not exist.") return "De gebruiker '" + user.UserName + "' bestaat niet.";
            //else if (error == "Store does not implement IQueryableRoleStore&lt;TRole&gt;.") return "";
            //else if (error == "Lockout is not enabled for this user.") return "Lockout is niet geactiveerd voor deze gebruiker.";
            ////else if (error == "Store does not implement IUserTwoFactorStore&lt;TUser&gt;.") return "";
            //else if (error == "Passwords must have at least one uppercase ('A'-'Z').") return "Wachtwoorden moeten minstens één hoofdletter bevatten. (A-Z)";
            //else if (error == "Passwords must have at least one digit ('0'-'9').") return "Wachtwoorden moeten minstens één getal bevatten. (0-9)";
            //else if (error == "Passwords must have at least one lowercase ('a'-'z').") return "Wachtwoorden moeten minstens één kleine letter bevatten. (a-z)";
            ////else if (error == "Store does not implement IQueryableUserStore&lt;TUser&gt;.") return "";
            //else if (user != null && error == "Email '" + user.Email + "' is already taken.") return "Het emailadres '" + user.Email + "' is reeds in gebruik. Probeer aan te melden.";
            ////else if (error == "Store does not implement IUserSecurityStampStore&lt;TUser&gt;.") return "";
            //else if (error == "A user with that external login already exists.") return "Een gebruiker met deze externe login bestaat reeds.";
            //else if (error == "An unknown failure has occured.") return "Een onbekende fout is opgetreden. Probeer het later opnieuw.";

            //return error;
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }

            public string RedirectUri { get; set; }

            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }

        #endregion HELPERS
    }
}