using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaProgramez.Repository.Entities
{
    public class Rating
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

        public int? SlotId { get; set; }

        [ForeignKey("SlotId")]
        public virtual Slot Slot { get; set; }

        public int Score { get; set; }

        [MaxLength(256)]
        public string ShortDescription { get; set; }
    }
}
