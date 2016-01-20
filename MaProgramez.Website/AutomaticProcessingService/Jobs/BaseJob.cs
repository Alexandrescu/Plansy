using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using MaProgramez.Repository.DbContexts;
using MaProgramez.Repository.Entities;

namespace MaProgramez.Website.AutomaticProcessingService.Jobs
{
    public class BaseJob
    {
        #region Local declarations

        /// <summary>
        /// The timer
        /// </summary>
        private readonly Timer _timer;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the job.
        /// </summary>
        /// <value>
        /// The job.
        /// </value>
        private AutomaticProcessingJob Job { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseJob" /> class.
        /// </summary>
        public BaseJob(AutomaticProcessingJob job)
        {
            this.Job = job;
            this._timer = new Timer(Run);

            if (this.Job.MinutesInterval.HasValue && this.Job.MinutesInterval.Value > 0)
            {
                this.SetTimer(this.Job.MinutesInterval.Value);
            }
            else if (this.Job.RunAtHour.HasValue && this.Job.RunAtMinute.HasValue)
            {
                this.SetTimer(this.Job.RunAtHour.Value, this.Job.RunAtMinute.Value);
            }

        }

        #endregion

        #region Methods

        /// <summary>
        /// Sets the timer at specific hour and minute of the day.
        /// </summary>
        /// <param name="hour">The hour of the day.</param>
        /// <param name="minute">The minute of the day.</param>
        protected void SetTimer(int hour, int minute)
        {
            // Figure how much time until 16:00
            var now = DateTime.Now;
            var runTime = DateTime.Today.AddHours(hour).AddMinutes(minute);

            // If it's already past 16:00, wait until 16:00 tomorrow    
            if (now > runTime)
            {
                runTime = runTime.AddDays(1.0);
            }

            var msUntilRunTime = (int) ((runTime - now).TotalMilliseconds);

            // Set the timer to elapse only once, at 16:00.
            _timer.Change(msUntilRunTime, Timeout.Infinite);
        }

        /// <summary>
        /// Sets the timer.
        /// </summary>
        /// <param name="min">The minimum.</param>
        protected void SetTimer(int min)
        {
            var now = DateTime.Now;
            var runTime = now.AddMinutes(min);

            var msUntilRunTime = (int) ((runTime - now).TotalMilliseconds);
            _timer.Change(msUntilRunTime, Timeout.Infinite);
        }

        /// <summary>
        ///  Runs this instance.
        ///  </summary>
        public async void Run(object state)
        {
            var param = string.Empty;
            await Task.Run(() => Execute(param));

            if (this.Job.MinutesInterval.HasValue && this.Job.MinutesInterval.Value > 0)
            {
                this.SetTimer(this.Job.MinutesInterval.Value);
            }
            else if (this.Job.RunAtHour.HasValue && this.Job.RunAtMinute.HasValue)
            {
                this.SetTimer(this.Job.RunAtHour.Value, this.Job.RunAtMinute.Value);
            }
        }

        /// <summary>
        /// Logs the job run.
        /// </summary>
        /// <param name="result">if set to <c>true</c> [result].</param>
        /// <param name="message">The message.</param>
        protected void LogJobRun(bool result, string message)
        {
            using (var db = new AppDbContext())
            {
                var logEntry = new AutomaticProcessingLog
                {
                    AutomaticProcessingJobId = Job.Id,
                    RunTime = DateTime.Now,
                    Result = result,
                    Message = message.Length > 2000 ? message.Substring(0, 1999) : message
                };

                db.AutomaticProcessingLogs.Add(logEntry);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Logs the job run.
        /// </summary>
        /// <param name="result">if set to <c>true</c> [result].</param>
        protected void LogJobRun(bool result)
        {
            this.LogJobRun(result, string.Empty);
        }


        #endregion

        #region Virtual methods

        /// <summary>
        /// Override this method in child classes and add Job logic
        /// </summary>
        /// <param name="param">The parameter.</param>
        protected virtual void Execute(string param)
        {

        }

        #endregion
    }
}