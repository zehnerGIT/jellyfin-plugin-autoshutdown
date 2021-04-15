using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.AutoShutDown.Configuration
{
    public class PluginConfiguration : BasePluginConfiguration
    {
        public const int DefaultInterval = 5;
        public const int DefaultInitialDelay = 15;
        public const int DefaultExecutions = 3;

        public PluginConfiguration()
        {
            IntervalInMin = DefaultInterval;
            InitialDelayInMin = DefaultInitialDelay;
            Executions = DefaultExecutions;
        }

        public int IntervalInMin { get; set; }

        public int InitialDelayInMin { get; set; }

        public int Executions { get; set; }

        private string _localPorts;

        public string LocalPorts
        {
            get
            {
                return _localPorts;
            }

            set
            {
                if (value != _localPorts)
                {
                    CheckPorts.Clear();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        value = value.Trim();
                        foreach (var port in value.Split(null))
                        {
                            if (!string.IsNullOrWhiteSpace(port) && int.TryParse(port, out int checkPort))
                            {
                                CheckPorts.Add(Convert.ToString(checkPort));
                            }
                        }
                    }

                    _localPorts = value;
                }
            }
        }

        [XmlIgnore]
        public List<string> CheckPorts { get; } = new List<string>();

        private string _remoteHosts;

        public string RemoteHosts
        {
            get
            {
                return _remoteHosts;
            }

            set
            {
                if (value != _remoteHosts)
                {
                    PingHosts.Clear();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        value = value.Trim();
                        foreach (var pingHost in value.Split(null))
                        {
                            if (!string.IsNullOrWhiteSpace(pingHost))
                            {
                                PingHosts.Add(pingHost.Trim());
                            }
                        }
                    }

                    _remoteHosts = value;
                }
            }
        }

        [XmlIgnore]
        public List<string> PingHosts { get; } = new List<string>();
    }
}
