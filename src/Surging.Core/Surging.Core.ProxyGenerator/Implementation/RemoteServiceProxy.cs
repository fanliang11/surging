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
    public class RemoteServiceProxy : ServiceProxyBase
    {
        /// <summary>
        /// 远程服务代理，通过RoutePath调用服务时使用
        /// 由ServiceProxyProvider调用
        /// 通过执行基类的Invoke函数实现远程服务调用
        /// </summary>
        public RemoteServiceProxy(string serviceKey, CPlatformContainer serviceProvider)
           : this(serviceProvider.GetInstances<IRemoteInvokeService>(),
        serviceProvider.GetInstances<ITypeConvertibleService>(), serviceKey, serviceProvider,
        serviceProvider.GetInstances<IServiceRouteProvider>())
        {

        }

        public RemoteServiceProxy(IRemoteInvokeService remoteInvokeService,
            ITypeConvertibleService typeConvertibleService, String serviceKey,
            CPlatformContainer serviceProvider, IServiceRouteProvider serviceRouteProvider
            ) : base(remoteInvokeService, typeConvertibleService, serviceKey, serviceProvider)
        {

        }

        public new async Task<T> Invoke<T>(IDictionary<string, object> parameters, string serviceId)
        {
            return await base.Invoke<T>(parameters, serviceId);
        }

    }
}
