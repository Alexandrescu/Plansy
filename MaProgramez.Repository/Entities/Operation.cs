using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaProgramez.Repository.Entities
{
    public class Operation
    {
        #region Public Constructors

        public Operation()
        {
        }

        public Operation(Operation sourceOperation)
        {
            this.Id = sourceOperation.Id;
            this.Description = sourceOperation.Description;
            this.CategoryId = sourceOperation.CategoryId;
            this.Category = sourceOperation.Category;
            this.Selected = sourceOperation.Selected;
            this.Price = sourceOperation.Price;
            this.DurationMinutes = sourceOperation.DurationMinutes;
        }

        #endregion Public Constructors

        #region Public Properties

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        public int CategoryId { get; set; }

        [MaxLength(100)]
        public string Description { get; set; }

        [NotMapped]
        public int DurationMinutes { get; set; }

        [Key]
        public int Id { get; set; }

        [NotMapped]
        public decimal Price { get; set; }

        [NotMapped]
        public bool Selected { get; set; }

        #endregion Public Properties
    }
}