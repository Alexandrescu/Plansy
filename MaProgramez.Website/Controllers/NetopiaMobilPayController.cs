using System.Data.Entity;
using Elmah;
using MaProgramez.Repository.DbContexts;
using MaProgramez.Repository.Entities;
using MaProgramez.Resources;
using MaProgramez.Website.Utility;
using MaProgramez.Website.ViewModels;
using Microsoft.AspNet.Identity;
using MobilpayEncryptDecrypt;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml;
using Common = MaProgramez.Repository.Utility.Common;
using MailAndSmsUtility = MaProgramez.Repository.Utility.MailAndSmsUtility;

namespace MaProgramez.Website.Controllers
{
  
    public partial class NetopiaMobilPayController : BaseController
    {
        #region Private Fields

        private readonly string signature = Common.GetDbConfig("NetopiaSignature");
        private readonly string urlMobilPay = Common.GetDbConfig("NetopiaUrl");

        private CardTransaction transaction;

        #endregion Private Fields

        #region Public Methods

        /// <summary>
        /// Confirms the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="env_key">The env_key.</param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public virtual ActionResult Confirm(string data, string env_key)
        {
            string errorCode = "0";
            string errorMessage = "";
            string errorType = "";
            var u = new UrlHelper(ControllerContext.RequestContext);

            if (Request.RequestType.ToUpper() == "POST")
            {
                if (string.IsNullOrWhiteSpace(data) & string.IsNullOrWhiteSpace(env_key))
                {
                    errorType = "0x02";
                    errorCode = "0x300000f5";
                    errorMessage = "mobilpay.ro posted invalid parameters";
                }
                else
                {
                    try
                    {
                        var decrypt = new MobilpayDecrypt
                        {
                            Data = data,
                            EnvelopeKey = env_key,
                            PrivateKeyFilePath = Path.Combine(Server.MapPath("~"), "NetopiaMobilPay", "private.key"),
                        };

                        var encrdecr = new MobilpayEncryptDecrypt.MobilpayEncryptDecrypt();
                        encrdecr.Decrypt(decrypt);

                        using (var db = new AppDbContext())
                        {
                            var xmlDocument = new XmlDocument();
                            xmlDocument.LoadXml(decrypt.DecryptedData);
                            int invoiceId = xmlDocument.DocumentElement.Attributes["id"].Value.ToInteger();

                            transaction = db.CardTransactions.FirstOrDefault(x => x.InvoiceId == invoiceId);

                            var tranHistory = new CardTransactionHistory
                            {
                                CardTransactionId = transaction.Id,
                                ResponseXmlContent = decrypt.DecryptedData
                            };

                            db.CardTransactionHistories.Add(tranHistory);

                            Mobilpay_Payment_Request_Card card = encrdecr.GetCard(decrypt.DecryptedData);
                            switch (card.Confirm.Action)
                            {
                                case "confirmed": //order_status = 'C' (confirmed)
                                case "confirmed_pending": //order_status = 'P' (pending)
                                case "paid_pending": //order_status = 'P' (pending)
                                case "paid": //order_status = 'P' (open/preauthorized - pending)
                                case "canceled": //order_status = 'X' (canceled)
                                case "credit": //order_status = 'R' (refunded)
                                    {
                                        errorMessage = card.Confirm.Crc;

                                        if (card.Confirm.Error.Code == "0" || //success
                                            Common.GetDbConfig("Environment") != "LIVE")
                                        // on DEMO and TEST go further as if the payment was OK
                                        {
                                            try
                                            {
                                                // Add payment line in InvoicePayment
                                                var paymentLine = new InvoicePayment()
                                                {
                                                    Amount = card.Confirm.Original_Amount,
                                                    Date = DateTime.Now,
                                                    Details = card.Invoice.Details,
                                                    InvoiceHeaderId = card.OrderId.ToInteger(),
                                                    PaymentMethod = PaymentMethod.Card
                                                };

                                                db.InvoicePayments.Add(paymentLine);

                                                // Activate account
                                                var invoice = db.InvoiceHeaders.Include("User").FirstOrDefault(ih => ih.Id == invoiceId);
                                                if (invoice != null)
                                                {
                                                    var user = invoice.User;
                                                    user.EmailConfirmed = true;
                                                    db.Entry(user).State = EntityState.Modified;
                                                }

                                                db.SaveChanges();
                                            }
                                            catch (Exception ex)
                                            {
                                                ErrorSignal.FromCurrentContext().Raise(ex);
                                            }
                                        }
                                        else //something is wrong
                                        {
                                            try
                                            {
                                                tranHistory.ErrorCode = card.Confirm.Error.Code.ToInteger();

                                                //send email to admin
                                                string link = u.Action("InvoicePdf", "Invoice",
                                                    new RouteValueDictionary(
                                                        new { invoiceHeaderId = transaction.InvoiceId }));

                                                MailAndSmsUtility.SendEmail(Common.GetDbConfig("AdminMail"), "Netopia card payment error",
                                                    "Error: " + card.Confirm.Error.Code + " - " + card.Confirm.Error.Message +
                                                    "\n\rAccesati factura aici: " +
                                                    Common.GetDbConfig("SiteUrl") + link);

                                                //send email to client
                                                ApplicationUser client = db.InvoiceHeaders.Find(transaction.InvoiceId).User;

                                                if (client.Email != null)
                                                {
                                                    string body = MvcUtility.RenderRazorViewToString(this,
                                                        MVC.Mail.Views.GenericMail,
                                                        new MailViewModel
                                                        {
                                                            Title = "Eroare plata cu cardul",
                                                            Content = "Ne pare rau, plata dumneavoastra pentru factura \"" +
                                                                      transaction.Invoice.Number + " din " + transaction.Invoice.Date.ToString("dd-MM-yyyy") +
                                                                      "\" nu s-a putut finaliza. Va rugam sa ne contactati pentru detalii la admin@maprogramez.net",
                                                            Footer = Resource.NoReply
                                                        });

                                                    MailAndSmsUtility.SendEmail(client.Email, "Eroare plata cu cardul", body);
                                                }

                                                //send sms to client
                                                if (client.AcceptsNotificationOnSms && client.PhoneNumber != null)
                                                {
                                                    MailAndSmsUtility.SendSms(client.PhoneNumber,
                                                       "Ne pare rau, plata dumneavoastra pentru factura \"" +
                                                        transaction.Invoice.Number + " din " + transaction.Invoice.Date.ToString("dd-MM-yyyy") +
                                                        "\" nu s-a putut finaliza. Va rugam sa ne contactati pentru detalii la admin@maprogramez.net");
                                                }

                                                //send notification
                                                var notification = new Notification
                                                {
                                                    UserId = client.Id,
                                                    Type = NotificationType.SystemAlert,
                                                    Text = "Ne pare rau, plata dumneavoastra pentru factura \"" +
                                                        transaction.Invoice.Number + " din " + transaction.Invoice.Date.ToString("dd-MM-yyyy") +
                                                        "\" nu s-a putut finaliza. "
                                                };
                                                db.Notifications.Add(notification);

                                                if (db.SaveChanges() < 0)
                                                {
                                                    return RedirectToAction(MVC.Error.Index());
                                                }

                                                NotificationCenter.SendNotificationToUser(client.Id,
                                                    RenderRazorViewToString(
                                                        "~/Views/Shared/PartialViews/_Notification.cshtml",
                                                        notification));
                                            }
                                            catch (Exception ex)
                                            {
                                                ErrorSignal.FromCurrentContext().Raise(ex);
                                            }
                                        }

                                        break;
                                    }
                                default:
                                    {
                                        try
                                        {
                                            //send email to admin
                                            string link = u.Action("InvoicePdf", "Invoice",
                                                new RouteValueDictionary(
                                                    new { invoiceHeaderId = transaction.InvoiceId }));
                                            MailAndSmsUtility.SendEmail(Common.GetDbConfig("AdminMail"),
                                                "Netopia failed - card.Confirm.Action != confirmed",
                                                "Decrypted response: " + decrypt.DecryptedData +
                                                "\n\rAccesati factura aici: " +
                                                Common.GetDbConfig("SiteUrl") + link);

                                            //send email to client
                                            ApplicationUser client =
                                                db.InvoiceHeaders.Find(transaction.InvoiceId).User;
                                            if (client.Email != null)
                                            {
                                                string body = MvcUtility.RenderRazorViewToString(this,
                                                    MVC.Mail.Views.GenericMail,
                                                    new MailViewModel
                                                    {
                                                        Title = "Eroare plata cu cardul",
                                                        Content = "Ne pare rau, plata dumneavoastra pentru factura \"" +
                                                        transaction.Invoice.Number + " din " + transaction.Invoice.Date.ToString("dd-MM-yyyy") +
                                                        "\" nu s-a putut finaliza. Va rugam sa ne contactati pentru detalii la admin@maprogramez.net",
                                                        Footer = Resource.NoReply
                                                    });

                                                MailAndSmsUtility.SendEmail(client.Email, "Eroare plata cu cardul", body);
                                            }

                                            //send sms to client
                                            if (client.AcceptsNotificationOnSms && client.PhoneNumber != null)
                                            {
                                                MailAndSmsUtility.SendSms(client.PhoneNumber,
                                                    "Ne pare rau, plata dumneavoastra pentru factura \"" +
                                                        transaction.Invoice.Number + " din " + transaction.Invoice.Date.ToString("dd-MM-yyyy") +
                                                        "\" nu s-a putut finaliza. Va rugam sa ne contactati pentru detalii la admin@maprogramez.net");
                                            }

                                            //send notification
                                            var notification = new Notification
                                            {
                                                UserId = client.Id,
                                                Type = NotificationType.SystemAlert,
                                                Text = "Ne pare rau, plata dumneavoastra pentru factura \"" +
                                                        transaction.Invoice.Number + " din " + transaction.Invoice.Date.ToString("dd-MM-yyyy") +
                                                        "\" nu s-a putut finaliza."
                                            };
                                            db.Notifications.Add(notification);

                                            if (db.SaveChanges() < 0)
                                            {
                                                return RedirectToAction(MVC.Error.Index());
                                            }
                                            NotificationCenter.SendNotificationToUser(client.Id,
                                                RenderRazorViewToString(
                                                    "~/Views/Shared/PartialViews/_Notification.cshtml",
                                                    notification));
                                        }
                                        catch (Exception ex)
                                        {
                                            ErrorSignal.FromCurrentContext().Raise(ex);
                                        }

                                        errorType = "0x02";
                                        errorCode = "0x300000f6";
                                        errorMessage = "mobilpay_refference_action paramaters is invalid";
                                        break;
                                    }
                            }

                            if (db.SaveChanges() < 0)
                            {
                                return RedirectToAction(MVC.Error.Index());
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //send email to admin
                        string link = u.Action("InvoicePdf", "Invoice",
                            new RouteValueDictionary(
                                new { invoiceHeaderId = transaction.InvoiceId }));
                        MailAndSmsUtility.SendEmail(Common.GetDbConfig("AdminMail"), "Netopia - exception on action Confirm",
                            "Eroarea aparuta: " + ex.Message + Environment.NewLine + ex.StackTrace +
                            "\n\rAccesati factura aici: " +
                            Common.GetDbConfig("SiteUrl") + link);

                        ErrorSignal.FromCurrentContext().Raise(ex);

                        errorType = "0x01";
                        errorCode = "1032";
                        errorMessage = ex.Message;
                    }
                }
            }
            else
            {
                errorType = "0x02";
                errorCode = "0x300000f4";
                errorMessage = "invalid request metod for payment confirmation";
            }

            Response.ContentType = "text/xml";
            string message = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n";
            if (errorCode == "0")
            {
                message = message + "<crc>" + errorMessage + "</crc>";
            }
            else
            {
                message = message + "<crc error_type=\"" + errorType + "\" error_code=\"" + errorCode + "\"> " +
                          errorMessage + "</crc>";
            }
            Response.Write(message);

            return null;
        }

        /// <summary>
        /// Pays the by card.
        /// </summary>
        /// <param name="invoiceId">The invoice identifier.</param>
        /// <param name="userToken">The user token.</param>
        /// <returns></returns>
        [AllowAnonymous]
        public virtual ActionResult PayByCard(int invoiceId, string userToken)
        {
            using (var db = new AppDbContext())
            {
                string userId = User.Identity.GetUserId();
                var invoiceHeader = db.InvoiceHeaders.Include("InvoiceLines").FirstOrDefault(i=>i.Id == invoiceId);

                if (invoiceHeader == null && userId == userToken)
                {
                    ViewBag.Message = "InvoiceId invalid";
                    return View();
                }

                transaction = new CardTransaction
                {
                    Amount = invoiceHeader.InvoiceLines.Sum(il => il.Price * il.Quantity),
                    Currency = "RON",
                    Details = "Factura " + invoiceHeader.Series + " " + invoiceHeader.Number + " din " + invoiceHeader.Date.ToString("dd-MM-yyyy"),
                    UserId = User.Identity.GetUserId(),
                    Date = DateTime.Now,
                    InvoiceId = invoiceId
                };

                db.CardTransactions.Add(transaction);

                if (db.SaveChanges() < 0)
                {
                    return RedirectToAction(MVC.Error.Index());
                }

                transaction =
                    (from tran in
                        db.CardTransactions.Include("User.Addresses")
                            .Include("User.Addresses.UserCity")
                            .Include("User.Addresses.UserCountry")
                            .Include("User.Addresses.UserCity.CityCounty")
                        where tran.Id == transaction.Id
                        select tran).First();

                var encrypt = new MobilpayEncrypt();

                var card = new Mobilpay_Payment_Request_Card();
                var invoice = new Mobilpay_Payment_Invoice();
                var billing = new Mobilpay_Payment_Address();
                var shipping = new Mobilpay_Payment_Address();
                //var itmm = new Mobilpay_Payment_Invoice_Item();
                //var itmm1 = new Mobilpay_Payment_Invoice_Item();
                //var itmColl = new Mobilpay_Payment_ItemCollection();
                //var exColl = new Mobilpay_Payment_Exchange_RateCollection();
                //var ex = new Mobilpay_Payment_Exchange_Rate();
                var ctinfo = new Mobilpay_Payment_Request_Contact_Info();
                var conf = new Mobilpay_Payment_Confirm();
                var url = new Mobilpay_Payment_Request_Url();

                var encdecr = new MobilpayEncryptDecrypt.MobilpayEncryptDecrypt();

                card.OrderId = transaction.InvoiceId.ToString(CultureInfo.InvariantCulture);
                card.Type = "card";
                card.Signature = signature;
                url.ConfirmUrl = Common.GetDbConfig("NetopiaConfirmUrl");
                url.ReturnUrl = Common.GetDbConfig("NetopiaReturnUrl");
                //card.Service = service;
                card.Url = url;
                card.TimeStamp = DateTime.Now.ToString("yyyyMMddhhmmss");
                invoice.Amount = transaction.Amount;
                invoice.Currency = string.IsNullOrWhiteSpace(transaction.Currency) ? "RON" : transaction.Currency;
                invoice.Details = transaction.Details;
                billing.FirstName = transaction.User.FirstName;
                billing.LastName = transaction.User.LastName;
                //billing.IdentityNumber = txtbilling_identity_number.Text;
                //billing.FiscalNumber = txtbilling_fiscal_number.Text;
                billing.MobilPhone = transaction.User.PhoneNumber;
                billing.Type = transaction.User.IsCompany ? "company" : "person";
                //billing.Iban = txtbilling_iban.Text;
                //billing.Bank = txtbilling_bank.Text;

                if (transaction.User.Addresses.Any(a => a.AddressType == AddressType.InvoiceAddress))
                {
                    Address address = transaction.User.Addresses.First(a => a.AddressType == AddressType.InvoiceAddress);
                    billing.Address = address.AddressText;
                    billing.ZipCode = address.PostalCode;
                    billing.City = address.UserCity.Name;
                    billing.Country = address.UserCountry.Name;
                    billing.County = address.UserCity.CityCounty.Name;
                }

                billing.Email = transaction.User.Email;

                //shipping.FirstName = txtshipping_first_name.Text;
                //shipping.LastName = txtshipping_last_name.Text;
                //shipping.IdentityNumber = txtshipping_identity_number.Text;
                //shipping.FiscalNumber = txtshipping_fiscal_number.Text;
                //shipping.MobilPhone = txtshipping_mobile_phone.Text;
                //shipping.Type = drshipping_type.SelectedValue;
                //shipping.ZipCode = txtshipping_zip_code.Text;
                //shipping.Iban = txtshipping_iban.Text;
                //shipping.Address = txtshipping_address.Text;
                //shipping.Bank = txtshipping_bank.Text;
                //shipping.City = txtshipping_city.Text;
                //shipping.Country = txtshipping_country.Text;
                //shipping.County = txtshipping_county.Text;
                //shipping.Email = txtshipping_email.Text;

                ctinfo.Billing = billing;
                //ctinfo.Shipping = shipping;
                invoice.ContactInfo = ctinfo;
                card.Invoice = invoice;
                encrypt.Data = encdecr.GetXmlText(card);
                encrypt.X509CertificateFilePath = Path.Combine(Server.MapPath("~"), "NetopiaMobilPay", "public.cer");
                encdecr.Encrypt(encrypt);

                var coll = new NameValueCollection();
                coll.Add("data", encrypt.EncryptedData);
                coll.Add("env_key", encrypt.EnvelopeKey);

                transaction.TransactionHistories = new List<CardTransactionHistory>
                {
                    new CardTransactionHistory
                    {
                        RequestXmlContent = encrypt.Data
                    }
                };

                if (db.SaveChanges() < 0)
                {
                    return RedirectToAction(MVC.Error.Index());
                }

                string redirectForm = HttpHelper.PreparePOSTForm(urlMobilPay, coll);
                return Content(redirectForm);
            }
        }

        /// <summary>
        /// Returns the specified order identifier.
        /// </summary>
        /// <param name="invoiceId">The invoice identifier.</param>
        /// <returns></returns>
        [AllowAnonymous]
        public virtual ActionResult Return(int invoiceId)
        {
            //return RedirectToAction(MVC.Request.View(invoiceId, ""));
            using (var db = new AppDbContext())
            {
                var invoice = db.InvoiceHeaders.FirstOrDefault(i => i.Id == invoiceId);

                return RedirectToConfirmation(
                    new ConfirmationViewModel
                    {
                        Title = Resource.FinalizePaymentSuccess_Title,
                        Message = Resource.FinalizePaymentSuccess_Message,
                        Type = ConfirmationType.Success,
                        Link = invoice != null
                            ? Url.Action(MVC.Invoice.InvoicePdf(invoice.Id))
                            : Url.Action(MVC.Home.Index()),
                    });
            }
        }

        #endregion Public Methods
    }
}