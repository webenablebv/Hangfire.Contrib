using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Webenable.Hangfire.Contrib.Tests
{
    public class HangfireJobTests
    {
        [Fact]
        public async Task ExecutesWithNullParams()
        {
            // Arrange
            var job = new FooJob(new LoggerFactory());

            // Act
            await job.ExecuteAsync(null, null);

            // Assert
            Assert.True(job.Executed);
        }
    }

    public class FooJob : HangfireJob
    {
        public FooJob(ILoggerFactory loggerFactory) : base(loggerFactory)
        {
        }

        protected override Task ExecuteCoreAsync(IJobCancellationToken cancellationToken)
        {
            Executed = true;
            return Task.CompletedTask;
        }

        internal bool Executed { get; set; }
    }
}
