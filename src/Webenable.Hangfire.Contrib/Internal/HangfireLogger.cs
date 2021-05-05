using System;
using System.Collections.Generic;
using System.Text;
using Hangfire.Console;
using Hangfire.Server;
using Microsoft.Extensions.Logging;

namespace Webenable.Hangfire.Contrib.Internal
{
    public class HangfireLogger : ILogger
    {
        public IExternalScopeProvider? ScopeProvider { get; internal set; }

        public IDisposable BeginScope<TState>(TState state) => ScopeProvider?.Push(state) ?? NoopDisposable.Instance;

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None && ScopeProvider != null;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            PerformContext? ctx = null;
            var msgBuilder = new StringBuilder();

            var scopeProvider = ScopeProvider;
            scopeProvider?.ForEachScope((scopeValue, scopeState) =>
            {
                string? msg = null;
                if (scopeValue is IReadOnlyList<KeyValuePair<string, object>> kvp)
                {
                    msg = kvp.ToString();
                }
                if (scopeValue is string str)
                {
                    msg = str;
                }
                if (msg != null && !msg.StartsWith("Job"))
                {
                    msgBuilder.Append(msgBuilder.Length == 0 ? "" : " => ").Append(msg);
                    return;
                }

                if (scopeValue is PerformContext performContext)
                {
                    ctx = performContext;
                }
            }, state);

            if (ctx != null)
            {
                msgBuilder.Append(msgBuilder.Length == 0 ? "" : " => ").Append(state?.ToString());

                var color = logLevel switch
                {
                    LogLevel.Critical or LogLevel.Error => ConsoleTextColor.Red,
                    LogLevel.Warning => ConsoleTextColor.Yellow,
                    _ => ConsoleTextColor.White,
                };

                ctx.WriteLine(color, msgBuilder.ToString());
            }
        }

        private class NoopDisposable : IDisposable
        {
            public static NoopDisposable Instance = new();

            public void Dispose()
            {
            }
        }
    }
}
