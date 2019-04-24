using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Caching.Configurations.Remote
{
    /// <summary>
    /// 添加 <see cref="RemoteConfigurationProvider"/>扩展方法
    /// </summary>
    public static class RemoteConfigurationExtensions
    {
        /// <summary>
        ///对于 <paramref name="builder"/>添加远程配置方法
        /// </summary>
        /// <param name="builder">  <see cref="IConfigurationBuilder"/></param>
        /// <param name="configurationUri">远程地址 </param>
        /// <returns> <see cref="IConfigurationBuilder"/></returns>
        public static IConfigurationBuilder AddRemoteSource(this IConfigurationBuilder builder, Uri configurationUri)
        {
            return builder.AddRemoteSource(configurationUri, optional: false);
        }

        /// <summary>
        ///对于 <paramref name="builder"/>添加远程配置方法
        /// </summary>
        /// <param name="builder"><see cref="IConfigurationBuilder"/></param>
        /// <param name="configurationUri">远程地址</param>
        /// <param name="optional">远程配置源是否可选</param>
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
        ///对于 <paramref name="builder"/>添加远程配置方法
        /// </summary>
        /// <param name="builder"> <see cref="IConfigurationBuilder"/></param>
        /// <param name="configurationUri">远程地址 </param>
        /// <param name="optional">远程配置源是否可选</param> 
        /// <param name="events">添加事件 </param>
        /// <returns> <see cref="IConfigurationBuilder"/></returns>
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


        /// <summary>
        ///对于 <paramref name="builder"/>添加远程配置方法
        /// </summary>
        /// <param name="builder"> <see cref="IConfigurationBuilder"/></param>
        /// <param name="source"> <see cref="RemoteConfigurationSource"/></param>
        /// <returns><see cref="IConfigurationBuilder"/></returns>
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
