using Microsoft.AspNetCore.Hosting;
using Surging.Core.CPlatform.Module;
using Surging.Core.KestrelHttpServer;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Surging.Core.Stage
{
    public class StageModule : KestrelHttpModule
    {
        public override void Initialize(AppModuleContext context)
        {
        }

        public override void RegisterBuilder(WebHostContext context)
        {
            context.KestrelOptions.Listen(context.Address, 443, listOptions =>
             {
                 listOptions.UseHttps();
             });
        }

        public override void Initialize(ApplicationInitializationContext context)
        {
        }

        public override void RegisterBuilder(ConfigurationContext context)
        {
        }

        protected override void RegisterBuilder(ContainerBuilderWrapper builder)
        {
        }
    }
}
