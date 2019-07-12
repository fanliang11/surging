using Autofac;
using Surging.Core.ServiceHosting.Internal;

namespace Surging.Core.AutoMapper
{
    /// <summary>
    /// Defines the <see cref="ServiceHostBuilderExtensions" />
    /// </summary>
    public static class ServiceHostBuilderExtensions
    {
        #region 方法

        /// <summary>
        /// The UserAutoMapper
        /// </summary>
        /// <param name="hostBuilder">The hostBuilder<see cref="IServiceHostBuilder"/></param>
        /// <returns>The <see cref="IServiceHostBuilder"/></returns>
        public static IServiceHostBuilder UserAutoMapper(this IServiceHostBuilder hostBuilder)
        {
            return hostBuilder.MapServices(mapper =>
            {
                var autoMapperBootstrap = mapper.Resolve<IAutoMapperBootstrap>();
                autoMapperBootstrap.Initialize();
            });
        }

        #endregion 方法
    }
}