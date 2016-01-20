using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaProgramez.Repository.Entities
{
    public class Address
    {
        [Key]
        public int Id { get; set; }

        public int CountryId { get; set; }

        [ForeignKey("CountryId")]
        [Display(ResourceType = typeof(Resources.Resource), Name = "User_Country")]
        public virtual Country UserCountry { get; set; }

        [Required(ErrorMessage = null, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resources.Resource))]
        [DataType(DataType.Text)]
        [Display(ResourceType = typeof(Resources.Resource), Name = "User_Address")]
        [MaxLength(500)]
        public string AddressText { get; set; }

        [Required(ErrorMessage = null, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resources.Resource))]
        [DataType(DataType.PostalCode)]
        [Display(ResourceType = typeof(Resources.Resource), Name = "User_PostalCode")]
        [MaxLength(50)]
        public string PostalCode { get; set; }

        public int CityId { get; set; }

        [ForeignKey("CityId")]
        [Display(ResourceType = typeof(Resources.Resource), Name = "User_City")]
        public virtual City UserCity { get; set; }

        [Required(ErrorMessage = null, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resources.Resource))]
        [MaxLength(128)]
        public string UserId { get; set; }

        [Required(ErrorMessage = null, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resources.Resource))]
        public AddressType AddressType { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public override string ToString()
        {
            return this.AddressText == null
                ? string.Empty
                : this.AddressText + ", " + this.UserCity.Name + ", Jud." +
                  this.UserCity.CityCounty.Name + ", ";
            //+this.UserCountry.Name;
        }

        public string ToLinkString()
        {
            return this.ToString().Replace(' ', '+');
        }
    }
}
