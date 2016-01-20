using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MaProgramez.Repository.DbContexts;
using MaProgramez.Repository.Entities;
using MaProgramez.Repository.Models;
using MaProgramez.Repository.Utility;

namespace MaProgramez.Repository.BusinessLogic
{
    public static class ScheduleBusinessLogic
    {
        /// <summary>
        /// Saves the new apointment.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>
        ///     0 = Schedule saved; 
        ///     1 = Overlapping schedule; 
        ///     2 = More than one appointment at the same provider in the same day; 
        ///     3 = No operations selected
        /// </returns>
        public static int SaveNewApointment(ScheduleParameters model)
        {
            var operations = RetrieveLists.GetSlotOperationsByIds(model.SelectedOperationIds, model.SlotId);
            var slot = RetrieveOthers.GetSlotById(model.SlotId);

            if (model.SelectedOperationIds.Any())
            {
                var s = new Schedule()
                {
                    CreatedDateTime = DateTime.Now,
                    ScheduleDateTimeStart = model.AppointmenDateTime,
                    SlotId = model.SlotId,
                    State = ScheduleState.Pending,
                    UserId = model.UserId,
                    Text = model.AppointmentText
                };
                s.ScheduleDateTimeEnd = s.ScheduleDateTimeStart.AddMinutes(operations.Sum(x => x.DurationMinutes));

                var errorCode = RetrieveOthers.IsSchedulePossible(s);
                if (errorCode == 0)
                {
                    using (var db = new AppDbContext())
                    {
                        db.Schedules.Add(s);

                        foreach (var slotOperation in operations)
                        {
                            db.ScheduleSlotOperations.Add(new ScheduleSlotOperation()
                            {
                                ScheduleId = s.Id,
                                SlotOperationId = slotOperation.Id
                            });
                        }

                        db.SaveChanges();

                        var client = RetrieveOthers.GetUserById(model.UserId);

                        var n = RetrieveOthers.AddNotification(s.Id, slot.ProviderId, NotificationType.NewSchedule,
                            slot.Provider.ProgrammingPerSlot
                                ? "A new schedule to " + slot.Name + " was made on " +
                                  s.ScheduleDateTimeStart.ToString("dd-MM-yyyy") + ", at " +
                                  s.ScheduleDateTimeStart.ToString("HH:mm") + ". Client: " + client.FirstName + " " +
                                  client.LastName + " " + client.PhoneNumber +
                                  ". Plansy.nl"
                                : "A new schedule was made on " +
                                  s.ScheduleDateTimeStart.ToString("dd-MM-yyyy") +
                                  ", at " + s.ScheduleDateTimeStart.ToString("HH:mm") + ". Client: " +
                                  client.FirstName + " " + client.LastName + " " + client.PhoneNumber +
                                  ". Plansy.nl");

                    
                        if (slot.Provider.PhoneNumber != null && slot.Provider.AcceptsNotificationOnSms &&
                            slot.Provider.PhoneNumberConfirmed)
                        {
                            MailAndSmsUtility.SendSms(slot.Provider.PhoneNumber, n.Text);
                        }

                        if (!string.IsNullOrWhiteSpace(slot.UserId))
                        {
                            var nn = RetrieveOthers.AddNotification(s.Id, slot.UserId, NotificationType.NewSchedule,
                                "A new schedule was made on " +
                                s.ScheduleDateTimeStart.ToString("dd-MM-yyyy") +
                                ", at " + s.ScheduleDateTimeStart.ToString("HH:mm") +
                                ". Client: " + client.FirstName + " " + client.LastName + " " + client.PhoneNumber +
                                ". Plansy.nl");

                            if (slot.User.PhoneNumber != null && slot.User.AcceptsNotificationOnSms &&
                                slot.User.PhoneNumberConfirmed)
                            {
                                MailAndSmsUtility.SendSms(slot.User.PhoneNumber, nn.Text);
                            }
                        }
                    }
                }
                return errorCode;
            }

            return 3; //no operations selected
        }
    }
}
