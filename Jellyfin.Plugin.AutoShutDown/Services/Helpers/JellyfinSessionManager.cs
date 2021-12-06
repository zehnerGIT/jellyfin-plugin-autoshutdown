using System.Linq;
using System.Threading.Tasks;
using Jellyfin.Plugin.AutoShutDown.Models;
using Jellyfin.Plugin.AutoShutDown.Models.Entities;
using MediaBrowser.Controller.Session;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.AutoShutDown.Services.Helpers
{
    public class JellyfinSessionManager : ICancelShutDown
    {
        private readonly ISessionManager _sessionManager;
        private readonly ILogger _logger;

        public JellyfinSessionManager(ISessionManager sessionManager, ILogger logger)
        {
            _sessionManager = sessionManager;
            _logger = logger;
        }

        public async Task<CancelResult> CancelShutDown(string config)
        {
            _logger.LogDebug($"CancelShutDown started for JellyfinSessionManager");

            string message = "JellyfinSessionManager CancelShutDown";
            bool cancel = false;

            if (_sessionManager != null)
            {
                if (_sessionManager.Sessions != null)
                {
                    SessionInfo activeSession = _sessionManager.Sessions.FirstOrDefault(si => si.NowPlayingItem != null);
                    if (activeSession != null)
                    {
                        cancel = true;
                        message = $"{message} ActiveSession: {activeSession.DeviceName} PlayState: {activeSession.PlayState} NowPlaying: {activeSession.NowPlayingItem.Name}";
                        _logger.LogDebug($"CancelShutDown (true) ActiveSession: {activeSession.DeviceName}");
                    }
                }
            }
            else
            {
                _logger.LogWarning($"Empty param {nameof(_sessionManager)}");
            }

            _logger.LogDebug($"CancelShutdown finished message: {message} cancel result: {cancel}");

            return new CancelResult() { Message = message, Cancel = cancel };
        }
    }
}