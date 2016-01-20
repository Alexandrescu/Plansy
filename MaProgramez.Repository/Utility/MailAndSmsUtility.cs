using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using MaProgramez.Repository.DbContexts;
using MaProgramez.Repository.Entities;
using Microsoft.AspNet.Identity;
using SendGrid;

namespace MaProgramez.Repository.Utility
{
    public class MailAndSmsUtility
    {
        #region PUBLIC STATIC METHODS

        /// <summary>
        /// Sends the email.
        /// </summary>
        /// <param name="emailAddress">The email address.</param>
        /// <param name="header">The header.</param>
        /// <param name="body">The body.</param>
        public static void SendEmail(string emailAddress, string header, string body)
        {
            var identityMessage = new IdentityMessage
            {
                Destination = emailAddress,
                Subject = header,
                Body = body
            };

            SendMailAsync(identityMessage);
        }

        /// <summary>
        /// Sends the email.
        /// </summary>
        /// <param name="emailAddress">The email address.</param>
        /// <param name="header">The header.</param>
        /// <param name="body">The body.</param>
        /// <param name="attachPaths">The attach paths.</param>
        public static void SendEmail(string emailAddress, string header, string body, List<string> attachPaths)
        {
            var identityMessage = new IdentityMessage
            {
                Destination = emailAddress,
                Subject = header,
                Body = body
            };

            SendMailWithAttachmentAsync(identityMessage, attachPaths);
        }

        /// <summary>
        /// Sends the SMS.
        /// </summary>
        /// <param name="phoneNo">The phone no.</param>
        /// <param name="message">The message.</param>
        public static void SendSms(string phoneNo, string message)
        {
            var sms = new IdentityMessage
            {
                Destination = phoneNo,
                Body = message
            };

            SendSmsAsync(sms);
        }

        #endregion


        #region PRIVATE METHODS

        /// <summary>
        /// Sends the mail asynchronous.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public static Task SendMailAsync(IdentityMessage message)
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

                // Send the email.
                // You can also use the **DeliverAsync** method, which returns an awaitable task.
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
        public static Task SendMailWithAttachmentAsync(IdentityMessage message, List<string> attachmentList)
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

        /// <summary>
        /// This method should send the message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Task SendSmsAsync(IdentityMessage message)
        {
            // Plug in your sms service here to send a text message.
            var client = new SmsServiceReference.SmsWcfServiceClient();

            var src = string.Concat("2,", DateTime.Now.ToString(CultureInfo.InvariantCulture));
            var source = Cryptography.EncryptStringAES(src, Common.GetAppConfig("PublicKey"));

            return client.SendSmsAsync(source, message.Destination, message.Body, null);
        }


        #endregion
    }
}
