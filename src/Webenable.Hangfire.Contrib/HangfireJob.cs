using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;

namespace Webenable.Hangfire.Contrib;

/// <summary>
/// Base class for Hangfire jobs which provides auto-scheduling and logging.
/// </summary>
public abstract class HangfireJob
{
    /// <summary>
    /// Initializes the <see cref="HangfireJob"/> base class with the specified <see cref="ILoggerFactory"/>.
    /// </summary>
    /// <param name="loggerFactory">The logger factory to use for logging.</param>
    protected HangfireJob(ILoggerFactory loggerFactory)
    {
        Logger = loggerFactory.CreateLogger(GetType());
    }

    /// <summary>
    /// Gets the <see cref="ILogger"/> instance for this job.
    /// </summary>
    protected ILogger Logger { get; }

    /// <summary>
    /// Gets the perform context instance of this job.
    /// May be null.
    /// </summary>
    protected PerformContext? PerformContext { get; private set; }

    /// <summary>
    /// Executes the job.
    /// </summary>
    /// <param name="performContext">
    /// The context in which the job is performed. Populated by Hangfire.
    /// It is safe to pass <c>null</c> in unit tests or other manual scenarios.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="IJobCancellationToken"/>. Populated by Hangfire.
    /// It is safe to pass <c>null</c> in unit tests or other manual scenarios,
    /// a new instance of <see cref="JobCancellationToken"/> will be created in that case.
    /// </param>
    public async Task ExecuteAsync(PerformContext performContext, IJobCancellationToken cancellationToken)
    {
        PerformContext = performContext;
        var jobId = performContext?.BackgroundJob?.Id;

        cancellationToken?.ThrowIfCancellationRequested();

        IDisposable? jobScope = null;
        IDisposable? performContextScope = null;

        // Perform context is optional, e.g. may be null in unit tests
        if (performContext != null)
        {
            if (!string.IsNullOrEmpty(jobId))
            {
                jobScope = Logger.BeginScope("Job {JobId}", jobId);
            }

            performContextScope = Logger.BeginJobScope(performContext);
        }

        Logger.LogDebug("Starting job {JobId}", jobId);
        try
        {
            await ExecuteCoreAsync(cancellationToken ?? new JobCancellationToken(false));
            Logger.LogDebug("Finished job {JobId}", jobId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed executing job {JobId}/{JobName}: {JobException}", jobId, GetType().Name, ex.ToStringDemystified());
            throw;
        }

        performContextScope?.Dispose();
        jobScope?.Dispose();
    }

    /// <summary>
    /// Executes the inner logic of the job.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="IJobCancellationToken"/>.</param>
    protected abstract Task ExecuteCoreAsync(IJobCancellationToken cancellationToken);

    /// <summary>
    /// Gets the schedule for automatically scheduled jobs.
    /// Default is null which means that the job is not automatically scheduled.
    /// </summary>
    public virtual string? Schedule => null;
}
