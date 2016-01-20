using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaProgramez.Repository.Entities
{
    public class AutomaticProcessingLog
    {
        [Key]
        public int Id { get; set; }

        public DateTime RunTime { get; set; }

        public bool Result { get; set; }

        public int AutomaticProcessingJobId { get; set; }

        [MaxLength(2000)]
        public string Message { get; set; }

        [ForeignKey("AutomaticProcessingJobId")]
        public virtual AutomaticProcessingJob Job { get; set; }
    }
}
