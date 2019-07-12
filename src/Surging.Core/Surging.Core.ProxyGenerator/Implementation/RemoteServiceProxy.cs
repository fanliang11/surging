using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Convertibles;
using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Runtime.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.ProxyGenerator.Implementation
{
    /// <summary>
    /// Defines the <see cref="RemoteServiceProxy" />
    /// </summary>
    public class RemoteServiceProxy : ServiceProxyBase
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteServiceProxy"/> class.
        /// </summary>
        /// <param name="remoteInvokeService">The remoteInvokeService<see cref="IRemoteInvokeService"/></param>
        /// <param name="typeConvertibleService">The typeConvertibleService<see cref="ITypeConvertibleService"/></param>
        /// <param name="serviceKey">The serviceKey<see cref="String"/></param>
        /// <param name="serviceProvider">The serviceProvider<see cref="CPlatformContainer"/></param>
        /// <param name="serviceRouteProvider">The serviceRouteProvider<see cref="IServiceRouteProvider"/></param>
        public RemoteServiceProxy(IRemoteInvokeService remoteInvokeService,
            ITypeConvertibleService typeConvertibleService, String serviceKey,
            CPlatformContainer serviceProvider, IServiceRouteProvider serviceRouteProvider
            ) : base(remoteInvokeService, typeConvertibleService, serviceKey, serviceProvider)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteServiceProxy"/> class.
        /// </summary>
        /// <param name="serviceKey">The serviceKey<see cref="string"/></param>
        /// <param name="serviceProvider">The serviceProvider<see cref="CPlatformContainer"/></param>
        public RemoteServiceProxy(string serviceKey, CPlatformContainer serviceProvider)
           : this(serviceProvider.GetInstances<IRemoteInvokeService>(),
        serviceProvider.GetInstances<ITypeConvertibleService>(), serviceKey, serviceProvider,
        serviceProvider.GetInstances<IServiceRouteProvider>())
        {
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The Invoke
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameters">The parameters<see cref="IDictionary{string, object}"/></param>
        /// <param name="serviceId">The serviceId<see cref="string"/></param>
        /// <returns>The <see cref="Task{T}"/></returns>
        public new async Task<T> Invoke<T>(IDictionary<string, object> parameters, string serviceId)
        {
            return await base.Invoke<T>(parameters, serviceId);
        }

        #endregion 方法
    }
}