using System;
using System.Diagnostics;
using Jellyfin.Plugin.AutoShutDown.Models;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.AutoShutDown.Services.Helpers.ShutDown
{
    public abstract class BaseShutDown : IShutDown
    {
        private readonly ILogger _logger;

        public BaseShutDown(ILogger logger)
        {
            _logger = logger;
        }

        private BaseShutDown()
        {
        }

        public abstract void ShutDown();

        protected void StartProcess(string command, string arguments)
        {
            if (!string.IsNullOrWhiteSpace(command))
            {
                try
                {
                    using (var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = command,
                            CreateNoWindow = true,
                            UseShellExecute = false,
                            RedirectStandardError = true,
                        }
                    })
                    {
                        process.ErrorDataReceived += (sender, args) => _logger.LogError($"AutoShutDown StandardError: {args.Data}");

                        if (!string.IsNullOrWhiteSpace(arguments))
                        {
                            process.StartInfo.Arguments = arguments;
                        }

                        _logger.LogDebug($"StartProcess: {command} arguments: {arguments}");
                        process.Start();
                        process.BeginErrorReadLine();

                        process.WaitForExit(8 * 1000); // you need this in order to flush the output buffer
                        process.Close();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "AutoShutDown Process.Start");
                }
            }
            else
            {
                _logger.LogWarning("AutoShutDown StartProcess: command is empty");
            }
        }
    }
}
