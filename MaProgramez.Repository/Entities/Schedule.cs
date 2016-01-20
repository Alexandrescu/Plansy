using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaProgramez.Repository.Entities
{
    public class Schedule
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(128)]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        public int SlotId { get; set; }

        [ForeignKey("SlotId")]
        public virtual Slot Slot { get; set; }

        public DateTime ScheduleDateTimeStart { get; set; }

        public DateTime ScheduleDateTimeEnd { get; set; }

        public DateTime CreatedDateTime { get; set; }

        [MaxLength(512)]
        public string Text { get; set; }

        public ScheduleState State { get; set; }

        public bool AddedByProvider { get; set; }

        public string ProviderView
        {
            get
            {
                if (this.User != null)
                {
                    return this.User.LastName + " " + this.User.FirstName + "<br/>" + this.User.PhoneNumber + "<br/>" + this.Text;
                }

                return string.Empty;
            }
        }

        public string ClientView
        {
            get
            {
                if (this.Slot != null && this.Slot.Provider != null)
                {
                    if (this.Slot.Provider.ProgrammingPerSlot)
                    {
                        return this.Slot.Provider.CompanyDisplayName +
                          " - " + this.Slot.Name + "<br/>" + this.Slot.Phone + "<br/>" + this.Text;
                    }

                    return this.Slot.Provider.CompanyDisplayName + "<br/>" + this.Slot.Phone + "<br/>" + this.Text;
                }

                return string.Empty;
            }
        }

        [NotMapped]
        public string Hour
        {
            get
            {
                return this.ScheduleDateTimeStart.ToString("HH");
            }
        }

        [NotMapped]
        public string Minutes
        {
            get
            {
                return this.ScheduleDateTimeStart.ToString("mm");
            }
        }

        public int DurationMinutes()
        {
            return Convert.ToInt32((this.ScheduleDateTimeEnd - this.ScheduleDateTimeStart).TotalMinutes);
        }

        [NotMapped]
        public List<Operation> OperationsForSchedule { get; set; }

        [NotMapped]
        public bool CanBeCanceled
        {
            get
            {
                return (State == ScheduleState.Pending || State == ScheduleState.Valid)
                    && ScheduleDateTimeStart > DateTime.Now;
            }
        }

        [NotMapped]
        public bool Canceled
        {
            get
            {
                return State == ScheduleState.CancelledByProvider || State == ScheduleState.CancelledByUser;
            }
        }
    }
}