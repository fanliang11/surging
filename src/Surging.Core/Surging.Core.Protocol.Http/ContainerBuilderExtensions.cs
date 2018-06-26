using Autofac;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Runtime.Server.Implementation;
using Surging.Core.CPlatform.Serialization;
using Surging.Core.CPlatform.Transport.Codec;

namespace Surging.Core.Protocol.Http
{
    public static  class ContainerBuilderExtensions
    {
        /// <summary>
        /// 添加http协议
        /// </summary>
        /// <param name="builder">服务构建者。</param>
        /// <returns>服务构建者。</returns>
        public static IServiceBuilder AddHttpProtocol(this IServiceBuilder builder)
        {
            var services = builder.Services;
            if (AppConfig.ServerOptions.Protocol == CommunicationProtocol.Http)
            {
                builder.Services.RegisterType(typeof(HttpServiceExecutor)).As(typeof(IServiceExecutor)).SingleInstance();
                services.Register(provider =>
                {
                    return new DotNettyHttpServerMessageListener(provider.Resolve<ILogger<DotNettyHttpServerMessageListener>>(),
                          provider.Resolve<ITransportMessageCodecFactory>(),
                          provider.Resolve<ISerializer<string>>()
                          );
                }).SingleInstance();
                services.Register(provider =>
                {

                    var serviceExecutor = provider.Resolve<IServiceExecutor>();
                    var messageListener = provider.Resolve<DotNettyHttpServerMessageListener>();
                    return new DefaultServiceHost(async endPoint =>
                {
                            await messageListener.StartAsync(endPoint);
                            return messageListener;
                        }, serviceExecutor);

                }).As<IServiceHost>();
            }
            return builder;
        }
    }
}
