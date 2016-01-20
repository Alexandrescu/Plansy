using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using MaProgramez.Repository.DbContexts;
using MaProgramez.Repository.Entities;
using MaProgramez.Website.AutomaticProcessingService.Jobs;

namespace MaProgramez.Website.AutomaticProcessingService
{
    public class AutomaticProcessingService
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomaticProcessingService" /> class.
        /// </summary>
        public AutomaticProcessingService()
        {
            using (var db = new AppDbContext())
            {
                var jobs = from j in db.AutomaticProcessingJobs
                           where j.Enabled
                                 && j.StartDate <= DateTime.Today
                                 && j.EndDate >= DateTime.Today
                           select j;

                foreach (var job in jobs)
                {
                    this.GetJob(job);
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the job.
        /// </summary>
        /// <param name="job">The job.</param>
        /// <returns></returns>
        public BaseJob GetJob(AutomaticProcessingJob job)
        {
            var instance = (from t in Assembly.GetExecutingAssembly().GetTypes()
                            where t.BaseType == typeof(BaseJob)
                                  && t.Name == job.JobName
                            select Activator.CreateInstance(t, job)).FirstOrDefault();

            return instance as BaseJob;
        }

        #endregion

    }
}