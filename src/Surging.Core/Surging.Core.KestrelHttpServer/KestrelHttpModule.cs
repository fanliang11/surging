using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Diagnostics;
using Surging.Core.CPlatform.Engines;
using Surging.Core.CPlatform.Module;
using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Serialization;
using Surging.Core.KestrelHttpServer.Diagnostics;
using Surging.Core.KestrelHttpServer.Extensions;
using Surging.Core.KestrelHttpServer.Filters;
using Surging.Core.KestrelHttpServer.Filters.Implementation;
using System.Net;

namespace Surging.Core.KestrelHttpServer
{
    public class KestrelHttpModule : EnginePartModule
    {
        public override void Initialize(AppModuleContext context)
        {
            base.Initialize(context);
        }

        public virtual void Initialize(ApplicationInitializationContext builder)
        {
        }

        public virtual void RegisterBuilder(WebHostContext context)
        {
        }

        public virtual void RegisterBuilder(ConfigurationContext context)
        {
            context.Services.AddFilters(typeof(HttpRequestFilterAttribute));
            context.Services.AddFilters(typeof(CustomerExceptionFilterAttribute));
        }

        /// <summary>
        /// Inject dependent third-party components
        /// </summary>
        /// <param name="builder"></param>
        protected override void RegisterBuilder(ContainerBuilderWrapper builder)
        {
            base.RegisterBuilder(builder);
            builder.AddFilter(typeof(ServiceExceptionFilter));
            builder.RegisterType<RestTransportDiagnosticProcessor>().As<ITracingDiagnosticProcessor>().SingleInstance();
            builder.RegisterType(typeof(HttpExecutor)).As(typeof(IServiceExecutor))
                .Named<IServiceExecutor>(CommunicationProtocol.Http.ToString()).SingleInstance();
            if (CPlatform.AppConfig.ServerOptions.Protocol == CommunicationProtocol.Http)
            {
                RegisterDefaultProtocol(builder);
            }
            else if (CPlatform.AppConfig.ServerOptions.Protocol == CommunicationProtocol.None)
            {
                RegisterHttpProtocol(builder);
            }
        }

        private static void RegisterDefaultProtocol(ContainerBuilderWrapper builder)
        {
            builder.Register(provider =>
            {
                return new KestrelHttpMessageListener(
                    provider.Resolve<ILogger<KestrelHttpMessageListener>>(),
                    provider.Resolve<ISerializer<string>>(),
                     provider.Resolve<IServiceEngineLifetime>(),
                     provider.Resolve<IModuleProvider>(),
                    provider.Resolve<IServiceRouteProvider>(),
                     provider.Resolve<CPlatformContainer>()
                      );
            }).SingleInstance();
            builder.Register(provider =>
            {
                var executor = provider.ResolveKeyed<IServiceExecutor>(CommunicationProtocol.Http.ToString());
                var messageListener = provider.Resolve<KestrelHttpMessageListener>();
                return new DefaultHttpServiceHost(async endPoint =>
                {
                    var address = endPoint as IPEndPoint;
                    await messageListener.StartAsync(address?.Address, address?.Port);
                    return messageListener;
                }, executor, messageListener);

            }).As<IServiceHost>();
        }

        private static void RegisterHttpProtocol(ContainerBuilderWrapper builder)
        {
            builder.Register(provider =>
            {
                return new KestrelHttpMessageListener(
                    provider.Resolve<ILogger<KestrelHttpMessageListener>>(),
                    provider.Resolve<ISerializer<string>>(),
                     provider.Resolve<IServiceEngineLifetime>(),
                       provider.Resolve<IModuleProvider>(),
                       provider.Resolve<IServiceRouteProvider>(),
                     provider.Resolve<CPlatformContainer>()

                      );
            }).SingleInstance();
            builder.Register(provider =>
            {
                var executor = provider.ResolveKeyed<IServiceExecutor>(CommunicationProtocol.Http.ToString());
                var messageListener = provider.Resolve<KestrelHttpMessageListener>();
                return new HttpServiceHost(async endPoint =>
                {
                    var address = endPoint as IPEndPoint;
                    await messageListener.StartAsync(address?.Address, address?.Port);
                    return messageListener;
                }, executor, messageListener);

            }).As<IServiceHost>();
        }
    }
}