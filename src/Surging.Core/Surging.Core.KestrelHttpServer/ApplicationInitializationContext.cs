using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Surging.Core.CPlatform.Module;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.KestrelHttpServer
{
    public class ApplicationInitializationContext
    {
        public ApplicationInitializationContext(IApplicationBuilder builder,
    List<AbstractModule> modules,
    string[] virtualPaths,
   IConfigurationRoot configuration)
        {
            Builder = Check.NotNull(builder, nameof(builder));
            Modules = Check.NotNull(modules, nameof(modules));
            VirtualPaths = Check.NotNull(virtualPaths, nameof(virtualPaths));
            Configuration = Check.NotNull(configuration, nameof(configuration));
        }

        public IApplicationBuilder Builder { get; }

        public IConfigurationRoot Configuration { get; }

        public List<AbstractModule> Modules { get; }

        public string[] VirtualPaths { get; }
    }
}
