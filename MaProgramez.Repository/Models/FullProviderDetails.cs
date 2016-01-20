using System.Collections.Generic;
using MaProgramez.Repository.Entities;

namespace MaProgramez.Repository.Models
{
    public class FullProviderDetails
    {
        #region PROPERTIES

        public ApplicationUser Provider { get; set; }

        public List<Operation> ProviderOperations { get; set; }

        public List<Category> ProviderCategories { get; set; }

        public List<Slot> ProviderSlots { get; set; }

        #endregion PROPERTIES
    }
}