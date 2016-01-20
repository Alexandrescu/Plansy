using System.ComponentModel.DataAnnotations;

namespace MaProgramez.Repository.Entities
{
    public class Country
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(2)]
        public string Code { get; set; }

        [MaxLength(30)]
        public string Name { get; set; }
    }
}