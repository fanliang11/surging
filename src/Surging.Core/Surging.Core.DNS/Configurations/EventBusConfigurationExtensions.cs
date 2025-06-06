using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Surging.Core.DNS.Configurations
{
    public static class EventBusConfigurationExtensions
    {
        public static IConfigurationBuilder AddDnsFile(this IConfigurationBuilder builder, string path)
        {
            return AddDnsFile(builder, provider: null, path: path, basePath: null, optional: false, reloadOnChange: false);
        }

        public static IConfigurationBuilder AddDnsFile(this IConfigurationBuilder builder, string path, bool optional)
        {
            return AddDnsFile(builder, provider: null, path: path, basePath: null, optional: optional, reloadOnChange: false);
        }

        public static IConfigurationBuilder AddDnsFile(this IConfigurationBuilder builder, string path, bool optional, bool reloadOnChange)
        {
            return AddDnsFile(builder, provider: null, path: path, basePath: null, optional: optional, reloadOnChange: reloadOnChange);
        }

        public static IConfigurationBuilder AddDnsFile(this IConfigurationBuilder builder, string path, string basePath, bool optional, bool reloadOnChange)
        {
            return AddDnsFile(builder, provider: null, path: path, basePath: basePath, optional: optional, reloadOnChange: reloadOnChange);
        }

        public static IConfigurationBuilder AddDnsFile(this IConfigurationBuilder builder, IFileProvider provider, string path, string basePath, bool optional, bool reloadOnChange)
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
            AppConfig.DnsOption = AppConfig.Configuration.Get<DnsOption>();
            return builder;
        }
    }
}
