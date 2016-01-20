using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MaProgramez.Repository.Entities;
using MaProgramez.Resources;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;

namespace MaProgramez.Website.ViewModels
{
    public class IndexViewModel
    {
        public bool HasPassword { get; set; }
        public IList<UserLoginInfo> Logins { get; set; }
        public string PhoneNumber { get; set; }
        public bool TwoFactor { get; set; }
        public bool BrowserRemembered { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "User_FirstName")]
        [Required(ErrorMessage = null, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        public string FirstName { get; set; }
        
        [Display(ResourceType = typeof(Resource), Name = "User_Name")]
        [Required(ErrorMessage = null, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        public string LastName { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "User_Commission")]
        public decimal Commission { get; set; }

        public List<Address> Addresses { get; set; }
    }

    public class AddressViewModel
    {
        public Address Address { get; set; }

        public List<City> Cities { get; set; }

        public List<County> Counties { get; set; } 
    }

    public class ManageLoginsViewModel
    {
        public IList<UserLoginInfo> CurrentLogins { get; set; }
        public IList<AuthenticationDescription> OtherLogins { get; set; }
    }

    public class FactorViewModel
    {
        public string Purpose { get; set; }
    }

    public class SetPasswordViewModel
    {
        [Required(ErrorMessage = null, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        [StringLength(100, MinimumLength = 6, ErrorMessage = null, ErrorMessageResourceName = "User_Password_ErrorMessage", ErrorMessageResourceType = typeof(Resource))]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(Resource), Name = "New_Password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(Resource), Name = "User_Confirm_Password")]
        [Compare("Password", ErrorMessage = null, ErrorMessageResourceName = "User_Password_ConfirmErrorMessage", ErrorMessageResourceType = typeof(Resource))]
        public string ConfirmPassword { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = null, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(Resource), Name = "Current_Password")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = null, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        [StringLength(100, MinimumLength = 6, ErrorMessage = null, ErrorMessageResourceName = "User_Password_ErrorMessage", ErrorMessageResourceType = typeof(Resource))]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(Resource), Name = "New_Password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(Resource), Name = "User_Confirm_Password")]
        [Compare("NewPassword", ErrorMessage = null, ErrorMessageResourceName = "User_Password_ConfirmErrorMessage", ErrorMessageResourceType = typeof(Resource))]
        public string ConfirmPassword { get; set; }
    }


    public class AddPhoneNumberViewModel
    {
        [Display(ResourceType = typeof(Resource), Name = "User_Phone")]
        [Phone(ErrorMessage = null, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "Invalid_Phone_Number")]
        [Required(ErrorMessage = null, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        public string Number { get; set; }
    }

    public class VerifyPhoneNumberViewModel
    {
        [Required(ErrorMessage = null, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        [Display(ResourceType = typeof(Resource), Name = "Code")]
        public string Code { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "User_Phone")]
        [Phone(ErrorMessage = null, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "Invalid_Phone_Number")]
        [Required(ErrorMessage = null, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        public string PhoneNumber { get; set; }
    }

    public class ConfigureTwoFactorViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
    }

}