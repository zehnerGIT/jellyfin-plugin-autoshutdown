using System;
using System.Collections.Generic;
using Jellyfin.Plugin.AutoShutDown.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.AutoShutDown
{
    public class AutoShutDownPlugin : BasePlugin<PluginConfiguration>, IHasWebPages
    {
        public AutoShutDownPlugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
            : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
        }

        /// <summary>
        /// Gets current plugin instance.
        /// </summary>
        public static AutoShutDownPlugin Instance { get; private set; }

        public override Guid Id => Guid.Parse("746785d5-20d7-4840-ad59-947f9844ad87");

        public override string Name => "Auto ShutDown";

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoShutDownPlugin"/> class.
        /// </summary>
        /// <param name="applicationPaths">Instance of the <see cref="IApplicationPaths"/> interface.</param>
        /// <param name="xmlSerializer">Instance of the <see cref="IXmlSerializer"/> interface.</param>
        /// <returns>Instance of the <see cref="PluginPageInfo"/> configuration page.</returns>
        public IEnumerable<PluginPageInfo> GetPages()
        {
            yield return new PluginPageInfo
            {
                Name = Name,
                EmbeddedResourcePath = $"{GetType().Namespace}.Configuration.configPage.html"
            };
        }
    }
}
