using Autofac;
using Microsoft.Extensions.Logging;
using SoapCore;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Module;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.KestrelHttpServer;
using Surging.Core.Protocol.WebService.Runtime;
using Surging.Core.Protocol.WebService.Runtime.Implementation;
using System.ServiceModel;

namespace Surging.Core.Protocol.WebService
{
    public class WebServiceModule : KestrelHttpModule
    {
        private IWebServiceEntryProvider _webServiceEntryProvider;
        public override void Initialize(AppModuleContext context)
        {
            _webServiceEntryProvider = context.ServiceProvoider.GetInstances<IWebServiceEntryProvider>();
        }

        public override void Initialize(ApplicationInitializationContext builder)
        {
            var webServiceEntries = _webServiceEntryProvider.GetEntries();
            var binging = new BasicHttpBinding();
            binging.ReaderQuotas.MaxStringContentLength = int.MaxValue;
            foreach (var webServiceEntry in webServiceEntries)
                builder.Builder.UseSoapEndpoint(webServiceEntry.BaseType, $"/{webServiceEntry.Path}.asmx", new SoapEncoderOptions(), SoapSerializer.XmlSerializer);
        }


        public override void RegisterBuilder(ConfigurationContext context)
        {
            context.Services.AddSoapCore(); 
        }

        protected async override void RegisterBuilder(ContainerBuilderWrapper builder)
        {
            builder.Register(provider =>
            {
                return new DefaultWebServiceEntryProvider(
                       provider.Resolve<IServiceEntryProvider>(),
                    provider.Resolve<ILogger<DefaultWebServiceEntryProvider>>(),
                      provider.Resolve<CPlatformContainer>() 
                      );
            }).As(typeof(IWebServiceEntryProvider)).SingleInstance();
        }
    }
}