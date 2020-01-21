using Com.Ctrip.Framework.Apollo;
using Com.Ctrip.Framework.Apollo.Enums;
using Microsoft.Extensions.Configuration;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Configurations;
using Surging.Core.CPlatform.Module;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Surging.Core.Apollo
{
    //todo:是否需要用module加载apollo?
    public class ApolloModule : EnginePartModule
    {
        public override void Initialize(AppModuleContext context)
        {
            var serviceProvider = context.ServiceProvoider;
            base.Initialize(context);

            var builder = serviceProvider.GetInstances<IConfigurationBuilder>();

            var section = AppConfig.GetSection("apollo");
            if (!section.Exists())
            {
                if (!string.IsNullOrEmpty(AppConfig.ServerOptions.RootPath))
                {
                    var skyapmPath = Path.Combine(AppConfig.ServerOptions.RootPath, "apollo.json");

                    builder.AddCPlatformFile(skyapmPath, optional: false, reloadOnChange: true);
                }

                builder.AddCPlatformFile("${apollopath}|apollo.json", optional: false, reloadOnChange: true);
            }

            var config = builder.Build();
            section = config.GetSection("apollo");
            if (!section.Exists())
            {
                throw new Exception("apollo config file not exists!");
            }

            builder.AddApollo(section)

                .AddNamespaceSurgingApollo("surgingSettings", ConfigFileFormat.Json);

            AppConfig.Configuration = builder.Build();
            AppConfig.ServerOptions = AppConfig.Configuration.Get<SurgingServerOptions>();
            var surgingSection = AppConfig.Configuration.GetSection("Surging");
            if (surgingSection.Exists())
                AppConfig.ServerOptions = AppConfig.Configuration.GetSection("Surging").Get<SurgingServerOptions>();
        }

        protected override void RegisterBuilder(ContainerBuilderWrapper builder)
        {
           
        }
    }
}
