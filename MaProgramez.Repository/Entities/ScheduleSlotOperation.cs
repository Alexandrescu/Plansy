using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaProgramez.Repository.Entities
{
    public class ScheduleSlotOperation
    {
        [Key]
        public int Id { get; set; }

        public int ScheduleId { get; set; }

        [ForeignKey("ScheduleId")]
        public virtual Schedule Schedule { get; set; }

        public int SlotOperationId { get; set; }

        [ForeignKey("SlotOperationId")]
        public virtual SlotOperation SlotOperation { get; set; }
    }
}
