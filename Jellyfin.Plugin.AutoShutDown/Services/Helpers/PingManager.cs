using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Jellyfin.Plugin.AutoShutDown.Models;
using Jellyfin.Plugin.AutoShutDown.Models.Entities;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.AutoShutDown.Services.Helpers
{
    public class PingManager : ICancelShutDown
    {
        private const int TimeOutInMsec = 8000;
        private readonly ILogger _logger;

        public PingManager(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<CancelResult> CancelShutDown(string remoteHost)
        {
            _logger.LogDebug($"CancelShutDown started for: {remoteHost}");
            bool cancel = false;
            string message = $"Ping {remoteHost}";

            try
            {
                var result = await new Ping().SendPingAsync(remoteHost, TimeOutInMsec);
                cancel = result != null && result.Status == IPStatus.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"AutoShutDown Exception in PingManager CancelShutDown for remoteHost {remoteHost}: ");
            }

            _logger.LogDebug($"CancelShutdown finished message: {message} cancel result: {cancel}");

            return new CancelResult() { Message = message, Cancel = cancel };
        }
    }
}
