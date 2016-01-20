using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using MaProgramez.Repository.BusinessLogic;

namespace MaProgramez.Repository.Entities
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        //WebApp
        [MaxLength(200)]
        public string CssClass { get; set; }

        // WebSite
        [MaxLength(200)]
        public string CssIcon { get; set; }

        public int? ParentCategoryId { get; set; }

        [ForeignKey("ParentCategoryId")]
        public virtual Category ParentCategory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this category is leaf (has no child categories).
        /// </summary>
        /// <value>
        ///   <c>true</c> if this category is leaf; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsLeaf { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this category is empty (contains no providers).
        /// </summary>
        /// <value>
        ///   <c>true</c> if this category is empty; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsPending { get; set; }

        public virtual List<Slot> Slots { get; set; }

        /// <summary>
        /// Gets all descendant categories for this category.
        /// </summary>
        /// <value>
        /// The descendants.
        /// </value>
        public List<Category> GetDescendants()
        {
            return this.DescendantCategories().Where(d => d.Slots != null && d.Slots.Count > 0).ToList();
               
        }
    }
}