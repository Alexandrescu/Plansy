using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaProgramez.Repository.Entities
{
    public class AutomaticProcessingJob
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(50)]
        public string JobName { get; set; }

        public int? MinutesInterval { get; set; }

        public int? RunAtHour { get; set; }

        public int? RunAtMinute { get; set; }

        public bool Enabled { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        #region Navigation properties

        public virtual List<AutomaticProcessingLog> Logs { get; set; }

        #endregion

    }
}
