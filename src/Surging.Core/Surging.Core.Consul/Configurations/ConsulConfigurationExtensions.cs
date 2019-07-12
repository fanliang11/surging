using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Surging.Core.Consul.Configurations
{
    /// <summary>
    /// Defines the <see cref="ConsulConfigurationExtensions" />
    /// </summary>
    public static class ConsulConfigurationExtensions
    {
        #region 方法

        /// <summary>
        /// The AddConsulFile
        /// </summary>
        /// <param name="builder">The builder<see cref="IConfigurationBuilder"/></param>
        /// <param name="provider">The provider<see cref="IFileProvider"/></param>
        /// <param name="path">The path<see cref="string"/></param>
        /// <param name="optional">The optional<see cref="bool"/></param>
        /// <param name="reloadOnChange">The reloadOnChange<see cref="bool"/></param>
        /// <returns>The <see cref="IConfigurationBuilder"/></returns>
        public static IConfigurationBuilder AddConsulFile(this IConfigurationBuilder builder, IFileProvider provider, string path, bool optional, bool reloadOnChange)
        {
            Check.NotNull(builder, "builder");
            Check.CheckCondition(() => string.IsNullOrEmpty(path), "path");
            path = EnvironmentHelper.GetEnvironmentVariable(path);
            if (provider == null && Path.IsPathRooted(path))
            {
                provider = new PhysicalFileProvider(Path.GetDirectoryName(path));
                path = Path.GetFileName(path);
            }
            var source = new ConsulConfigurationSource
            {
                FileProvider = provider,
                Path = path,
                Optional = optional,
                ReloadOnChange = reloadOnChange
            };
            builder.Add(source);
            AppConfig.Configuration = builder.Build();
            return builder;
        }

        /// <summary>
        /// The AddConsulFile
        /// </summary>
        /// <param name="builder">The builder<see cref="IConfigurationBuilder"/></param>
        /// <param name="path">The path<see cref="string"/></param>
        /// <returns>The <see cref="IConfigurationBuilder"/></returns>
        public static IConfigurationBuilder AddConsulFile(this IConfigurationBuilder builder, string path)
        {
            return AddConsulFile(builder, provider: null, path: path, optional: false, reloadOnChange: false);
        }

        /// <summary>
        /// The AddConsulFile
        /// </summary>
        /// <param name="builder">The builder<see cref="IConfigurationBuilder"/></param>
        /// <param name="path">The path<see cref="string"/></param>
        /// <param name="optional">The optional<see cref="bool"/></param>
        /// <returns>The <see cref="IConfigurationBuilder"/></returns>
        public static IConfigurationBuilder AddConsulFile(this IConfigurationBuilder builder, string path, bool optional)
        {
            return AddConsulFile(builder, provider: null, path: path, optional: optional, reloadOnChange: false);
        }

        /// <summary>
        /// The AddConsulFile
        /// </summary>
        /// <param name="builder">The builder<see cref="IConfigurationBuilder"/></param>
        /// <param name="path">The path<see cref="string"/></param>
        /// <param name="optional">The optional<see cref="bool"/></param>
        /// <param name="reloadOnChange">The reloadOnChange<see cref="bool"/></param>
        /// <returns>The <see cref="IConfigurationBuilder"/></returns>
        public static IConfigurationBuilder AddConsulFile(this IConfigurationBuilder builder, string path, bool optional, bool reloadOnChange)
        {
            return AddConsulFile(builder, provider: null, path: path, optional: optional, reloadOnChange: reloadOnChange);
        }

        #endregion 方法
    }
}