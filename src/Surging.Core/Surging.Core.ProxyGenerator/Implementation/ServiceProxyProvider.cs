using Surging.Core.CPlatform;
using Surging.Core.CPlatform.DependencyResolution;
using Surging.Core.CPlatform.Routing;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Surging.Core.ProxyGenerator.Implementation
{
    /// <summary>
    /// Defines the <see cref="ServiceProxyProvider" />
    /// </summary>
    public class ServiceProxyProvider : IServiceProxyProvider
    {
        #region 字段

        /// <summary>
        /// Defines the _serviceProvider
        /// </summary>
        private readonly CPlatformContainer _serviceProvider;

        /// <summary>
        /// Defines the _serviceRouteProvider
        /// </summary>
        private readonly IServiceRouteProvider _serviceRouteProvider;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProxyProvider"/> class.
        /// </summary>
        /// <param name="serviceRouteProvider">The serviceRouteProvider<see cref="IServiceRouteProvider"/></param>
        /// <param name="serviceProvider">The serviceProvider<see cref="CPlatformContainer"/></param>
        public ServiceProxyProvider(IServiceRouteProvider serviceRouteProvider
            , CPlatformContainer serviceProvider)
        {
            _serviceRouteProvider = serviceRouteProvider;
            _serviceProvider = serviceProvider;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The Invoke
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameters">The parameters<see cref="IDictionary{string, object}"/></param>
        /// <param name="routePath">The routePath<see cref="string"/></param>
        /// <returns>The <see cref="Task{T}"/></returns>
        public async Task<T> Invoke<T>(IDictionary<string, object> parameters, string routePath)
        {
            var serviceRoute = await _serviceRouteProvider.GetRouteByPath(routePath.ToLower());
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

        /// <summary>
        /// The Invoke
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameters">The parameters<see cref="IDictionary{string, object}"/></param>
        /// <param name="routePath">The routePath<see cref="string"/></param>
        /// <param name="serviceKey">The serviceKey<see cref="string"/></param>
        /// <returns>The <see cref="Task{T}"/></returns>
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

        #endregion 方法
    }
}