using System.ComponentModel.DataAnnotations;

namespace MaProgramez.Repository.Entities
{
    public class Bank
    {
        [Key]
        public string Name { get; set; }

        public string Code { get; set; }
    }
}