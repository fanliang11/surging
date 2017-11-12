using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Routing;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Surging.Core.ProxyGenerator.Implementation
{
    public class ServiceProxyProvider : IServiceProxyProvider
    {
        private readonly IServiceRouteProvider _serviceRouteProvider;
        private readonly CPlatformContainer _serviceProvider;
        public ServiceProxyProvider( IServiceRouteProvider serviceRouteProvider
            , CPlatformContainer serviceProvider)
        {
            _serviceRouteProvider = serviceRouteProvider;
            _serviceProvider = serviceProvider;
        }

        public  async Task<T> Invoke<T>(IDictionary<string, object> parameters, string routePath)
        {
           var serviceRoute= await _serviceRouteProvider.GetRouteByPath(routePath.ToLower());
            T result = default(T);
            if( parameters.ContainsKey("serviceKey"))
            {
                var proxy= new RemoteServiceProxy(parameters["serviceKey"].ToString(), _serviceProvider);
                result = await proxy.Invoke<T>(parameters, serviceRoute.ServiceDescriptor.Id);
            }
            else
            {
                var proxy = new RemoteServiceProxy(null, _serviceProvider);
                result = await proxy.Invoke<T>(parameters, serviceRoute.ServiceDescriptor.Id);
            }
            return result;
        }

        public async Task<T> Invoke<T>(IDictionary<string, object> parameters, string routePath, string serviceKey)
        {
            var serviceRoute = await _serviceRouteProvider.GetRouteByPath(routePath.ToLower());
            T result = default(T);
            if (!string.IsNullOrEmpty(serviceKey))
            {
                var proxy = new RemoteServiceProxy(serviceKey, _serviceProvider);
                result = await proxy.Invoke<T>(parameters, serviceRoute.ServiceDescriptor.Id);
            }
            else
            {
                var proxy = new RemoteServiceProxy(null, _serviceProvider);
                result = await proxy.Invoke<T>(parameters, serviceRoute.ServiceDescriptor.Id);
            }
            return result;
        }
    }
}
