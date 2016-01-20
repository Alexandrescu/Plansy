using System;
using System.Collections.Generic;
using System.Linq;
using MaProgramez.Repository.Entities;

namespace MaProgramez.Repository.Utility
{
    public static class TimeHelpers
    {
        /// <summary>
        /// Gets the available minutes for interval.
        /// </summary>
        /// <param name="day">The day of the schedule.</param>
        /// <param name="startTime">The start time of the day - EX: (28.04.2015 08:00:00).TimeOfDay </param>
        /// <param name="endTime">The end time - EX: (28.04.2015 20:00:00).TimeOfDay </param>
        /// <param name="schedulesOfTheDay">The schedules of the day - list of Schedule entity for selected day</param>
        /// <param name="lowestOperationDuration">Duration of the shortest operation in minutes.</param>
        /// <param name="requestedOperationDuration">Duration of the requested operation in minutes.</param>
        /// <returns>A list of available times</returns>
        public static List<DateTime> GetAvailableMinutesForInterval(
            DateTime day,
            TimeSpan startTime, TimeSpan endTime,
            List<Schedule> schedulesOfTheDay,
            int lowestOperationDuration,
            int requestedOperationDuration
            )
        {
            var minutes = new List<DateTime>();

            var scheduledTimes = schedulesOfTheDay.Select(
                 sod =>
                     new Tuple<int, DateTime, DateTime>
                         (sod.SlotId, sod.ScheduleDateTimeStart, sod.ScheduleDateTimeStart.AddMinutes(sod.DurationMinutes())
                     )).ToList();

            for (var i = (int)startTime.TotalMinutes; i <= endTime.TotalMinutes - requestedOperationDuration; i += lowestOperationDuration)
            {
                if (scheduledTimes.Any(
                                       st => (  //inceputul programarii se suprapune cu o prog.existenta
                                                st.Item2 <= day.Date.AddMinutes(i) &&
                                                st.Item3 > day.Date.AddMinutes(i)
                                             )
                                             ||
                                             (  //sfarsitul programarii se suprapune cu o prog.existenta
                                                st.Item2 < day.Date.AddMinutes(i + requestedOperationDuration) &&
                                                st.Item3 >= day.Date.AddMinutes(i + requestedOperationDuration)
                                             )
                                       )
                    )
                {
                    continue;
                }

                var minuteToAdd = day.Date.AddMinutes(i);

                if (minuteToAdd < DateTime.Now.AddMinutes(15))
                {
                    continue;
                }
                
                minutes.Add(minuteToAdd);
            }

            return minutes;
        }

        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = dt.DayOfWeek - startOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }

            return dt.AddDays(-1 * diff).Date;
        }

        /// <summary>
        /// Determines whether the selected date is in a working week.
        /// Used for those who work every 2 weeks
        /// </summary>
        /// <param name="workingWeekStartDate">The working week start date.</param>
        /// <param name="selectedDate">The selected date.</param>
        /// <returns></returns>
        public static bool IsWorkingWeeek(DateTime workingWeekStartDate, DateTime selectedDate)
        {
            var weeksBetweenDates = selectedDate > workingWeekStartDate ?
                (selectedDate.StartOfWeek(DayOfWeek.Monday) - workingWeekStartDate.StartOfWeek(DayOfWeek.Monday)).Days / 7 :
                (workingWeekStartDate.StartOfWeek(DayOfWeek.Monday) - selectedDate.StartOfWeek(DayOfWeek.Monday)).Days / 7;

            return weeksBetweenDates % 2 == 0;
        }
    }
}
