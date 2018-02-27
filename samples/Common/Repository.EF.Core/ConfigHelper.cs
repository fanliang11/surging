using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Surging.Core.CPlatform.Configurations.Remote;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Repository.EF.Extensions
{
    public static class DBConfigurationExtensions
    {
        public static IConfigurationBuilder AddDBFile(this IConfigurationBuilder builder, string path)
        {
            return AddDBFile(builder, provider: null, path: path, optional: false, reloadOnChange: false);
        }

        public static IConfigurationBuilder AddDBFile(this IConfigurationBuilder builder, string path, bool optional)
        {
            return AddDBFile(builder, provider: null, path: path, optional: optional, reloadOnChange: false);
        }

        public static IConfigurationBuilder AddDBFile(this IConfigurationBuilder builder, string path, bool optional, bool reloadOnChange)
        {
            return AddDBFile(builder, provider: null, path: path, optional: optional, reloadOnChange: reloadOnChange);
        }

        public static IConfigurationBuilder AddDBFile(this IConfigurationBuilder builder, IFileProvider provider, string path, bool optional, bool reloadOnChange)
        {
            Check.NotNull(builder, "builder");
            Check.CheckCondition(() => string.IsNullOrEmpty(path), "path");
            if (provider == null && Path.IsPathRooted(path))
            {
                provider = new PhysicalFileProvider(Path.GetDirectoryName(path));
                path = Path.GetFileName(path);
            }
            var source = new DBConfigurationSource
            {
                FileProvider = provider,
                Path = path,
                Optional = optional,
                ReloadOnChange = reloadOnChange
            };
            builder.Add(source);
            DBConfig.Configuration = builder.Build();
            return builder;
        }
    }
    public class DBConfig
    {

        public static IConfiguration Configuration { get; set; }
        /* static ConfigHelper()
       {
           configuration = AutofacContainer.Resolve<IConfiguration>();
       }

       public static IConfigurationSection GetSection(string key)
       {
           return configuration.GetSection(key);
       }

       public static string GetConfigurationValue(string key)
       {
           return configuration[key];
       }

       public static string GetConfigurationValue(string section, string key)
       {
           return GetSection(section)?[key];
       }

       public static string GetConnectionString(string key)
       {
           return configuration.GetConnectionString(key);
       }
       */
    }

    public class DBConfigurationSource : FileConfigurationSource
    {
        public string ConfigurationKeyPrefix { get; set; }

        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            FileProvider = FileProvider ?? builder.GetFileProvider();
            return new DBConfigurationProvider(this);
        }
    }
    public class DBConfigurationProvider : FileConfigurationProvider
    {
        public DBConfigurationProvider(DBConfigurationSource source) : base(source) { }

        public override void Load(Stream stream)
        {
            var parser = new JsonConfigurationParser();
            this.Data = parser.Parse(stream, null);
        }
    }
}
