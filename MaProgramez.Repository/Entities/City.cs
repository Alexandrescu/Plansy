using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaProgramez.Repository.Entities
{
    public class City
    {
        [Key]
        public int Id { get; set; }

        public int Siruta { get; set; }

        public decimal Longitude { get; set; }

        public decimal Latitude { get; set; }
        
        [MaxLength(64)]
        public string Name { get; set; }
        
        [MaxLength(64)]
        public string Region { get; set; }
        
        public int CountyId { get; set; }

        [ForeignKey("CountyId")]
        public virtual County CityCounty { get; set; }
    }
}