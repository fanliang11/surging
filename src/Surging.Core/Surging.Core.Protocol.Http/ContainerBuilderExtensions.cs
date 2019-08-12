using Autofac;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Runtime.Server.Implementation;
using Surging.Core.CPlatform.Serialization;
using Surging.Core.CPlatform.Transport.Codec;

namespace Surging.Core.Protocol.Http
{
    public static class ContainerBuilderExtensions
    {
        /// <summary>
        /// 添加http协议
        /// </summary>
        /// <param name="builder">服务构建者。</param>
        /// <returns>服务构建者。</returns>
        public static IServiceBuilder AddHttpProtocol(this IServiceBuilder builder)
        {
            var services = builder.Services;
            services.RegisterType(typeof(HttpServiceExecutor)).As(typeof(IServiceExecutor))
                .Named<IServiceExecutor>(CommunicationProtocol.Http.ToString()).SingleInstance();
            if (AppConfig.ServerOptions.Protocol == CommunicationProtocol.Http)
            {
                RegisterDefaultProtocol(services);
            }
            else if (AppConfig.ServerOptions.Protocol == CommunicationProtocol.None)
            {
                RegisterHttpProtocol(services);
            }
            return builder;
        }

        private static void RegisterDefaultProtocol(ContainerBuilder builder)
        {
            builder.Register(provider =>
            {
                return new DotNettyHttpServerMessageListener(provider.Resolve<ILogger<DotNettyHttpServerMessageListener>>(),
                      provider.Resolve<ITransportMessageCodecFactory>(),
                      provider.Resolve<ISerializer<string>>(),
                      provider.Resolve<IServiceRouteProvider>()
                      );
            }).SingleInstance();
            builder.Register(provider =>
            {

                var serviceExecutor = provider.ResolveKeyed<IServiceExecutor>(CommunicationProtocol.Http.ToString());
                var messageListener = provider.Resolve<DotNettyHttpServerMessageListener>();
                return new DefaultServiceHost(async endPoint =>
                {
                    await messageListener.StartAsync(endPoint);
                    return messageListener;
                }, serviceExecutor);

            }).As<IServiceHost>();
        }

        private static void RegisterHttpProtocol(ContainerBuilder builder)
        {

            builder.Register(provider =>
            {
                return new DotNettyHttpServerMessageListener(provider.Resolve<ILogger<DotNettyHttpServerMessageListener>>(),
                      provider.Resolve<ITransportMessageCodecFactory>(),
                      provider.Resolve<ISerializer<string>>(),
                      provider.Resolve<IServiceRouteProvider>()
                      );
            }).SingleInstance();
            builder.Register(provider =>
            {
                var serviceExecutor = provider.ResolveKeyed<IServiceExecutor>(CommunicationProtocol.Http.ToString());
                var messageListener = provider.Resolve<DotNettyHttpServerMessageListener>();
                return new HttpServiceHost(async endPoint =>
                {
                    await messageListener.StartAsync(endPoint);
                    return messageListener;
                }, serviceExecutor);

            }).As<IServiceHost>();
        }
    }
}
