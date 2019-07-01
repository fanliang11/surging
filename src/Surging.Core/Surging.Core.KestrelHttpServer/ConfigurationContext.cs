using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Surging.Core.CPlatform.Module;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.KestrelHttpServer
{
    public class ConfigurationContext
    {
        public ConfigurationContext( IServiceCollection services, 
            List<AbstractModule> modules,
            string[] virtualPaths,
           IConfigurationRoot configuration)
        {
            Services = Check.NotNull(services, nameof(services));
            Modules = Check.NotNull(modules, nameof(modules));
            VirtualPaths = Check.NotNull(virtualPaths, nameof(virtualPaths));
            Configuration = Check.NotNull(configuration, nameof(configuration));
        }

        public IConfigurationRoot Configuration { get; }
        public IServiceCollection Services { get; }

        public List<AbstractModule> Modules { get; }

        public string[] VirtualPaths { get; }
    }
}
