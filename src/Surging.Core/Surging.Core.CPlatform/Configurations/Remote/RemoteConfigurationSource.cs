using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;

namespace Surging.Core.CPlatform.Configurations.Remote
{
   public class RemoteConfigurationSource : IConfigurationSource
    {
        /// <summary>
        /// The uri to call to fetch 
        /// </summary>
        public Uri ConfigurationUri { get; set; }

        /// <summary>
        /// Determines if the remote source is optional 
        /// </summary>
        public bool Optional { get; set; }

        /// <summary>
        /// The HttpMessageHandler used to communicate with remote configuration provider.
        /// </summary>
        public HttpMessageHandler BackchannelHttpHandler { get; set; }

        /// <summary>
        /// Gets or sets timeout value in milliseconds for back channel communications with the remote identity provider.
        /// </summary>
        /// <value>
        /// The back channel timeout.
        /// </value>
        public TimeSpan BackchannelTimeout { get; set; } = TimeSpan.FromSeconds(60);

        /// <summary>
        /// Parser for parsing the returned data into the required configuration source
        /// </summary>
        public IConfigurationParser Parser { get; set; }

        /// <summary>
        /// The accept header used to create a MediaTypeWithQualityHeaderValue
        /// </summary>
        public string MediaType { get; set; } = "application/json";

        /// <summary>
        /// Events providing hooks into the remote call
        /// </summary>
        public RemoteConfigurationEvents Events { get; set; } = new RemoteConfigurationEvents();

        /// <summary>
        /// If provided, keys loaded from endpoint will be prefixed with the provided value
        /// </summary>
        public string ConfigurationKeyPrefix { get; set; }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new RemoteConfigurationProvider(this);
        }
    }
}