using System.ComponentModel.DataAnnotations;
using MaProgramez.Resources;

namespace MaProgramez.Repository.Models
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = null, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        [EmailAddress(ErrorMessageResourceType = typeof(Resource), ErrorMessage = null, ErrorMessageResourceName = "Email_ErrorMessage")]
        [Display(Name = @"Email")]
        public string Email { get; set; }
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
}
