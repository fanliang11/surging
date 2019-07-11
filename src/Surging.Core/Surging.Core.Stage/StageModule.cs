using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using Surging.Core.CPlatform.Module;
using Surging.Core.KestrelHttpServer;
using Surging.Core.KestrelHttpServer.Filters;
using Surging.Core.Stage.Configurations;
using Surging.Core.Stage.Internal;
using Surging.Core.Stage.Internal.Implementation;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Surging.Core.Stage
{
    public class StageModule : KestrelHttpModule
    {
        private IWebServerListener _listener;
        public override void Initialize(AppModuleContext context)
        {
            _listener = context.ServiceProvoider.GetInstances<IWebServerListener>();
        }

        public override void RegisterBuilder(WebHostContext context)
        {
            _listener.Listen(context);
        }

        public override void Initialize(ApplicationInitializationContext context)
        {
        }

        public override void RegisterBuilder(ConfigurationContext context)
        {
       
        }

        protected override void RegisterBuilder(ContainerBuilderWrapper builder)
        {
            var section = CPlatform.AppConfig.GetSection("Stage");
            if (section.Exists())
            {
                AppConfig.Options = section.Get<StageOption>();
            }
            builder.RegisterType<WebServerListener>().As<IWebServerListener>().SingleInstance();
            builder.RegisterType<AuthorizationFilterAttribute>().As<IAuthorizationFilter>().SingleInstance();
        }
    }
}
