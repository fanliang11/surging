using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;

namespace Surging.Core.CPlatform.Configurations.Remote
{
    /// <summary>
    /// Defines the <see cref="RemoteConfigurationSource" />
    /// </summary>
    public class RemoteConfigurationSource : IConfigurationSource
    {
        #region 属性

        /// <summary>
        /// Gets or sets the BackchannelHttpHandler
        /// The HttpMessageHandler used to communicate with remote configuration provider.
        /// </summary>
        public HttpMessageHandler BackchannelHttpHandler { get; set; }

        /// <summary>
        /// Gets or sets the BackchannelTimeout
        /// Gets or sets timeout value in milliseconds for back channel communications with the remote identity provider.
        /// </summary>
        public TimeSpan BackchannelTimeout { get; set; } = TimeSpan.FromSeconds(60);

        /// <summary>
        /// Gets or sets the ConfigurationKeyPrefix
        /// If provided, keys loaded from endpoint will be prefixed with the provided value
        /// </summary>
        public string ConfigurationKeyPrefix { get; set; }

        /// <summary>
        /// Gets or sets the ConfigurationUri
        /// The uri to call to fetch
        /// </summary>
        public Uri ConfigurationUri { get; set; }

        /// <summary>
        /// Gets or sets the Events
        /// Events providing hooks into the remote call
        /// </summary>
        public RemoteConfigurationEvents Events { get; set; } = new RemoteConfigurationEvents();

        /// <summary>
        /// Gets or sets the MediaType
        /// The accept header used to create a MediaTypeWithQualityHeaderValue
        /// </summary>
        public string MediaType { get; set; } = "application/json";

        /// <summary>
        /// Gets or sets a value indicating whether Optional
        /// Determines if the remote source is optional
        /// </summary>
        public bool Optional { get; set; }

        /// <summary>
        /// Gets or sets the Parser
        /// Parser for parsing the returned data into the required configuration source
        /// </summary>
        public IConfigurationParser Parser { get; set; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The Build
        /// </summary>
        /// <param name="builder">The builder<see cref="IConfigurationBuilder"/></param>
        /// <returns>The <see cref="IConfigurationProvider"/></returns>
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new RemoteConfigurationProvider(this);
        }

        #endregion 方法
    }
}