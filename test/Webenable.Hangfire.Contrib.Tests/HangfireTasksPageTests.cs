using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Webenable.Hangfire.Contrib.Tests
{
    public class HangfireTasksPageTests : IClassFixture<WebApplicationFactory<TestApp.Startup>>
    {
        private readonly WebApplicationFactory<TestApp.Startup> _factory;

        public HangfireTasksPageTests(WebApplicationFactory<TestApp.Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task RendersTaskPage()
        {
            // Make sure the client is created, otherwise SendAync will null-ref
            _ = _factory.CreateClient();

            // Use SendAsync which allows us to configure the HttpContext
            // so we can set the Remote IP address which the default Hangfire
            // dashboard authorization uses to only allow local connections
            var response = await _factory.Server.SendAsync(c =>
            {
                c.Request.Path = "/hangfire/tasks";
                c.Connection.RemoteIpAddress = IPAddress.Loopback;
            });

            Assert.Equal(StatusCodes.Status200OK, response.Response.StatusCode);
        }
    }
}
