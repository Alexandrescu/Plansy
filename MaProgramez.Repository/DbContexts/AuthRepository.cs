using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Web.ModelBinding;
using MaProgramez.Repository.BusinessLogic;
using MaProgramez.Repository.Entities;
using MaProgramez.Repository.Models;
using MaProgramez.Repository.Utility;
using MaProgramez.Resources;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.DataProtection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SendGrid;
using WebGrease.Css.Extensions;

namespace MaProgramez.Repository.DbContexts
{
    public class AuthRepository : IDisposable
    {
        #region Private Fields

        private readonly AppDbContext _ctx;

        private readonly UserManager<ApplicationUser> _userManager;

        #endregion Private Fields

        #region Public Constructors

        public AuthRepository()
        {

            _ctx = new AppDbContext();

            var provider = new DpapiDataProtectionProvider("MaProgramez");

            _userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_ctx))
            {
                UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser>(
                    provider.Create("MaProgramez"))
            };

            // Configure validation logic for usernames
            _userManager.UserValidator = new UserValidator<ApplicationUser>(_userManager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            _userManager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = false,
                RequireDigit = false,
                RequireLowercase = false,
                RequireUppercase = false,
            };

            // Configure user lockout defaults
            _userManager.UserLockoutEnabledByDefault = true;
            _userManager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            _userManager.MaxFailedAccessAttemptsBeforeLockout = 5;
            // Register two factor authentication providers. This application uses Phone and Emails
            // as a step of receiving a code for verifying the user You can write your own provider
            // and plug in here.

            //manager.RegisterTwoFactorProvider("PhoneCode", new PhoneNumberTokenProvider<User>
            //{
            //    MessageFormat = string.Concat(Resources.YourSecurityCodeIs, " {0}")
            //});

            //manager.RegisterTwoFactorProvider("EmailCode", new EmailTokenProvider<User>
            //{
            //    Subject = "MaProgramez.net",
            //    BodyFormat = string.Concat(Resources.YourSecurityCodeIs, " {0}")
            //});

