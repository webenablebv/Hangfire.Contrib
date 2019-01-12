using System.Threading.Tasks;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Webenable.Hangfire.Contrib.TestApp
{
    public class FooTask
    {
        public Task ExecuteAsync(int fooId) => Task.CompletedTask;
    }

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHangfireContrib(cfg => cfg.UseMemoryStorage())
                    .AddHangfireTasksPage();
            HangfireTaskRegister.AddTask<FooTask>(x => x.ExecuteAsync(default));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
}
