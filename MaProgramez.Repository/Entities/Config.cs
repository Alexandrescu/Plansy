using System.ComponentModel.DataAnnotations;

namespace MaProgramez.Repository.Entities
{
    public class Config
    {
        [MaxLength(100)]
        [Key]
        public string Name { get; set; }

        [MaxLength(100)]
        public string Value { get; set; }
    }
}