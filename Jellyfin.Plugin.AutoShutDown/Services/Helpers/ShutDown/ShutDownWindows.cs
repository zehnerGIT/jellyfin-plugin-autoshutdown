using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.AutoShutDown.Services.Helpers.ShutDown
{
    public class ShutDownWindows : BaseShutDown
    {
        public ShutDownWindows(ILogger logger) : base(logger)
        {
        }

        public override void ShutDown()
        {
            StartProcess("shutdown", "/s /t 0");
        }
    }
}
