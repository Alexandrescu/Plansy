using System;
using System.ComponentModel.DataAnnotations;

namespace MaProgramez.Repository.Entities
{
    public class Email
    {
        [Key]
        public int Id { get; set; }

        public string Destination { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public DateTime SendingDateTime { get; set; }
    }
}