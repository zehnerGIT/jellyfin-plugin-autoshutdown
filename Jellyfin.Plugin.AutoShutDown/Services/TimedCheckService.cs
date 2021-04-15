using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.AutoShutDown.Configuration;
using Jellyfin.Plugin.AutoShutDown.Models.Entities;
using Jellyfin.Plugin.AutoShutDown.Services.Helpers;
using Jellyfin.Plugin.AutoShutDown.Services.Helpers.ShutDown;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.AutoShutDown.Services
{
    public sealed class TimedCheckService : IServerEntryPoint
    {
        private static Lazy<BaseShutDown> _lazyShutDown = null;
        private static int _initialDelayInMin = PluginConfiguration.DefaultInitialDelay;
        private static int _intervalInMin = PluginConfiguration.DefaultInterval;
        private readonly CancellationTokenSource _stoppingCts = new CancellationTokenSource();
        private readonly PingManager _pingManager;
        private readonly PortManager _portManager;
        private readonly ILogger _logger;
        private Timer _timer;
        private int _executionCount = 0;

        public TimedCheckService(ILogger<TimedCheckService> logger)
        {
            _logger = logger;
            _initialDelayInMin = AutoShutDownPlugin.Instance.Configuration.InitialDelayInMin;
            _intervalInMin = AutoShutDownPlugin.Instance.Configuration.IntervalInMin;
            _pingManager = new PingManager(logger);
            _portManager = new PortManager(logger);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"AutoShutDown TimedCheckService is stopping");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _stoppingCts.Cancel();
            _timer?.Dispose();
            GC.SuppressFinalize(this);
        }

        public Task RunAsync()
        {
            _logger.LogInformation($"AutoShutDown TimedCheckService is starting");
            _lazyShutDown = new Lazy<BaseShutDown>(InitShutDown);
            _initialDelayInMin = AutoShutDownPlugin.Instance.Configuration.InitialDelayInMin;
            _intervalInMin = AutoShutDownPlugin.Instance.Configuration.IntervalInMin;
            _logger.LogInformation($"AutoShutDown Initial configuration:");
            LogConfiguration();
            _timer = new Timer(DoWork, null, TimeSpan.FromMinutes(AutoShutDownPlugin.Instance.Configuration.InitialDelayInMin), TimeSpan.FromMinutes(AutoShutDownPlugin.Instance.Configuration.IntervalInMin));

            return Task.CompletedTask;
        }

        private BaseShutDown InitShutDown()
        {
            BaseShutDown shutDown;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD) ||
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                shutDown = new ShutDownLinux(_logger);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                shutDown = new ShutDownWindows(_logger);
            }
            else
            {
                throw new NotImplementedException("Platform not supported");
            }

            return shutDown;
        }

        private async void DoWork(object state)
        {
            _logger.LogDebug($"TimedCheckService is working");
            // don't run tasks if configuration has changed => better wait for next run
            if (!SynchronizeTimerWithConfiguration() && (AutoShutDownPlugin.Instance.Configuration.CheckPorts.Any() || AutoShutDownPlugin.Instance.Configuration.PingHosts.Any()))
            {
                var cancelResults = new List<Task<CancelResult>>();
                foreach (var port in AutoShutDownPlugin.Instance.Configuration.CheckPorts)
                {
                    cancelResults.Add(_portManager.CancelShutDown(port));
                }

                foreach (var pingHost in AutoShutDownPlugin.Instance.Configuration.PingHosts)
                {
                    cancelResults.Add(_pingManager.CancelShutDown(pingHost));
                }

                var cancelTask = await Helpers.TaskExtensions.WhenFirst(cancelResults, t => t.Status == TaskStatus.RanToCompletion && t.Result.Cancel);
                if (cancelTask != null)
                {
                    var value = await cancelTask;
                    Interlocked.Exchange(ref _executionCount, 0);
                    _logger.LogInformation($"AutoShutDown canceled: {value.Message} {value.Cancel}");
                }
                else
                {
                    var count = Interlocked.Increment(ref _executionCount);
                    _logger.LogInformation($"AutoShutDown increment executionCount to {count}");
                    if (count >= AutoShutDownPlugin.Instance.Configuration.Executions)
                    {
                        var shutDown = _lazyShutDown.Value;
                        if (shutDown != null)
                        {
                            lock (shutDown)
                            {
                                _logger.LogInformation($"AutoShutDown called {shutDown}");
                                shutDown.ShutDown();
                            }
                        }
                    }
                }
            }
        }

        private bool SynchronizeTimerWithConfiguration()
        {
            _logger.LogDebug("SynchronizeTimerWithConfiguration");
            bool changed = false;
            if (AutoShutDownPlugin.Instance.Configuration.InitialDelayInMin >= 0 && AutoShutDownPlugin.Instance.Configuration.InitialDelayInMin != _initialDelayInMin)
            {
                _initialDelayInMin = AutoShutDownPlugin.Instance.Configuration.InitialDelayInMin;
                changed = true;
            }

            if (AutoShutDownPlugin.Instance.Configuration.IntervalInMin >= 1 && AutoShutDownPlugin.Instance.Configuration.IntervalInMin != _intervalInMin)
            {
                _intervalInMin = AutoShutDownPlugin.Instance.Configuration.IntervalInMin;
                changed = true;
            }

            if (changed)
            {
                _timer.Change(TimeSpan.FromMinutes(AutoShutDownPlugin.Instance.Configuration.InitialDelayInMin), TimeSpan.FromMinutes(AutoShutDownPlugin.Instance.Configuration.IntervalInMin));
                _logger.LogInformation($"AutoShutDown Timer changed:");
                LogConfiguration();
            }

            return changed;
        }

        private void LogConfiguration()
        {
            _logger.LogInformation($"AutoShutDown InitialDelayInMin: {_initialDelayInMin} IntervalInMin: {_intervalInMin} Executions: {AutoShutDownPlugin.Instance.Configuration.Executions}");
            _logger.LogInformation($"AutoShutDown LocalPorts: {AutoShutDownPlugin.Instance.Configuration.LocalPorts} RemoteHosts: {AutoShutDownPlugin.Instance.Configuration.RemoteHosts}");
        }
    }
}
