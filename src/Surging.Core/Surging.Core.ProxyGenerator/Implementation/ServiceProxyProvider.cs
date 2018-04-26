using Surging.Core.CPlatform;
using Surging.Core.CPlatform.DependencyResolution;
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
            if (parameters.ContainsKey("serviceKey"))
            {
                var serviceKey = parameters["serviceKey"].ToString();
                var proxy = ServiceResolver.Current.GetService<RemoteServiceProxy>(serviceKey);
                if (proxy == null)
                {
                     proxy = new RemoteServiceProxy(serviceKey.ToString(), _serviceProvider);
                    ServiceResolver.Current.Register(serviceKey.ToString(), proxy);
                }
                result = await proxy.Invoke<T>(parameters, serviceRoute.ServiceDescriptor.Id);
                    
            }
            else
            {
                var proxy = ServiceResolver.Current.GetService<RemoteServiceProxy>();
                if (proxy == null)
                {
                     proxy = new RemoteServiceProxy(null, _serviceProvider);
                    ServiceResolver.Current.Register(null, proxy);
                }
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
                var proxy = ServiceResolver.Current.GetService<RemoteServiceProxy>(serviceKey);
                if (proxy == null)
                {
                    proxy = new RemoteServiceProxy(serviceKey, _serviceProvider);
                    ServiceResolver.Current.Register(serviceKey, proxy);
                }
                result = await proxy.Invoke<T>(parameters, serviceRoute.ServiceDescriptor.Id);
            }
            else
            {
                var proxy = ServiceResolver.Current.GetService<RemoteServiceProxy>();
                if (proxy == null)
                {
                    proxy = new RemoteServiceProxy(null, _serviceProvider);
                    ServiceResolver.Current.Register(null, proxy);
                }
                result = await proxy.Invoke<T>(parameters, serviceRoute.ServiceDescriptor.Id);
            }
            return result;
        }
    }
}
