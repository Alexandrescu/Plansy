using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MaProgramez.Repository.Entities.CustomAttributes;
using MaProgramez.Resources;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace MaProgramez.Repository.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserUniqueId { get; set; }

        [MaxLength(500)]
        [Display(ResourceType = typeof(Resource), Name = "User_Name")]
        public string LastName { get; set; }

        [MaxLength(500)]
        [Display(ResourceType = typeof(Resource), Name = "User_FirstName")]
        public string FirstName { get; set; }

        //[Display(ResourceType = typeof(Resource), Name = "User_Phone")]
        //public override string PhoneNumber { get; set; }

        public int ComplaintsNumber { get; set; }
        
        [Display(ResourceType = typeof(Resource), Name = "User_IsCompany")]
        public bool IsCompany { get; set; }

        [MaxLength(500)]
        [Display(ResourceType = typeof(Resource), Name = "User_CompanyName")]
        [RequiredIf("IsCompany", true, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        public string CompanyName { get; set; }

        [MaxLength(50)]
        [Display(ResourceType = typeof(Resource), Name = "User_CUI")]
        [RequiredIf("IsCompany", true, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        public string Cui { get; set; }

        [MaxLength(50)]
        [Display(ResourceType = typeof(Resource), Name = "User_JNO")]
        [RequiredIf("IsCompany", true, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        public string Jno { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "User_DateOfBirth")]
        public DateTime? DateOfBirth { get; set; }

        [MaxLength(100)]
        [Display(ResourceType = typeof(Resource), Name = "User_AccountNumber")]
        [RequiredIf("IsCompany", true, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        public string AccountNumber { get; set; }

        [MaxLength(100)]
        [Display(ResourceType = typeof(Resource), Name = "User_Bank")]
        [RequiredIf("IsCompany", true, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        public string Bank { get; set; }

        [MaxLength(50)]
        [Display(ResourceType = typeof(Resource), Name = "User_IdCardNo")]
        public string IdCardNo { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "User_VatRate")]
        [RequiredIf("IsCompany", true, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        public int VatRate { get; set; }

        [MaxLength(1000)]
        [Display(ResourceType = typeof(Resource), Name = "User_LogoPath")]
        public string LogoPath { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "User_AcceptsNotificationOnEmail")]
        public bool AcceptsNotificationOnEmail { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "User_AcceptsNotificationOnSms")]
        public bool AcceptsNotificationOnSms { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "User_AcceptsPushNotification")]
        public bool AcceptsPushNotifications { get; set; }

        [MaxLength(128)]
        public string AgentId { get; set; }

        [ForeignKey("AgentId")]
        public virtual ApplicationUser Agent { get; set; }

        public DateTime CreatedDate { get; set; }

        public int ModifiedByUserId { get; set; }

        [Required(ErrorMessage = null, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resource))]
        public bool AcceptedTermsFlag { get; set; }
        
        [Display(ResourceType = typeof(Resource), Name = "ContractNo")]
        public int? ContractNo { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "ContractDate")]
        public DateTime? ContractDate { get; set; }

        public DateTime? EndOfSubscription { get; set; }

        public decimal Commision { get; set; }

        public bool ProgrammingPerSlot { get; set; }

        public bool IsAnonymous { get; set; }
        /// <summary>
        /// This will be displayed on the screen - full description of the company / person
        /// </summary>
        public string FullDescription { get; set; }

        /// <summary>
        /// This alias will be displayed on the screen instead of the Company Name
        /// </summary>
        public string Alias { get; set; }

        #region Navigation properties

        public virtual List<Address> Addresses { get; set; }

        #endregion

        
        #region Helper Methods

        public Address GetWorkLocationAddress()
        {
            if (Addresses == null || !Addresses.Any())
            {
                return null;
            }

            return this.Addresses.Count > 1 ? Addresses.First(a => a.AddressType == AddressType.PlaceOfBusinessAddress) : Addresses.First();
        }

        #endregion

        /// <summary>
        /// Gets the display name of the company - Alias if any or the company name if no alias is defined.
        /// </summary>
        [NotMapped]
        public string CompanyDisplayName {
            get { return string.IsNullOrWhiteSpace(Alias) ? CompanyName : Alias; } }
    }
}