using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MaProgramez.Repository.Entities;

namespace MaProgramez.Website.ViewModels
{
    public class SlotOperationViewModel
    {
        public int Id { get; set; }

        public int SlotId { get; set; }

        public int DurationMinutes { get; set; }
        
        public int OperationId { get; set; }

        public decimal Price { get; set; }

        public string Description { get; set; }

        public int CategoryId { get; set; }

        public string CategoryName { get; set; }

        public List<Operation> Operations { get; set; } 

        public SlotOperationViewModel()
        {
            Operations = new List<Operation>();
        }

        public SlotOperationViewModel(SlotOperation slotOperation)
        {
            this.Id = slotOperation.Id;
            this.SlotId = slotOperation.SlotId;
            this.DurationMinutes = slotOperation.DurationMinutes;
            this.OperationId = slotOperation.OperationId;
            this.Price = slotOperation.Price;
            this.Description = slotOperation.Operation.Description;
            this.CategoryId = slotOperation.Operation.CategoryId;
            this.CategoryName = slotOperation.Operation.Category.Name;
            Operations = new List<Operation>();
        }

        public SlotOperationViewModel(DefaultCategoryOperation defaultOperation)
        {
            this.Id = defaultOperation.Id;
            this.DurationMinutes = defaultOperation.DefaultDurationMinutes;
            this.Price = defaultOperation.DefaultPrice;
            this.Description = defaultOperation.Operation.Description;
            this.CategoryId = defaultOperation.Operation.CategoryId;
            this.CategoryName = defaultOperation.Operation.Category.Name;
            Operations = new List<Operation>();
        }
    }
}