using Hangfire.Server;

namespace Webenable.Hangfire.Contrib.Internal
{
    internal class PerformContextWrapper : PerformContext
    {
        public PerformContextWrapper(PerformContext context) : base(context)
        {
        }

        public override string ToString() => BackgroundJob.Job.Type.Name;
    }
}
