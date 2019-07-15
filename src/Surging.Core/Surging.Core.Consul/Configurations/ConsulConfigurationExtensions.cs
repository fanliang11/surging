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
    /// Consul配置扩展 <see cref="ConsulConfigurationExtensions" />
    /// </summary>
    public static class ConsulConfigurationExtensions
    {
        #region 方法

        /// <summary>
        /// 加Consul配置文件
        /// </summary>
        /// <param name="builder">构建配置对象建造器<see cref="IConfigurationBuilder"/></param>
        /// <param name="provider">The provider<see cref="IFileProvider"/></param>
        /// <param name="path">文件相对路径<see cref="string"/></param>
        /// <param name="optional">是否可选<see cref="bool"/></param>
        /// <param name="reloadOnChange">当改变时是不是重新加载<see cref="bool"/></param>
        /// <returns>构建配置对象建造器 <see cref="IConfigurationBuilder"/></returns>
        /// <exception cref="PathTooLongException">路径太长</exception>
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
        /// 加Consul配置文件
        /// </summary>
        /// <param name="builder">构建配置对象建造器<see cref="IConfigurationBuilder"/></param>
        /// <param name="path">文件相对路径<see cref="string"/></param>
        /// <returns>构建配置对象建造器 <see cref="IConfigurationBuilder"/></returns>
        /// <exception cref="PathTooLongException">路径太长</exception>
        public static IConfigurationBuilder AddConsulFile(this IConfigurationBuilder builder, string path)
        {
            return AddConsulFile(builder, provider: null, path: path, optional: false, reloadOnChange: false);
        }

        /// <summary>
        /// 加Consul配置文件
        /// </summary>
        /// <param name="builder">构建配置对象建造器<see cref="IConfigurationBuilder"/></param>
        /// <param name="path">文件相对路径<see cref="string"/></param>
        /// <param name="optional">是否可选<see cref="bool"/></param>
        /// <returns>构建配置对象建造器 <see cref="IConfigurationBuilder"/></returns>
        /// <exception cref="PathTooLongException">路径太长</exception>
        public static IConfigurationBuilder AddConsulFile(this IConfigurationBuilder builder, string path, bool optional)
        {
            return AddConsulFile(builder, provider: null, path: path, optional: optional, reloadOnChange: false);
        }

        /// <summary>
        /// 加Consul配置文件
        /// </summary>
        /// <param name="builder">构建配置对象建造器<see cref="IConfigurationBuilder"/></param>
        /// <param name="path">文件相对路径<see cref="string"/></param>
        /// <param name="optional">是否可选<see cref="bool"/></param>
        /// <param name="reloadOnChange">当改变时是不是重新加载<see cref="bool"/></param>
        /// <returns>构建配置对象建造器 <see cref="IConfigurationBuilder"/></returns>
        /// <exception cref="PathTooLongException">路径太长</exception>
        public static IConfigurationBuilder AddConsulFile(this IConfigurationBuilder builder, string path, bool optional, bool reloadOnChange)
        {
            return AddConsulFile(builder, provider: null, path: path, optional: optional, reloadOnChange: reloadOnChange);
        }

        #endregion 方法
    }
}