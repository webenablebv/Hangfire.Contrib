using System;
using System.Reflection;
using Microsoft.AspNetCore.Http;

namespace Webenable.Hangfire.Contrib
{
    /// <summary>
    /// Defines options for the Hangfire contrib extensions.
    /// </summary>
    public class HangfireContribOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether the Hangfire background server should be enabled.
        /// </summary>
        public bool EnableServer { get; set; }

        /// <summary>
        /// Gets or sets the assemblies used to scan for job types.
        /// By default the entry assembly of the application.
        /// </summary>
        public Assembly[] ScanningAssemblies { get; set; } = Array.Empty<Assembly>();

        /// <summary>
        /// Gets or sets options for the Hangfire dashboard.
        /// </summary>
        public DasbhoardOptions Dasbhoard { get; set; } = new DasbhoardOptions();

        /// <summary>
        /// Defines options for the Hangfire dashboard.
        /// </summary>
        public class DasbhoardOptions
        {
            /// <summary>
            /// Gets or sets a value indicating whether the Hangfire dashboard should be enabled.
            /// </summary>
            public bool Enabled { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the Hangfire dashboard IP-based authorization filter should be enabled.
            /// </summary>
            public bool EnableAuthorization { get; set; }

            /// <summary>
            /// Gets or sets a callback which gets invoked when authorizing a dashboard request.
            /// If not specified, the default authorization polcy is IP-based using <see cref="AllowedIps"/> when IP-addresses are specified.
            /// </summary>
            public Func<HttpContext, bool>? AuthorizationCallback { get; set; }

            /// <summary>
            /// Gets or sets the collection of IP-addresses which are allowed to access the Hangfire dashboard.
            /// This is the default authorization policy.
            /// </summary>
            public string[]? AllowedIps { get; set; }
        }
    }
}
