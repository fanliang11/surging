using Autofac;
using Surging.Core.CPlatform.Engines;
using Surging.Core.ServiceHosting.Internal;

namespace Surging.Core.ProxyGenerator
{
    /// <summary>
    /// Defines the <see cref="ServiceHostBuilderExtensions" />
    /// </summary>
    public static class ServiceHostBuilderExtensions
    {
        #region 方法

        /// <summary>
        /// The UseProxy
        /// </summary>
        /// <param name="hostBuilder">The hostBuilder<see cref="IServiceHostBuilder"/></param>
        /// <returns>The <see cref="IServiceHostBuilder"/></returns>
        public static IServiceHostBuilder UseProxy(this IServiceHostBuilder hostBuilder)
        {
            return hostBuilder.MapServices(mapper =>
            {
                mapper.Resolve<IServiceEngineLifetime>().ServiceEngineStarted.Register(() =>
                 {
                     mapper.Resolve<IServiceProxyFactory>();
                 });
            });
        }

        #endregion 方法
    }
}