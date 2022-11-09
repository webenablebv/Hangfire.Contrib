using System;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using Webenable.Hangfire.Contrib.Internal;

namespace Webenable.Hangfire.Contrib;

/// <summary>
/// Extensions for logging in Hangfire jobs.
/// </summary>
public static class HangfireExtensions
{
    /// <summary>
    /// Begins a logical operation scope for the given <see cref="PerformContext"/> within the scope of a job.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/> instance.</param>
    /// <param name="performContext">The <see cref="PerformContext"/> instance for the job.</param>
    public static IDisposable BeginJobScope(this ILogger logger, PerformContext performContext) =>
        performContext != null ? logger.BeginScope(new PerformContextWrapper(performContext))! : NoopDisposable.Instance;

    private class NoopDisposable : IDisposable
    {
        public static NoopDisposable Instance = new();

        public void Dispose()
        {
        }
    }
}
