using System;
using System.ComponentModel.DataAnnotations;

namespace MaProgramez.Repository.Entities
{
    public class DefaultNonWorkingDay
    {
        [Key]
        public int Id { get; set; }

        public DateTime StartDateTime { get; set; }

        public DateTime EndDateTime { get; set; }

        [MaxLength(300)]
        public string Description { get; set; }
    }
}