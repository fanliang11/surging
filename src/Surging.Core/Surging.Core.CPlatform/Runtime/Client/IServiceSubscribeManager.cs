using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Runtime.Client
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IServiceSubscribeManager" />
    /// </summary>
    public interface IServiceSubscribeManager
    {
        #region 方法

        /// <summary>
        /// 清空所有的服务订阅者。
        /// </summary>
        /// <returns>一个任务。</returns>
        Task ClearAsync();

        /// <summary>
        /// 获取所有可用的服务订阅者信息。
        /// </summary>
        /// <returns>服务路由集合。</returns>
        Task<IEnumerable<ServiceSubscriber>> GetSubscribersAsync();

        /// <summary>
        /// 设置服务订阅者。
        /// </summary>
        /// <param name="subscibers">The subscibers<see cref="IEnumerable{ServiceSubscriber}"/></param>
        /// <returns>一个任务。</returns>
        Task SetSubscribersAsync(IEnumerable<ServiceSubscriber> subscibers);

        #endregion 方法
    }

    #endregion 接口

    /// <summary>
    /// 服务路由管理者扩展方法。
    /// </summary>
    public static class ServiceRouteManagerExtensions
    {
        #region 方法

        /// <summary>
        /// 获取地址
        /// </summary>
        /// <param name="serviceSubscribeManager">The serviceSubscribeManager<see cref="IServiceSubscribeManager"/></param>
        /// <param name="condition">The condition<see cref="string"/></param>
        /// <returns></returns>
        public static async Task<IEnumerable<AddressModel>> GetAddressAsync(this IServiceSubscribeManager serviceSubscribeManager, string condition = null)
        {
            var subscribers = await serviceSubscribeManager.GetSubscribersAsync();
            if (condition != null)
            {
                if (!condition.IsIP())
                {
                    subscribers = subscribers.Where(p => p.ServiceDescriptor.Id == condition);
                }
                else
                {
                    subscribers = subscribers.Where(p => p.Address.Any(m => m.ToString() == condition));
                }
            }
            Dictionary<string, AddressModel> result = new Dictionary<string, AddressModel>();
            foreach (var route in subscribers)
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

        /// <summary>
        /// 根据服务Id获取一个服务订阅者。
        /// </summary>
        /// <param name="serviceSubscribeManager">The serviceSubscribeManager<see cref="IServiceSubscribeManager"/></param>
        /// <param name="serviceId">服务Id。</param>
        /// <returns>服务路由。</returns>
        public static async Task<ServiceSubscriber> GetAsync(this IServiceSubscribeManager serviceSubscribeManager, string serviceId)
        {
            return (await serviceSubscribeManager.GetSubscribersAsync()).SingleOrDefault(i => i.ServiceDescriptor.Id == serviceId);
        }

        /// <summary>
        /// The GetServiceDescriptorAsync
        /// </summary>
        /// <param name="serviceSubscribeManager">The serviceSubscribeManager<see cref="IServiceSubscribeManager"/></param>
        /// <param name="address">The address<see cref="string"/></param>
        /// <param name="serviceId">The serviceId<see cref="string"/></param>
        /// <returns>The <see cref="Task{IEnumerable{ServiceDescriptor}}"/></returns>
        public static async Task<IEnumerable<ServiceDescriptor>> GetServiceDescriptorAsync(this IServiceSubscribeManager serviceSubscribeManager, string address, string serviceId = null)
        {
            var subscribers = await serviceSubscribeManager.GetSubscribersAsync();
            if (serviceId == null)
            {
                return subscribers.Where(p => p.Address.Any(m => m.ToString() == address))
                 .Select(p => p.ServiceDescriptor);
            }
            else
            {
                return subscribers.Where(p => p.ServiceDescriptor.Id == serviceId)
               .Select(p => p.ServiceDescriptor);
            }
        }

        #endregion 方法
    }
}