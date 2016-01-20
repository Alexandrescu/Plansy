using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaProgramez.Repository.Entities
{
    public class Favorite
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(128)]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [MaxLength(128)]
        public string FavoriteUserId { get; set; }

        [ForeignKey("FavoriteUserId")]
        public virtual ApplicationUser FavoriteUser { get; set; }

        public int? FavoriteSlotId { get; set; }

        [ForeignKey("FavoriteSlotId")]
        public virtual Slot FavoriteSlot { get; set; }
    }
}