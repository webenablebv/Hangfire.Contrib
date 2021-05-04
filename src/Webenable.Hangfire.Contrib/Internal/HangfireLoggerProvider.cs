using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Webenable.Hangfire.Contrib.Internal
{
    public sealed class HangfireLoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        private readonly ConcurrentDictionary<string, HangfireLogger> _loggers = new ConcurrentDictionary<string, HangfireLogger>();

        private IExternalScopeProvider? _scopeProvider;

        public ILogger CreateLogger(string categoryName) =>
            _loggers.GetOrAdd(categoryName, new HangfireLogger { ScopeProvider = _scopeProvider });

        public void Dispose()
        {
        }

        public void SetScopeProvider(IExternalScopeProvider scopeProvider) => _scopeProvider = scopeProvider;
    }
}
