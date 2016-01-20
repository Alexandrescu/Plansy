using MaProgramez.Resources;
using System.ComponentModel.DataAnnotations;

namespace MaProgramez.Repository.Models
{
    public class AddPhoneNumberViewModel
    {
        #region Public Properties

        [Phone(ErrorMessage = null, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "Invalid_Phone_Number")]
        [Required(ErrorMessage = null, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        public string Number { get; set; }

        public string UserId;

        #endregion Public Properties
    }

    public class VerifyPhoneNumberViewModel
    {
        #region Public Properties

        [Required(ErrorMessage = null, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        [Display(ResourceType = typeof(Resource), Name = "Code")]
        public string Code { get; set; }

        [Phone(ErrorMessage = null, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "Invalid_Phone_Number")]
        [Required(ErrorMessage = null, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        public string PhoneNumber { get; set; }
        
        public string UserId;

        #endregion Public Properties
    }
}