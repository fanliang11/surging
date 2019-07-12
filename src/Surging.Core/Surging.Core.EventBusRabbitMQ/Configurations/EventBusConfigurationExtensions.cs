using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Surging.Core.EventBusRabbitMQ.Configurations
{
    /// <summary>
    /// Defines the <see cref="EventBusConfigurationExtensions" />
    /// </summary>
    public static class EventBusConfigurationExtensions
    {
        #region 方法

        /// <summary>
        /// The AddEventBusFile
        /// </summary>
        /// <param name="builder">The builder<see cref="IConfigurationBuilder"/></param>
        /// <param name="provider">The provider<see cref="IFileProvider"/></param>
        /// <param name="path">The path<see cref="string"/></param>
        /// <param name="basePath">The basePath<see cref="string"/></param>
        /// <param name="optional">The optional<see cref="bool"/></param>
        /// <param name="reloadOnChange">The reloadOnChange<see cref="bool"/></param>
        /// <returns>The <see cref="IConfigurationBuilder"/></returns>
        public static IConfigurationBuilder AddEventBusFile(this IConfigurationBuilder builder, IFileProvider provider, string path, string basePath, bool optional, bool reloadOnChange)
        {
            Check.NotNull(builder, "builder");
            Check.CheckCondition(() => string.IsNullOrEmpty(path), "path");
            if (provider == null && Path.IsPathRooted(path))
            {
                provider = new PhysicalFileProvider(Path.GetDirectoryName(path));
                path = Path.GetFileName(path);
            }

            var source = new EventBusConfigurationSource
            {
                FileProvider = provider,
                Path = path,
                Optional = optional,
                ReloadOnChange = reloadOnChange
            };
            builder.Add(source);
            if (!string.IsNullOrEmpty(basePath))
                builder.SetBasePath(basePath);
            AppConfig.Configuration = builder.Build();
            return builder;
        }

        /// <summary>
        /// The AddEventBusFile
        /// </summary>
        /// <param name="builder">The builder<see cref="IConfigurationBuilder"/></param>
        /// <param name="path">The path<see cref="string"/></param>
        /// <returns>The <see cref="IConfigurationBuilder"/></returns>
        public static IConfigurationBuilder AddEventBusFile(this IConfigurationBuilder builder, string path)
        {
            return AddEventBusFile(builder, provider: null, path: path, basePath: null, optional: false, reloadOnChange: false);
        }

        /// <summary>
        /// The AddEventBusFile
        /// </summary>
        /// <param name="builder">The builder<see cref="IConfigurationBuilder"/></param>
        /// <param name="path">The path<see cref="string"/></param>
        /// <param name="optional">The optional<see cref="bool"/></param>
        /// <returns>The <see cref="IConfigurationBuilder"/></returns>
        public static IConfigurationBuilder AddEventBusFile(this IConfigurationBuilder builder, string path, bool optional)
        {
            return AddEventBusFile(builder, provider: null, path: path, basePath: null, optional: optional, reloadOnChange: false);
        }

        /// <summary>
        /// The AddEventBusFile
        /// </summary>
        /// <param name="builder">The builder<see cref="IConfigurationBuilder"/></param>
        /// <param name="path">The path<see cref="string"/></param>
        /// <param name="optional">The optional<see cref="bool"/></param>
        /// <param name="reloadOnChange">The reloadOnChange<see cref="bool"/></param>
        /// <returns>The <see cref="IConfigurationBuilder"/></returns>
        public static IConfigurationBuilder AddEventBusFile(this IConfigurationBuilder builder, string path, bool optional, bool reloadOnChange)
        {
            return AddEventBusFile(builder, provider: null, path: path, basePath: null, optional: optional, reloadOnChange: reloadOnChange);
        }

        /// <summary>
        /// The AddEventBusFile
        /// </summary>
        /// <param name="builder">The builder<see cref="IConfigurationBuilder"/></param>
        /// <param name="path">The path<see cref="string"/></param>
        /// <param name="basePath">The basePath<see cref="string"/></param>
        /// <param name="optional">The optional<see cref="bool"/></param>
        /// <param name="reloadOnChange">The reloadOnChange<see cref="bool"/></param>
        /// <returns>The <see cref="IConfigurationBuilder"/></returns>
        public static IConfigurationBuilder AddEventBusFile(this IConfigurationBuilder builder, string path, string basePath, bool optional, bool reloadOnChange)
        {
            return AddEventBusFile(builder, provider: null, path: path, basePath: basePath, optional: optional, reloadOnChange: reloadOnChange);
        }

        #endregion 方法
    }
}