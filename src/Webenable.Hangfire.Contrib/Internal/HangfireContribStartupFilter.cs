using System;
using System.Reflection;
using Hangfire;
using Hangfire.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Webenable.Hangfire.Contrib.Internal
{
    public class HangfireContribStartupFilter : IStartupFilter
    {
        private readonly HangfireContribOptions _contribOptions;
        private readonly BackgroundJobServerOptions _backgroundJobServerOptions;
        private readonly DashboardOptions _dashboardOptions;
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly ILogger<HangfireContribStartupFilter> _logger;

        public HangfireContribStartupFilter(
            IOptions<HangfireContribOptions> options,
            IOptions<BackgroundJobServerOptions> backgroundJobServerOptions,
            IOptions<DashboardOptions> dashboardOptions,
            IRecurringJobManager recurringJobManager,
            ILogger<HangfireContribStartupFilter> logger)
        {
            _contribOptions = options.Value;
            _backgroundJobServerOptions = backgroundJobServerOptions?.Value;
            _dashboardOptions = dashboardOptions?.Value;
            _recurringJobManager = recurringJobManager;
            _logger = logger;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) =>
            app =>
            {
                next(app);

                if (_contribOptions.EnableServer)
                {
                    _logger.LogInformation("Enabling Hangfire server");
                    app.UseHangfireServer(_backgroundJobServerOptions);
                }

                if (_contribOptions.Dasbhoard.Enabled)
                {
                    ConfigureDashboard(app);
                }

                RegisterJobs(app);
            };

        private void ConfigureDashboard(IApplicationBuilder app)
        {
            _logger.LogInformation("Enabling Hangfire dashboard");
            var dashboardOptions = _dashboardOptions ?? new DashboardOptions();
            if (_contribOptions.Dasbhoard.EnableAuthorization)
            {
                var loggerFactory = app.ApplicationServices.GetRequiredService<ILoggerFactory>();

                DashboardAuthorizationFilter dashboardAuthorizationFilter;
                if (_contribOptions.Dasbhoard.AuthorizationCallback != null)
                {
                    _logger.LogInformation("Configuring Hangfire dashboard authorization with custom callback");
                    dashboardAuthorizationFilter = new DashboardAuthorizationFilter(_contribOptions.Dasbhoard.AuthorizationCallback, loggerFactory);
                }
                else if (_contribOptions.Dasbhoard.AllowedIps?.Length > 0)
                {
                    _logger.LogInformation("Configuring Hangfire IP-based dashboard authorization");
                    dashboardAuthorizationFilter = new DashboardAuthorizationFilter(app.ApplicationServices.GetRequiredService<IHostingEnvironment>(), _contribOptions.Dasbhoard.AllowedIps, loggerFactory);
                }
                else
                {
                    throw new InvalidOperationException("No custom authorization callback or allowed IP-addresses configured for Hangfire dashboard authorization.");
                }

                dashboardOptions.Authorization = new[] { dashboardAuthorizationFilter };
            }

            app.UseHangfireDashboard(options: dashboardOptions);
        }

        private static MethodInfo _executeMethod = typeof(HangfireJob).GetMethod(nameof(HangfireJob.ExecuteAsync));

        private void RegisterJobs(IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var sp = scope.ServiceProvider;
                var hangfireJobType = typeof(HangfireJob);
                foreach (var assembly in _contribOptions.ScanningAssemblies)
                {
                    foreach (var candidate in assembly.ExportedTypes)
                    {
                        if (hangfireJobType.IsAssignableFrom(candidate) && candidate != hangfireJobType)
                        {
                            try
                            {
                                var jobInstance = (HangfireJob)ActivatorUtilities.CreateInstance(sp, candidate);
                                if (!string.IsNullOrEmpty(jobInstance.Schedule))
                                {
                                    _logger.LogInformation("Auto-scheduling job {JobName} with schedule {JobSchedule}", candidate.Name, jobInstance.Schedule);
                                    _recurringJobManager.AddOrUpdate(candidate.Name, new Job(candidate, _executeMethod, null, null), jobInstance.Schedule);
                                }
                                else
                                {
                                    _logger.LogDebug("Job {JobName} auto-scheduling is disabled", candidate.Name);
                                }
                            }
                            catch (Exception ex)
                            {
                                throw new InvalidOperationException($"Unable to activate job {hangfireJobType.Name}. Probably due to missing dependencies. See inner exception for more details.", ex);
                            }
                        }
                        else
                        {
                            var scheduleAttr = candidate.GetCustomAttribute<AutoScheduleAttribute>();
                            if (scheduleAttr != null)
                            {
                                _logger.LogInformation("Auto-scheduling job {JobName} via [AutoScheduled] attribute with schedule {JobSchedule}", candidate.Name, scheduleAttr.CronExpression);
                                _recurringJobManager.AddOrUpdate(candidate.Name, new Job(candidate, candidate.GetMethod(scheduleAttr.MethodName)), scheduleAttr.CronExpression);
                            }
                        }
                    }
                }
            }
        }
    }
}
