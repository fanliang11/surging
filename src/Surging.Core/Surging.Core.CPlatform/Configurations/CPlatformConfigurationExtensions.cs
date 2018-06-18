using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Surging.Core.CPlatform.Configurations
{
    public static class CacheConfigurationExtensionsstatic
    {
        public static IConfigurationBuilder AddCPlatformFile(this IConfigurationBuilder builder, string path)
        {
            return AddCPlatformFile(builder, provider: null, path: path, optional: false, reloadOnChange: false);
        }

        public static IConfigurationBuilder AddCPlatformFile(this IConfigurationBuilder builder, string path, bool optional)
        {
            return AddCPlatformFile(builder, provider: null, path: path, optional: optional, reloadOnChange: false);
        }

        public static IConfigurationBuilder AddCPlatformFile(this IConfigurationBuilder builder, string path, bool optional, bool reloadOnChange)
        {
            return AddCPlatformFile(builder, provider: null, path: path, optional: optional, reloadOnChange: reloadOnChange);
        }

        public static IConfigurationBuilder AddCPlatformFile(this IConfigurationBuilder builder, IFileProvider provider, string path, bool optional, bool reloadOnChange)
        {
            Check.NotNull(builder, "builder");
            Check.CheckCondition(() => string.IsNullOrEmpty(path), "path");
            path = EnvironmentHelper.GetEnvironmentVariable(path);
            if (provider == null && Path.IsPathRooted(path))
            {
                provider = new PhysicalFileProvider(Path.GetDirectoryName(path));
                path = Path.GetFileName(path);
            }
            var source = new CPlatformConfigurationSource
            {
                FileProvider = provider,
                Path = path,
                Optional = optional,
                ReloadOnChange = reloadOnChange
            };
            builder.Add(source);
            AppConfig.Configuration = builder.Build();
           AppConfig.ServerOptions = AppConfig.Configuration.Get<SurgingServerOptions>();
            var section = AppConfig.Configuration.GetSection("Surging");
            if (section.Exists())
                AppConfig.ServerOptions = AppConfig.Configuration.GetSection("Surging").Get<SurgingServerOptions>();
            return builder;
        }
    }
}