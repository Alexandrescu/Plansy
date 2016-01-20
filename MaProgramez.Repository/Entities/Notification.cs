using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaProgramez.Repository.Entities
{
    public class Notification
    {
        #region PROPERTIES

        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = null, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resources.Resource))]
        [MaxLength(128)]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
        
        [Required(ErrorMessage = null, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resources.Resource))]
        public NotificationType Type { get; set; }
        
        public DateTime CreatedDate { get; set; }

        [DefaultValue(false)]
        public bool IsRead { get; set; }

        [DefaultValue(false)]
        public bool IsDeleted { get; set; }

        [MaxLength(1000)]
        public string Text { get; set; }

        public int ScheduleId { get; set; }
        
        #endregion

        #region CONSTRUCTOR

        public Notification()
        {
            this.CreatedDate = DateTime.Now;
        }

        public Notification(string userId, NotificationType type, string text)
        {
            this.CreatedDate = DateTime.Now;
            this.UserId = userId;
            this.Type = type;
            this.Text = text;
        }
        
        #endregion
    }
}
