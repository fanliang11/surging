using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Surging.Core.CPlatform;
namespace Surging.Core.Configuration.Apollo.Configurations
{
    public class ConfigurationFactory : IConfigurationFactory
    { 
        private const string CONFIG_FILE_PATH = "APOLLO__CONFIG__PATH"; 

        public ConfigurationFactory()
        {  
        }

        public IConfiguration Create()
        {
            var builder = new ConfigurationBuilder();
            var environmentName = Environment.GetEnvironmentVariable("environmentname");
          

            builder.AddJsonFile("apollo.json", true)
                .AddJsonFile($"apollo.{environmentName}.json", true);


            if (!string.IsNullOrEmpty(AppConfig.ServerOptions.RootPath))
            {
                var skyapmPath = Path.Combine(AppConfig.ServerOptions.RootPath, "apollo.json");
                builder.AddJsonFile(skyapmPath, true);
            }


            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(CONFIG_FILE_PATH)))
            {
                builder.AddJsonFile(Environment.GetEnvironmentVariable(CONFIG_FILE_PATH), false);
            }

            builder.AddEnvironmentVariables();

            var providers = AppConfig.Configuration.Providers;

            foreach (var provider in providers)
            {
                var fileConfigurationProvider = provider as FileConfigurationProvider;
                if (fileConfigurationProvider != null)
                {
                    builder.Add(fileConfigurationProvider.Source);
                }
            }
            var config = builder.Build();
            var section = config.GetSection("apollo");
            if (!section.Exists())
            {
                throw new Exception("apollo config file not exists!");
            }
            var apollo = builder.AddApollo(section); 
            apollo.AddNamespaceSurgingApollo("surgingSettings");
            return config;
        }
    }
}