using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Webenable.Hangfire.Contrib.Internal;

namespace Webenable.Hangfire.Contrib
{
    public static class TasksPageServiceCollectionExtensions
    {
        public static IServiceCollection AddHangfireTasksPage(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddMvcCore()
                .AddRazorViewEngine();

            services.AddSingleton<IStartupFilter, HangfireTasksStartupFilter>();

            return services;
        }
    }
}
