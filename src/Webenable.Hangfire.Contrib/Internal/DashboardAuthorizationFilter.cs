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
        private readonly IWebHostEnvironment? _environment;
        private readonly string[]? _allowedIps;
        private readonly ILogger<DashboardAuthorizationFilter> _logger;
        private readonly Func<HttpContext, bool>? _authorizationCallback;

        public DashboardAuthorizationFilter(Func<HttpContext, bool> authorizationCallback, ILoggerFactory loggerFactory)
        {
            _authorizationCallback = authorizationCallback;
            _logger = loggerFactory.CreateLogger<DashboardAuthorizationFilter>();
        }

        public DashboardAuthorizationFilter(IWebHostEnvironment environment, string[] allowedIps, ILoggerFactory loggerFactory)
        {
            _environment = environment;
            _allowedIps = allowedIps;
            _logger = loggerFactory.CreateLogger<DashboardAuthorizationFilter>();
        }

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
            if (_authorizationCallback?.Invoke(httpContext) == true)
            {
                _logger.LogDebug("Grant access to Hangfire dashboard");
                return true;
            }

            _logger.LogWarning("Deny access to Hangfire dashboard");
            return false;
        }

        private bool AuthorizeIpAddress(HttpContext httpContext)
        {
            if (_environment?.IsDevelopment() == true)
            {
                // Always allow requests in development environment.
                _logger.LogDebug("Grant access to Hangfire dashboard in development environment");
                return true;
            }

            if (_allowedIps == null || _allowedIps.Length == 0)
            {
                _logger.LogWarning("Deny access to dashboard: no allowed IP addresses configured");
                return false;
            }

            // Resolve remote IP addresses from the forwarded headers as well as the default header.
            var ips = httpContext
                .Request
                .Headers["X-Forwarded-For"]
                .ToString()
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Append(httpContext.Connection.RemoteIpAddress?.ToString())
                .Select(r => r?.Trim())
                .Where(x => !string.IsNullOrEmpty(x));

            foreach (var ip in ips)
            {
                if (_allowedIps.Contains(ip))
                {
                    _logger.LogDebug("Grant access to Hangfire dashboard for IP-address {IpAddress}", ip);
                    return true;
                }
            }

            _logger.LogWarning("Deny access to Hangfire dashboard for IP-addresses {IpAddresses}", string.Join(", ", ips));
            return false;
        }
    }
}
