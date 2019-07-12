using Autofac;
using Surging.Core.CPlatform.EventBus;
using Surging.Core.ServiceHosting.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.EventBusKafka
{
    /// <summary>
    /// Defines the <see cref="ServiceHostBuilderExtensions" />
    /// </summary>
    public static class ServiceHostBuilderExtensions
    {
        #region 方法

        /// <summary>
        /// The SubscribeAt
        /// </summary>
        /// <param name="hostBuilder">The hostBuilder<see cref="IServiceHostBuilder"/></param>
        /// <returns>The <see cref="IServiceHostBuilder"/></returns>
        public static IServiceHostBuilder SubscribeAt(this IServiceHostBuilder hostBuilder)
        {
            return hostBuilder.MapServices(mapper =>
            {
                mapper.Resolve<ISubscriptionAdapt>().SubscribeAt();
            });
        }

        #endregion 方法
    }
}