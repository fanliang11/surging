using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Routing.Implementation;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Routing
{
    /// <summary>
    /// 一个抽象的服务路由发现者。
    /// </summary>
    public interface IServiceRouteManager
    {

        /// <summary>
        /// 服务路由被创建。
        /// </summary>
        event EventHandler<ServiceRouteEventArgs> Created;

        /// <summary>
        /// 服务路由被删除。
        /// </summary>
        event EventHandler<ServiceRouteEventArgs> Removed;

        /// <summary>
        /// 服务路由被修改。
        /// </summary>
        event EventHandler<ServiceRouteChangedEventArgs> Changed;

        /// <summary>
        /// 获取所有可用的服务路由信息。
        /// </summary>
        /// <returns>服务路由集合。</returns>
        Task<IEnumerable<ServiceRoute>> GetRoutesAsync();

        /// <summary>
        /// 设置服务路由。
        /// </summary>
        /// <param name="routes">服务路由集合。</param>
        /// <returns>一个任务。</returns>
        Task SetRoutesAsync(IEnumerable<ServiceRoute> routes);

        /// <summary>
        /// 移除地址列表
        /// </summary>
        /// <param name="routes">地址列表。</param>
        /// <returns>一个任务。</returns>
        Task RemveAddressAsync(IEnumerable<AddressModel> Address);
        /// <summary>
        /// 清空所有的服务路由。
        /// </summary>
        /// <returns>一个任务。</returns>
        Task ClearAsync();
    }

    /// <summary>
    /// 服务路由管理者扩展方法。
    /// </summary>
    public static class ServiceRouteManagerExtensions
    {
        /// <summary>
        /// 根据服务Id获取一个服务路由。
        /// </summary>
        /// <param name="serviceRouteManager">服务路由管理者。</param>
        /// <param name="serviceId">服务Id。</param>
        /// <returns>服务路由。</returns>
        public static async Task<ServiceRoute> GetAsync(this IServiceRouteManager serviceRouteManager, string serviceId)
        {
            return (await serviceRouteManager.GetRoutesAsync()).SingleOrDefault(i => i.ServiceDescriptor.Id == serviceId);
        }

       

        /// <summary>
        /// 获取地址
        /// </summary>
        /// <returns></returns>
        public static async Task<IEnumerable<AddressModel>> GetAddressAsync(this IServiceRouteManager serviceRouteManager, string condition = null)
        {
            var routes = await serviceRouteManager.GetRoutesAsync();
            Dictionary<string, AddressModel> result = new Dictionary<string, AddressModel>();
            if (condition != null)
            {
                if (!condition.IsIP())
                {
                    routes = routes.Where(p => p.ServiceDescriptor.Id == condition);
                }
                else
                {
                    routes = routes.Where(p => p.Address.Any(m => m.ToString() == condition));
                    var addresses = routes.FirstOrDefault().Address;
                    return addresses.Where(p => p.ToString() == condition);
                }
            }

            foreach (var route in routes)
            {
                var addresses = route.Address;
                foreach (var address in addresses)
                {
                    if (!result.ContainsKey(address.ToString()))
                    {
                        result.Add(address.ToString(), address);
                    }
                }
            }
            return result.Values;
        }

        public static async Task<IEnumerable<ServiceRoute>> GetRoutesAsync(this IServiceRouteManager serviceRouteManager, string address)
        {
            var routes = await serviceRouteManager.GetRoutesAsync();
            return routes.Where(p => p.Address.Any(m => m.ToString() == address));
        }

        public static async Task<IEnumerable<ServiceDescriptor>> GetServiceDescriptorAsync(this IServiceRouteManager serviceRouteManager, string address, string serviceId = null)
        {
            var routes = await serviceRouteManager.GetRoutesAsync();
            if (serviceId == null)
            {
                return routes.Where(p => p.Address.Any(m => m.ToString() == address))
                 .Select(p => p.ServiceDescriptor);
            }
            else
            {
                return routes.Where(p => p.ServiceDescriptor.Id == serviceId)
               .Select(p => p.ServiceDescriptor);
            }
        }
    }
}