using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Surging.Core.CPlatform.Module;
using Surging.Core.KestrelHttpServer;
using Surging.Core.KestrelHttpServer.Extensions;
using Surging.Core.Protokollwandler.Configurations;
using Surging.Core.Protokollwandler.Filters;
using Surging.Core.Protokollwandler.Internal;
using Surging.Core.Protokollwandler.Internal.Http;
using Surging.Core.Protokollwandler.Internal.Implementation;
using Surging.Core.Protokollwandler.Internal.WebService;
using Surging.Core.Protokollwandler.Metadatas;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protokollwandler
{
    public class ProtokollwandlerModule : KestrelHttpModule
    {
        public override void Initialize(AppModuleContext context)
        { 
        }

        public override void Initialize(ApplicationInitializationContext context)
        { 
        }

        public override void RegisterBuilder(ConfigurationContext context)
        {
            context.Services.AddHttpClient();
            context.Services.AddFilters(typeof(ActionFilterAttribute));
        }

        protected override void RegisterBuilder(ContainerBuilderWrapper builder)
        {
            CPlatform.AppConfig.ServerOptions.DisableServiceRegistration = true;
            var section = CPlatform.AppConfig.GetSection("TransferContract");
            if (section.Exists())
            {
                AppConfig.Options = section.Get<List<TransferContractOption>>();
            }
            builder.RegisterType<HttpTransportClient>().Named<ITransportClient>(TransferContractType.Rest.ToString()).SingleInstance();
            builder.RegisterType<WebServiceTransportClient>().Named<ITransportClient>(TransferContractType.WebService.ToString()).SingleInstance();
            builder.RegisterType<SoapWebServiceTransportClient>().Named<ITransportClient>(TransferContractType.SoapWebService.ToString()).SingleInstance();
            builder.RegisterType<HttpClientProvider>().As<IHttpClientProvider>().SingleInstance();
            builder.RegisterType<WebServiceProvider>().As<IWebServiceProvider>().SingleInstance();
            builder.RegisterType<MessageSender>().As<IMessageSender>().SingleInstance();
        }
    }
}
