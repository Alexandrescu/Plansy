using MaProgramez.Repository.Entities;
using MaProgramez.Resources;

namespace MaProgramez.Website.ViewModels
{
    using MaProgramez.Website.Extensions;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;

    public class RoleViewModel
    {
        public string Id { get; set; }
        [Required(AllowEmptyStrings = false)]
        [Display(Name = "RoleName")]
        public string Name { get; set; }
    }

    public class EditUserViewModel
    {
        public string Id { get; set; }

        public IEnumerable<SelectListItem> RolesList { get; set; }

        [Required(ErrorMessage = null, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        [EmailAddress(ErrorMessageResourceType = typeof(Resource), ErrorMessage = null, ErrorMessageResourceName = "Email_ErrorMessage")]
        [Display(Name = @"Email")]
        public string Email { get; set; }

        //[Required(ErrorMessage = null, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        [StringLength(100, MinimumLength = 6, ErrorMessage = null, ErrorMessageResourceName = "User_Password_ErrorMessage", ErrorMessageResourceType = typeof(Resource))]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(Resource), Name = "User_Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(Resource), Name = "User_Confirm_Password")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = null, ErrorMessageResourceName = "User_Password_ConfirmErrorMessage", ErrorMessageResourceType = typeof(Resource))]
        public string ConfirmPassword { get; set; }

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

        [Display(ResourceType = typeof(Resource), Name = "User_VatRate")]
        [RequiredIf("IsCompany", true, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        public int VatRate { get; set; }

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

        [Display(ResourceType = typeof(Resource), Name = "Category")]
        public int SelectedCategoryId { get; set; }
        public Dictionary<int, string> Categories { get; set; }

        public IEnumerable<Address> Addresses { get; set; }
    }
}