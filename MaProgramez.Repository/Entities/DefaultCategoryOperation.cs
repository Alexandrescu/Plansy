using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaProgramez.Repository.Entities
{
    public class DefaultCategoryOperation
    {
        [Key]
        public int Id { get; set; }

        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        public int OperationId { get; set; }

        [ForeignKey("OperationId")]
        public virtual Operation Operation { get; set; }

        public int DefaultDurationMinutes { get; set; }

        public decimal DefaultPrice { get; set; }
    }
}