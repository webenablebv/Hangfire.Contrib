using System.IO;
using System.Text;
using Hangfire.Dashboard.Pages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;

namespace Webenable.Hangfire.Contrib.Internal
{
    internal class HangfireTasksPage : global::Hangfire.Dashboard.RazorPage
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HangfireTasksPage(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override void Execute()
        {
            Layout = new LayoutPage("Tasks");

            // Resolve HttpContext and the service provider
            var httpContext = _httpContextAccessor.HttpContext;
            var sp = httpContext.RequestServices;

            // Find and create the Razor page instance
            var pageFactoryProvider = sp.GetRequiredService<IRazorPageFactoryProvider>();
            var pageActivator = sp.GetRequiredService<IRazorPageActivator>();
            var factoryResult = pageFactoryProvider.CreateFactory(ViewEnginePath.ResolvePath("~/Internal/HangfireTasks.cshtml"));
            var page = factoryResult.RazorPageFactory.Invoke();

            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                // Create a view context with the in-memory string writer and assign it to the page
                var viewContext = new ViewContext
                {
                    HttpContext = httpContext,
                    Writer = writer
                };
                page.ViewContext = viewContext;

                // Actives the necessary properties on the Razor page
                pageActivator.Activate(page, viewContext);

                // Render the page (synchronously...)
                page.ExecuteAsync().GetAwaiter().GetResult();
            }

            // Write the HTML output to the Razor page of Hangfire
            WriteLiteral(sb.ToString());
        }
    }
}
