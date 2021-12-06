using System.Threading.Tasks;
using Jellyfin.Plugin.AutoShutDown.Models;
using Jellyfin.Plugin.AutoShutDown.Models.Entities;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.AutoShutDown.Services.Helpers
{
    public class JellyfinLibraryManager : ICancelShutDown
    {
        private readonly ILibraryManager _libraryManager;
        private readonly ILogger _logger;

        public JellyfinLibraryManager(ILibraryManager libraryManager, ILogger logger)
        {
            _libraryManager = libraryManager;
            _logger = logger;
        }

        public async Task<CancelResult> CancelShutDown(string config)
        {
            _logger.LogDebug($"CancelShutDown started for JellyfinLibraryManager");

            string message = "JellyfinLibraryManager CancelShutDown";
            bool cancel = false;

            if (_libraryManager != null)
            {
                cancel = _libraryManager.IsScanRunning;
                message = $"{message}: {cancel}";
                _logger.LogDebug($"{message}");
            }
            else
            {
                _logger.LogWarning($"Empty param {nameof(_libraryManager)}");
            }

            _logger.LogDebug($"CancelShutdown finished message: {message} cancel result: {cancel}");

            return new CancelResult() { Message = message, Cancel = cancel };
        }
    }
}