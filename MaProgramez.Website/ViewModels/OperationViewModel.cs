using MaProgramez.Repository.Entities;

namespace MaProgramez.Website.ViewModels
{
    public class OperationViewModel
    {
        public int Id { get; set; }

        public int SlotId { get; set; }
        
        public string Description { get; set; }

        public int CategoryId { get; set; }

        public string CategoryName { get; set; }

        public OperationViewModel()
        {
            
        }

        public OperationViewModel(Operation operation)
        {
            this.Id = operation.Id;
            this.Description = operation.Description;
            this.CategoryId = operation.CategoryId;
            this.CategoryName = operation.Category.Name;
        }
    }
}