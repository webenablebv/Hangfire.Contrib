using System;
using System.Linq;
using Hangfire.Common;
using Hangfire.Dashboard;
using Hangfire.States;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Webenable.Hangfire.Contrib.Internal
{
    public class HangfireTasksStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        {
            // Add a route to a custom Razor page
            var httpContextAccessor = app.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
            DashboardRoutes.Routes.AddRazorPage("/tasks", _ => new HangfireTasksPage(httpContextAccessor));

            // Add a route to handle the trigger command
            DashboardRoutes.Routes.AddCommand("/tasks/trigger/(?<TaskName>.+)/(?<Args>.+)", ctx =>
            {
                var taskName = ctx.UriMatch.Groups["TaskName"].Value;
                if (HangfireTaskRegister.Tasks.TryGetValue(taskName, out var task))
                {
                    // Extract the job arguments from the URL
                    var urlArgs = ctx.UriMatch.Groups["Args"].Value;
                    var args = urlArgs.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                    // Create a job instance and trigger it
                    var client = ctx.GetBackgroundJobClient();

                    if (task.Method.GetParameters().Length != args.Length)
                    {
                        // Append default values for remaining parameters
                        // For example, pass null for the PerformContext and JobCancellationToken
                        args = args.Concat(Enumerable.Repeat<string>(null, task.Method.GetParameters().Length - args.Length)).ToArray();
                    }

                    client.Create(new Job(task.Method, args), new EnqueuedState());
                    return true;
                }

                return false;
            });

            // Add a navigation item to the Hangfire dashboard menu
            NavigationMenu.Items.Add(page => new MenuItem("Tasks", page.Url.To("/tasks"))
            {
                Active = page.RequestPath.Equals("/tasks"),
                Metric = new DashboardMetric("Tasks", _ => new Metric(HangfireTaskRegister.Tasks.Count))
            });
            next(app);
        };
    }
}
