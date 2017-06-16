using Microsoft.Extensions.DependencyInjection;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Runtime.Server.Implementation;
using Surging.Core.CPlatform.Transport;
using Autofac;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Transport.Codec;

namespace Surging.Core.DotNetty
{
   public static class ContainerBuilderExtensions
    {
        /// <summary>
        /// 使用DotNetty进行传输。
        /// </summary>
        /// <param name="builder">服务构建者。</param>
        /// <returns>服务构建者。</returns>
        public static IServiceBuilder UseDotNettyTransport(this IServiceBuilder builder)
        {
            var services = builder.Services;
            services.RegisterType(typeof(DotNettyTransportClientFactory)).As(typeof(ITransportClientFactory)).SingleInstance();
            services.Register(provider => {
              return  new DotNettyServerMessageListener(provider.Resolve<ILogger<DotNettyServerMessageListener>>(),
                    provider.Resolve<ITransportMessageCodecFactory>());
            }).SingleInstance();
            services.Register(provider =>
            {
                var messageListener = provider.Resolve<DotNettyServerMessageListener>();
                var serviceExecutor = provider.Resolve<IServiceExecutor>();
                return new DefaultServiceHost(async endPoint =>
                {
                    await messageListener.StartAsync(endPoint);
                    return messageListener;
                }, serviceExecutor);
                }).As<IServiceHost>(); 

            return builder;
        }
    }
}