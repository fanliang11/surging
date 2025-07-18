using Autofac;
using Microsoft.Extensions.Logging;
using SoapCore;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Module;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.KestrelHttpServer;
using Surging.Core.Protocol.WebService.Runtime;
using Surging.Core.Protocol.WebService.Runtime.Implementation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.ServiceModel;
using System.Xml;

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
            builder.Builder.UseRouting();
            builder.Builder.Use((context, next) => { context.Request.EnableBuffering(); return next(); });
            builder.Builder.UseEndpoints(endpoints =>
            {
                foreach (var webServiceEntry in webServiceEntries)
                {
                    endpoints.UseSoapEndpoint(webServiceEntry.BaseType, $"/{webServiceEntry.Path}.asmx", new SoapEncoderOptions()
                    {
                        ReaderQuotas = new XmlDictionaryReaderQuotas()
                        {
                            MaxStringContentLength = int.MaxValue,
                            MaxArrayLength = int.MaxValue,
                            MaxDepth = int.MaxValue
                        }
                    }, SoapSerializer.XmlSerializer);
                }
            });

        }


        public override void RegisterBuilder(ConfigurationContext context)
        {
            context.Services.AddSoapServiceOperationTuner(new ServiceOperationTuner());
            //  context.Services.AddSoapCore(); 
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