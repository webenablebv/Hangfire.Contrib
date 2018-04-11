using System;

namespace Webenable.Hangfire.Contrib
{
    /// <summary>
    /// Specifies that the job should be automatically scheduled in Hangfire
    /// using the specified <see cref="MethodName"/> and <see cref="CronExpression"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class AutoScheduleAttribute : Attribute
    {
        /// <summary>
        /// Specifies that the job should be automatically scheduled in Hangfire
        /// using the specified <paramref name="methodName"/> and <paramref name="cronExpression"/>.
        /// </summary>
        /// <param name="methodName">The name of the method to invoke the job with.</param>
        /// <param name="cronExpression">The CRON expression to schedule the job with.</param>
        public AutoScheduleAttribute(string methodName, string cronExpression)
        {
            MethodName = methodName;
            CronExpression = cronExpression;
        }

        /// <summary>
        /// Gets the name of the method to invoke the job with.
        /// </summary>
        public string MethodName { get; }

        /// <summary>
        /// Gets the CRON expression to schedule the job with.
        /// </summary>
        public string CronExpression { get; }
    }
}
