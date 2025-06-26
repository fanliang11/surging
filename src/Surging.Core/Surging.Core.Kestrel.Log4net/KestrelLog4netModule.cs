using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Module;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.KestrelHttpServer;
using Surging.Core.Log4net;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Kestrel.Log4net
{
    
    public class KestrelLog4netModule : KestrelHttpModule
    {
        private string log4NetConfigFile = "${LogPath}|log4net.config";
        public override void Initialize(AppModuleContext context)
        {

        }

        public override void RegisterBuilder(WebHostContext context)
        {
        }

        public override void Initialize(ApplicationInitializationContext context)
        {
            var serviceProvider = context.Builder.ApplicationServices;
            base.Initialize(context);
            var section = CPlatform.AppConfig.GetSection("Logging");
            log4NetConfigFile = EnvironmentHelper.GetEnvironmentVariable(log4NetConfigFile);
            serviceProvider.GetService<ILoggerFactory>().AddProvider(new Log4NetProvider(log4NetConfigFile));
        }

        public override void RegisterBuilder(ConfigurationContext context)
        {
            context.Services.AddLogging();
        }

        protected override void RegisterBuilder(ContainerBuilderWrapper builder)
        {

        }
    }
}
