using System;
using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;

namespace Webenable.Hangfire.Contrib
{
    /// <summary>
    /// Sets the expiration timeout of a job.
    /// </summary>
    public class JobExpirationAttribute : JobFilterAttribute, IApplyStateFilter
    {
        /// <summary>
        /// Sets the expiration timeout of a job.
        /// </summary>
        public JobExpirationAttribute()
        {
        }

        /// <summary>
        /// Gets or sets the expiration timeout duration in days.
        /// </summary>
        public int Days { get; set; }

        /// <inheritdoc />
        public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction) =>
            context.JobExpirationTimeout = TimeSpan.FromDays(Days);

        /// <inheritdoc />
        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction) =>
            context.JobExpirationTimeout = TimeSpan.FromDays(Days);
    }
}
