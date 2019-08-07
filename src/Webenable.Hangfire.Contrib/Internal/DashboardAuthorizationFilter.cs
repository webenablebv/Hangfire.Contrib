using System;
using System.Linq;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Webenable.Hangfire.Contrib.Internal
{
    public class DashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
#if NETSTANDARD2_0
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment;
#elif NETCOREAPP3_0
        private readonly IWebHostEnvironment _environment;
#endif
        private readonly string[] _allowedIps;
        private readonly ILogger<DashboardAuthorizationFilter> _logger;
        private readonly Func<HttpContext, bool> _authorizationCallback;

        public DashboardAuthorizationFilter(Func<HttpContext, bool> authorizationCallback, ILoggerFactory loggerFactory)
        {
            _authorizationCallback = authorizationCallback;
            _logger = loggerFactory.CreateLogger<DashboardAuthorizationFilter>();
        }
#if NETSTANDARD2_0
        public DashboardAuthorizationFilter(Microsoft.AspNetCore.Hosting.IHostingEnvironment environment, string[] allowedIps, ILoggerFactory loggerFactory)
        {
            _environment = environment;
            _allowedIps = allowedIps;
            _logger = loggerFactory.CreateLogger<DashboardAuthorizationFilter>();
        }
#elif NETCOREAPP3_0
        public DashboardAuthorizationFilter(IWebHostEnvironment environment, string[] allowedIps, ILoggerFactory loggerFactory)
        {
            _environment = environment;
            _allowedIps = allowedIps;
            _logger = loggerFactory.CreateLogger<DashboardAuthorizationFilter>();
        }
#endif

        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            // Invoke the custom specified authorization callback if specified.
            // Otherwise execute IP-based authorization.
            if (_authorizationCallback != null)
            {
                return InvokeAuthorizationCallback(httpContext);
            }

            return AuthorizeIpAddress(httpContext);
        }

        private bool InvokeAuthorizationCallback(HttpContext httpContext)
        {
            if (_authorizationCallback.Invoke(httpContext))
            {
                _logger.LogDebug("Grant access to dashboard");
                return true;
            }

            _logger.LogWarning("Deny access to dashboard");
            return false;
        }

        private bool AuthorizeIpAddress(HttpContext httpContext)
        {
            if (_environment.IsDevelopment())
            {
                // Always allow requests in tehe development environment.
                _logger.LogDebug("Grant access to dashboard in development environment");
                return true;
            }

            // Resolve remote IP addresses from the forwarded headers as well as the default header.
            var ips = httpContext
                .Request
                .Headers["X-Forwarded-For"]
                .ToString()
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(r => r.Trim())
                .Append(httpContext.Connection.RemoteIpAddress.ToString());

            foreach (var ip in ips)
            {
                if (_allowedIps.Contains(ip))
                {
                    _logger.LogDebug("Grant access to dashboard for IP-address {IpAddress}", ip);
                    return true;
                }
            }

            _logger.LogWarning("Deny access to dashboard for IP-addresses {IpAddresses}", string.Join(", ", ips));
            return false;
        }
    }
}
