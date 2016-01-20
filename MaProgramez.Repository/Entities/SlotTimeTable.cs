using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaProgramez.Repository.Entities
{
    public class SlotTimeTable
    {
        [Key]
        public int Id { get; set; }

        public int SlotId { get; set; }

        [ForeignKey("SlotId")]
        public virtual Slot Slot { get; set; }

        public Day DayOfWeek { get; set; }

        public DateTime StarTime { get; set; }

        public DateTime EndTime { get; set; }

        public DateTime? WorkingWeekStartDate { get; set; }
    }
}