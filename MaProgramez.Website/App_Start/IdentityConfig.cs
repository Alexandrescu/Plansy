namespace MaProgramez.Website
{
    using Repository.DbContexts;
    using Repository.Entities;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Microsoft.AspNet.Identity.Owin;
    using Microsoft.Owin;
    using Microsoft.Owin.Security;
    using SendGrid;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net;
    using System.Net.Mail;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Utility;

    // Configure the application user manager used in this application. UserManager is defined in
    // ASP.NET Identity and is used by the application.

    // Configure the RoleManager used in the application. RoleManager is defined in the ASP.NET
    // Identity core assembly
    public class ApplicationRoleManager : RoleManager<IdentityRole>
    {
        #region Public Constructors

        public ApplicationRoleManager(IRoleStore<IdentityRole, string> roleStore)
            : base(roleStore)
        {
        }

        #endregion Public Constructors

        #region Public Methods

        public static ApplicationRoleManager Create(IdentityFactoryOptions<ApplicationRoleManager> options, IOwinContext context)
        {
            return new ApplicationRoleManager(new RoleStore<IdentityRole>(context.Get<AppDbContext>()));
        }

        #endregion Public Methods
    }

    public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
    {
        #region Public Constructors

        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager) :
            base(userManager, authenticationManager) { }

        #endregion Public Constructors

        #region Public Methods

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        }

        #endregion Public Methods
    }

    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        #region Public Constructors

        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {
        }

        #endregion Public Constructors

        #region Public Methods

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<AppDbContext>()));
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };
            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = false,
                RequireDigit = false,
                RequireLowercase = true,
                RequireUppercase = false,
            };
            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;
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
            manager.EmailService = new EmailService();
            manager.SmsService = new SmsService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
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
                myMessage.From = new MailAddress("contact@Plansy.nl", "Plansy.nl");
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