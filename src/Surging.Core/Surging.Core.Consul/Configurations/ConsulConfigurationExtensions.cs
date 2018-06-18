using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Surging.Core.Consul.Configurations
{
    public static class ConsulConfigurationExtensions
    {
        public static IConfigurationBuilder AddConsulFile(this IConfigurationBuilder builder, string path)
        {
            return AddConsulFile(builder, provider: null, path: path, optional: false, reloadOnChange: false);
        }

        public static IConfigurationBuilder AddConsulFile(this IConfigurationBuilder builder, string path, bool optional)
        {
            return AddConsulFile(builder, provider: null, path: path, optional: optional, reloadOnChange: false);
        }

        public static IConfigurationBuilder AddConsulFile(this IConfigurationBuilder builder, string path, bool optional, bool reloadOnChange)
        {
            return AddConsulFile(builder, provider: null, path: path, optional: optional, reloadOnChange: reloadOnChange);
        }

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
    }
}