            _userManager.EmailService = new EmailService();
            _userManager.SmsService = new SmsService();
        }

        #endregion Public Constructors

        #region Public Methods

        public async Task<IdentityResult> AddLoginAsync(string userId, UserLoginInfo login)
        {
            var result = await _userManager.AddLoginAsync(userId, login);

            return result;
        }

        public async Task<bool> AddRefreshToken(RefreshToken token)
        {
            var existingToken = _ctx.RefreshTokens.Where(r => r.Subject == token.Subject && r.ClientId == token.ClientId).SingleOrDefault();

            if (existingToken != null)
            {
                var result = await RemoveRefreshToken(existingToken);
            }

            _ctx.RefreshTokens.Add(token);

            return await _ctx.SaveChangesAsync() > 0;
        }

        public async Task<string> ChangePassword(string userId, string oldPassword, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                return "Parola si confirmarea parolei nu se potrivesc.";
            }

            IdentityResult result =
                await _userManager.ChangePasswordAsync(userId, oldPassword, newPassword);

            if (result.Succeeded)
            {
                return string.Empty;
            }
            else
            {
                if (result.Errors.Any())
                {
                    return result.Errors.First().Replace("Incorrect password.", "Parola incorecta.");
                }
            }

            return "Eroare";
        }

        public async Task<IdentityResult> ChangePhoneNumberAsync(string userId, string phoneNumber, string code)
        {
            return await _userManager.ChangePhoneNumberAsync(userId, phoneNumber, code);
        }

        public async Task<IdentityResult> CreateAsync(ApplicationUser user)
        {
            var result = await _userManager.CreateAsync(user);

            return result;
        }

        public void Dispose()
        {
            _ctx.Dispose();
            _userManager.Dispose();
        }

        public async Task<ApplicationUser> FindAsync(UserLoginInfo loginInfo)
        {
            ApplicationUser user = await _userManager.FindAsync(loginInfo);

            return user;
        }

        public async Task<ApplicationUser> FindByNameAsync(string email)
        {
            ApplicationUser user = await _userManager.FindByNameAsync(email);

            return user;
        }

        public Client FindClient(string clientId)
        {
            var client = _ctx.Clients.Find(clientId);

            return client;
        }

        public async Task<RefreshToken> FindRefreshToken(string refreshTokenId)
        {
            var refreshToken = await _ctx.RefreshTokens.FindAsync(refreshTokenId);

            return refreshToken;
        }

        public async Task<ApplicationUser> FindUser(string userName, string password)
        {
            ApplicationUser user = await _userManager.FindAsync(userName, password);

            return user;
        }

        public async Task<string> GeneratePasswordResetTokenAsync(string userId)
        {
            return await _userManager.GeneratePasswordResetTokenAsync(userId);
        }

        public List<RefreshToken> GetAllRefreshTokens()
        {
            return _ctx.RefreshTokens.ToList();
        }

        public async Task<bool> IsEmailConfirmedAsync(string userId)
        {
            return await _userManager.IsEmailConfirmedAsync(userId);
        }

        public async Task<IdentityResult> RegisterUser(UserModel userModel)
        {
            ApplicationUser anonymous = RetrieveOthers.GetUserByPhoneNumber(userModel.Phone);
            if (anonymous == null)
            {
                var user = new ApplicationUser
                {
                    UserName = userModel.UserName,
                    FirstName = userModel.FirstName,
                    LastName = userModel.LastName,
                    PhoneNumber = userModel.Phone,
                    Email = userModel.UserName
                };
                try
                {
                    user.CreatedDate = DateTime.Now;
                    var result = await _userManager.CreateAsync(user, userModel.Password);
                    if (result.Succeeded)
                    {
                        var result2 = _userManager.AddToRoles(user.Id, new[] { "Client" });
                        if (!result2.Succeeded)
                        {
                            return result2;
                        }
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.StackTrace);
                }
            }
            else if (anonymous.IsAnonymous)
            {
                anonymous.UserName = userModel.UserName;
                anonymous.Email = userModel.Email;
                anonymous.LastName = userModel.LastName;
                anonymous.FirstName = userModel.FirstName;
                anonymous.TwoFactorEnabled = false;
                anonymous.CreatedDate = DateTime.Now;
                anonymous.AcceptedTermsFlag = true;
                anonymous.AcceptsNotificationOnEmail = true;
                anonymous.AcceptsNotificationOnSms = true;
                try
                {
                    var result = await _userManager.UpdateAsync(anonymous);
                    if (!result.Succeeded)
                    {
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.StackTrace);
                }
            }

            return null;
        }

        public async Task<bool> RemoveRefreshToken(string refreshTokenId)
        {
            var refreshToken = await _ctx.RefreshTokens.FindAsync(refreshTokenId);

            if (refreshToken != null)
            {
                _ctx.RefreshTokens.Remove(refreshToken);
                return await _ctx.SaveChangesAsync() > 0;
            }

            return false;
        }

        public async Task<bool> RemoveRefreshToken(RefreshToken refreshToken)
        {
            _ctx.RefreshTokens.Remove(refreshToken);
            return await _ctx.SaveChangesAsync() > 0;
        }

        public async Task SendEmailAsync(string userId, string resetPasswordSubject, string mailContent)
        {
            await _userManager.SendEmailAsync(userId, resetPasswordSubject, mailContent);
        }

        public async Task SendPhoneNumberValidationCode(string userId, string number)
        {
            string code = await _userManager.GenerateChangePhoneNumberTokenAsync(userId, number);

            var message = new IdentityMessage
            {
                Destination = number,
                Body = Resource.YourSecurityCodeIs + code
            };

            await MailAndSmsUtility.SendSmsAsync(message);
        }

        #endregion Public Methods
    }

    public class EmailService : IIdentityMessageService
    {
        #region Public Methods

        /// <summary>
        /// This method should send the message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task SendAsync(IdentityMessage message)
        {
            try
            {
                // Create the email object first, then add the properties.
                var myMessage = new SendGridMessage();
                myMessage.AddTo(message.Destination);
                myMessage.From = new MailAddress("contact@plansy.nl", "Plansy.nl");
                myMessage.Subject = message.Subject;
                //myMessage.Text = message.Body;
                myMessage.Html = message.Body;

                // Create credentials, specifying your user name and password.
                var credentials = new NetworkCredential(Common.GetDbConfig("SendGridUserName"),
                    Common.GetDbConfig("SendGridPassword"));

                // Create an Web transport for sending email.
                var transportWeb = new Web(credentials);

                //Log the mail
                var email = new Email()
                {
                    Destination = message.Destination,
                    Title = message.Subject,
                    Content = message.Body,
                    SendingDateTime = DateTime.Now
                };
                using (var db = new AppDbContext())
                {
                    db.Emails.Add(email);
                    db.SaveChanges();
                }

                // Send the email. You can also use the **DeliverAsync** method, which returns an
                // awaitable task.
                return transportWeb.DeliverAsync(myMessage);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
            }
            return null;
        }

        /// <summary>
        /// Sends the with attachment asynchronous.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="attachmentList">The attachment list.</param>
        /// <returns></returns>
        public Task SendWithAttachmentAsync(IdentityMessage message, List<string> attachmentList)
        {
            try
            {
                // Create the email object first, then add the properties.
                var myMessage = new SendGridMessage();
                myMessage.AddTo(message.Destination);
                myMessage.From = new MailAddress("contact@Plansy.nl", "Plansy.nl");
                myMessage.Subject = message.Subject;
                myMessage.Html = message.Body;

                foreach (var attachment in attachmentList)
                {
                    try
                    {
                        // Add attachment
                        myMessage.AddAttachment(attachment);
                    }
                    catch (Exception ex)
                    {
                        ErrorLogger.LogError(ex);
                    }
                }

                // Create credentials, specifying your user name and password.
                var credentials = new NetworkCredential(Common.GetDbConfig("SendGridUserName"),
                     Common.GetDbConfig("SendGridPassword"));

                // Create an Web transport for sending email.
                var transportWeb = new Web(credentials);

                //Log the mail
                var email = new Email()
                {
                    Destination = message.Destination,
                    Title = message.Subject,
                    Content = message.Body,
                    SendingDateTime = DateTime.Now
                };
                using (var db = new AppDbContext())
                {
                    db.Emails.Add(email);
                    db.SaveChanges();
                }

                // Send the email.
                return transportWeb.DeliverAsync(myMessage);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
            }

            return null;
        }

        #endregion Public Methods
    }

    public class SmsService : IIdentityMessageService
    {
        #region Public Methods

        /// <summary>
        /// This method should send the message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your sms service here to send a text message.
            var client = new SmsServiceReference.SmsWcfServiceClient();

            var src = string.Concat("2,", DateTime.Now.ToString(CultureInfo.InvariantCulture));
            var source = Cryptography.EncryptStringAES(src, Common.GetAppConfig("PublicKey"));

            return client.SendSmsAsync(source, message.Destination, message.Body, null);
            //return Task.FromResult(0);
        }

        #endregion Public Methods
    }
}