using MaProgramez.Resources;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaProgramez.Repository.Entities
{
    public class Slot
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(128)]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [MaxLength(128)]
        public string ProviderId { get; set; }

        [ForeignKey("ProviderId")]
        public virtual ApplicationUser Provider { get; set; }

        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        [MaxLength(50)]
        public string Email { get; set; }

        [MaxLength(15)]
        public string Phone { get; set; }

        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "User_AcceptsNotificationOnEmail")]
        public bool AcceptsNotificationOnEmail { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "User_AcceptsNotificationOnSms")]
        public bool AcceptsNotificationOnSms { get; set; }

        [MaxLength(1000)]
        [Display(ResourceType = typeof(Resource), Name = "User_LogoPath")]
        public string LogoPath { get; set; }

        [NotMapped]
        public string FullAddress { get; set; }

        #region Navigation Properties

        public virtual List<SlotOperation> SlotOperations { get; set; }

        public virtual List<SlotNonWorkingDay> SlotNonWorkingDays { get; set; }

        public virtual List<SlotTimeTable> SlotTimeTables { get; set; }

        #endregion

        #region Constructors

        public Slot()
        {
        }

        public Slot(ApplicationUser provider)
        {
            ProviderId = provider.Id;
            AcceptsNotificationOnSms = provider.AcceptsNotificationOnSms;
            AcceptsNotificationOnEmail = provider.AcceptsNotificationOnEmail;
        }

        #endregion
    }
}