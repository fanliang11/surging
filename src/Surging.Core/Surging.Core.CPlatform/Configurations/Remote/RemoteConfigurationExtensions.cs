using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Configurations.Remote
{
    /// <summary>
    /// Extension methods for adding <see cref="RemoteConfigurationProvider"/>.
    /// </summary>
    public static class RemoteConfigurationExtensions
    {
        /// <summary>
        /// Adds a remote configuration source to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="configurationUri">The remote uri to </param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddRemoteSource(this IConfigurationBuilder builder, Uri configurationUri)
        {
            return builder.AddRemoteSource(configurationUri, optional: false);
        }

        /// <summary>
        /// Adds a remote configuration source to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="configurationUri">The remote uri to </param>
        /// <param name="optional">Whether the remote configuration source is optional.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddRemoteSource(this IConfigurationBuilder builder, Uri configurationUri, bool optional)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (configurationUri == null)
            {
                throw new ArgumentNullException(nameof(configurationUri));
            }

            var source = new RemoteConfigurationSource
            {
                ConfigurationUri = configurationUri,
                Optional = optional,
            };

            return builder.AddRemoteSource(source);
        }

        /// <summary>
        /// Adds a remote configuration source to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="configurationUri">The remote uri to </param>
        /// <param name="optional">Whether the remote configuration source is optional.</param> 
        /// <param name="events">Events that get add </param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddRemoteSource(this IConfigurationBuilder builder, Uri configurationUri, bool optional, RemoteConfigurationEvents events)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (configurationUri == null)
            {
                throw new ArgumentNullException(nameof(configurationUri));
            }

            if (events == null)
            {
                throw new ArgumentNullException(nameof(events));
            }

            var source = new RemoteConfigurationSource
            {
                ConfigurationUri = configurationUri,
                Events = events,
                Optional = optional,
            };

            return builder.AddRemoteSource(source);
        }
        
        public static IConfigurationBuilder AddRemoteSource(this IConfigurationBuilder builder, RemoteConfigurationSource source)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            builder.Add(source);
            return builder;
        }
    }
}
