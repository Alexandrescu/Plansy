using MaProgramez.Resources;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaProgramez.Repository.Entities
{
    public class SlotNonWorkingDay
    {
        [Key]
        public int Id { get; set; }

        public DateTime StartDateTime { get; set; }

        public DateTime EndDateTime { get; set; }

        [MaxLength(300)]
        public string Description { get; set; }

        public int SlotId { get; set; }

        [ForeignKey("SlotId")]
        public virtual Slot Slot { get; set; }

        public bool IsWorkingDay { get; set; }
    }
}