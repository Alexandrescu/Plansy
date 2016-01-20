using System.Collections.Generic;
using MaProgramez.Repository.Entities;

namespace MaProgramez.Repository.Models
{
    public class LocationViewModel
    {
        public List<County> Counties { get; set; }
        public List<City> Cities { get; set; }
    }
}