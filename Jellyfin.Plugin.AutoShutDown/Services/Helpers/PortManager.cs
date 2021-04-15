using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Jellyfin.Plugin.AutoShutDown.Models;
using Jellyfin.Plugin.AutoShutDown.Models.Entities;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.AutoShutDown.Services.Helpers
{
    public class PortManager : ICancelShutDown
    {
        private readonly ILogger _logger;

        public PortManager(ILogger logger)
        {
            _logger = logger;
            foreach (TcpConnectionInformation c in IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections())
            {
                _logger.LogDebug($"TcpConnectionInformation LocalEndPoint: {c.LocalEndPoint} RemoteEndPoint: {c.RemoteEndPoint} State: {c.State}");
            }
        }

        public async Task<CancelResult> CancelShutDown(string port)
        {
            _logger.LogDebug($"CancelShutDown started for: {port}");

            bool cancel = false;
            string message = $"Port {port}";
            try
            {
                if (int.TryParse(port, out int portParsed))
                {
                    var openConnection = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections().FirstOrDefault(ep => ep.State == TcpState.Established && ep.LocalEndPoint.Port == portParsed &&
                        IsLocalIpAddress(ep.LocalEndPoint.Address.ToString()) && !IsLocalIpAddress(ep.RemoteEndPoint.Address.ToString()));

                    if (openConnection != null)
                    {
                        cancel = true;
                        message = $"{message} LocalEndPoint: {openConnection.LocalEndPoint} RemoteEndPoint: {openConnection.RemoteEndPoint}";
                        _logger.LogDebug($"CancelShutDown (true) RemoteEndPoint: {openConnection.RemoteEndPoint}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"AutoShutDown Exception in PortManager CancelShutDown for port {port}: ");
            }

            _logger.LogDebug($"CancelShutdown finished message: {message} cancel result: {cancel}");

            return new CancelResult() { Message = message, Cancel = cancel };
        }

        private /*static*/ bool IsLocalIpAddress(string host)
        {
            try
            {
                // get host IP addresses
                IPAddress[] hostIPs = Dns.GetHostAddresses(host);
                // get local IP addresses
                IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());

                // test if any host IP equals to any local IP or to localhost
                foreach (IPAddress hostIP in hostIPs)
                {
                    // is localhost
                    if (IPAddress.IsLoopback(hostIP))
                    {
                        return true;
                    }

                    // is local address
                    foreach (IPAddress localIP in localIPs)
                    {
                        if (hostIP.Equals(localIP))
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"AutoShutDown Exception in PortManager IsLocalIpAddress for host {host}: ");
            }

            return false;
        }
    }
}
