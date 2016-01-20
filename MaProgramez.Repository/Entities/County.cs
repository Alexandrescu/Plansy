using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MaProgramez.Repository.Entities
{
    public class County
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(2)]
        public string Code { get; set; }

        [MaxLength(20)]
        public string Name { get; set; }

        #region Navigation Properties

        public virtual List<City> Cities { get; set; }

        #endregion Navigation Properties
    }
}