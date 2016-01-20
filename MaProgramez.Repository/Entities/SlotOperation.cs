using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaProgramez.Repository.Entities
{
    public class SlotOperation
    {
        [Key]
        public int Id { get; set; }

        public int SlotId { get; set; }

        [ForeignKey("SlotId")]
        public virtual Slot Slot { get; set; }

        public int OperationId { get; set; }

        [ForeignKey("OperationId")]
        public virtual Operation Operation { get; set; }

        public int DurationMinutes { get; set; }

        public decimal Price { get; set; }

        public SlotOperation()
        {
            
        }

        public SlotOperation(DefaultCategoryOperation defaultOperation)
        {
            this.OperationId = defaultOperation.OperationId;
            this.DurationMinutes = defaultOperation.DefaultDurationMinutes;
            this.Price = defaultOperation.DefaultPrice;
        }
    }
}