using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MaProgramez.Repository.Entities;
using MaProgramez.Resources;
using MaProgramez.Website.Extensions;

namespace MaProgramez.Website.Models
{
    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required(ErrorMessage = null, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        public string Provider { get; set; }

        [Required(ErrorMessage = null, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        [Display(ResourceType = typeof(Resource), Name = "Code")]
        public string Code { get; set; }

        public string ReturnUrl { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "Remember_This_Browser")]
        public bool RememberBrowser { get; set; }
    }

    public class ExternalLoginConfirmationViewModel
    {
        [Required(ErrorMessage = null, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        [EmailAddress(ErrorMessageResourceType = typeof(Resource), ErrorMessage = null, ErrorMessageResourceName = "Email_ErrorMessage")]
        [Display(Name = @"Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = null, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        [EnforceTrue(ErrorMessage = null, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        public bool AcceptedTermsFlag { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        //public string Action { get; set; }
        public string ReturnUrl { get; set; }
    }

    public class ManageUserViewModel
    {
        [Required(ErrorMessage = null, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(Resource), Name = "Current_Password")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = null, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        //[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = null, ErrorMessageResourceName = "User_Password_ErrorMessage", ErrorMessageResourceType = typeof(Resource))]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(Resource), Name = "New_Password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(Resource), Name = "User_Confirm_Password")]
        [Compare("Password", ErrorMessage = null, ErrorMessageResourceName = "User_Password_ConfirmErrorMessage", ErrorMessageResourceType = typeof(Resource))]
        public string ConfirmPassword { get; set; }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = null, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        [EmailAddress(ErrorMessageResourceType = typeof(Resource), ErrorMessage = null, ErrorMessageResourceName = "Email_ErrorMessage")]
        [Display(Name = @"Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = null, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(Resource), Name = "User_Password")]
        public string Password { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "RememberMe")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = null, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        [EmailAddress(ErrorMessageResourceType = typeof(Resource), ErrorMessage = null, ErrorMessageResourceName = "Email_ErrorMessage")]
        [Display(Name = @"Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = null, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        [StringLength(100, MinimumLength = 6, ErrorMessage = null, ErrorMessageResourceName = "User_Password_ErrorMessage", ErrorMessageResourceType = typeof(Resource))]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(Resource), Name = "User_Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(Resource), Name = "User_Confirm_Password")]
        [Compare("Password", ErrorMessage = null, ErrorMessageResourceName = "User_Password_ConfirmErrorMessage", ErrorMessageResourceType = typeof(Resource))]
        public string ConfirmPassword { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "User_FirstName")]
        [Required(ErrorMessage = null, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        public string FirstName { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "User_Name")]
        [Required(ErrorMessage = null, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        public string LastName { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "User_DateOfBirth")]
        [Required(ErrorMessage = null, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        [DataType(DataType.Date, ErrorMessage = null, ErrorMessageResourceName = "Date_Error_Message", ErrorMessageResourceType = typeof(Resource))]
        public DateTime BirthDate { get; set; }

        [Required(ErrorMessage = null, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        [EnforceTrue(ErrorMessage = null, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        public bool AcceptedTermsFlag { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "User_Phone")]
        [Phone(ErrorMessage = null, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "Invalid_Phone_Number")]
        [Required(ErrorMessage = null, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        public string PhoneNumber { get; set; }
    }

    public class CreateUserViewModel
    {
        [Required(ErrorMessage = null, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        [EmailAddress(ErrorMessageResourceType = typeof(Resource), ErrorMessage = null, ErrorMessageResourceName = "Email_ErrorMessage")]
        [Display(Name = @"Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = null, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        [StringLength(100, MinimumLength = 6, ErrorMessage = null, ErrorMessageResourceName = "User_Password_ErrorMessage", ErrorMessageResourceType = typeof(Resource))]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(Resource), Name = "User_Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(Resource), Name = "User_Confirm_Password")]
        [Compare("Password", ErrorMessage = null, ErrorMessageResourceName = "User_Password_ConfirmErrorMessage", ErrorMessageResourceType = typeof(Resource))]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = null, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        [EnforceTrue(ErrorMessage = null, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        public bool AcceptedTermsFlag { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "User_FirstName")]
        [Required(ErrorMessage = null, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        public string FirstName { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "User_Name")]
        [Required(ErrorMessage = null, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        public string LastName { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "User_CompanyName")]
        [RequiredIf("IsCompany", true, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        public string CompanyName { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "User_DateOfBirth")]
        [DataType(DataType.Date, ErrorMessage = null, ErrorMessageResourceName = "Date_Error_Message", ErrorMessageResourceType = typeof(Resource))]
        public DateTime BirthDate { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "User_IsCompany")]
        public bool IsCompany { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "User_AcceptsNotificationOnEmail")]
        public bool AcceptsNotificationOnEmail { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "User_AcceptsNotificationOnSms")]
        public bool AcceptsNotificationOnSms { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "ProgrammingPer")]
        public bool ProgrammingPerSlot { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "Description")]
        public string FullDescription { get; set; }
        
        [Display(ResourceType = typeof(Resource), Name = "Alias")]
        public string Alias { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "User_Commission")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal Commission { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "User_PaymentDelayDays")]
        public int PaymentDelayDays { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "User_LogoPath")]
        public string LogoPath { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "User_CUI")]
        [RequiredIf("IsCompany", true, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        public string Cui { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "User_JNO")]
        [RequiredIf("IsCompany", true, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        public string Jno { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "User_Bank")]
        //[RequiredIf("IsCompany", true, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        public string Bank { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "User_AccountNumber")]
        [RequiredIf("IsCompany", true, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        public string AccountNumber { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "User_IdCardNo")]
        public string IdCardNo { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "ContractNo")]
        public int? ContractNo { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "ContractDate")]
        public DateTime? ContractDate { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "User_Phone")]
        [Phone(ErrorMessage = null, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "Invalid_Phone_Number")]
        //[Required(ErrorMessage = null, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        public string PhoneNumber { get; set; }

        public IEnumerable<Address> Addresses { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "User_InvoiceSum")]
        public int InvoiceSum { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "Category")]
        public int SelectedCategoryId { get; set; }
        public Dictionary<int, string> Categories { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "SubscriptionPeriod")]
        public int SubscriptionPeriod { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "WithReceipt")]
        public bool WithReceipt { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = null, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        [EmailAddress(ErrorMessageResourceType = typeof(Resource), ErrorMessage = null, ErrorMessageResourceName = "Email_ErrorMessage")]
        [Display(Name = @"Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = null, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        [StringLength(100, MinimumLength = 6, ErrorMessage = null, ErrorMessageResourceName = "User_Password_ErrorMessage", ErrorMessageResourceType = typeof(Resource))]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(Resource), Name = "User_Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(Resource), Name = "User_Confirm_Password")]
        [Compare("Password", ErrorMessage = null, ErrorMessageResourceName = "User_Password_ConfirmErrorMessage", ErrorMessageResourceType = typeof(Resource))]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }

        public string MobileCode { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "VerifyMobileCode")]
        public string MobileVerifyCode { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = null, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        [EmailAddress(ErrorMessageResourceType = typeof(Resource), ErrorMessage = null, ErrorMessageResourceName = "Email_ErrorMessage")]
        [Display(Name = @"Email")]
        public string Email { get; set; }
    }

    public class ForgotViewModel
    {
        [Required(ErrorMessage = null, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        [Display(Name = @"Email")]
        public string Email { get; set; }
    }
}
