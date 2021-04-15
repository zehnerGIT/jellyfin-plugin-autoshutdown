using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.AutoShutDown.Services.Helpers.ShutDown
{
    public class ShutDownLinux : BaseShutDown
    {
        public ShutDownLinux(ILogger logger) : base(logger)
        {
        }

        public override void ShutDown()
        {
            StartProcess("shutdown", "-h now");
            StartProcess("sudo", "shutdown -h now");
        }
    }
}
