using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Runtime.Client.Implementation
{
    public abstract  class ServiceSubscribeManagerBase : IServiceSubscribeManager
    { 
        private readonly ISerializer<string> _serializer; 

        protected ServiceSubscribeManagerBase(ISerializer<string> serializer)
        {
            _serializer = serializer;
        }

        #region Implementation of IServiceRouteManager


        /// <summary>
        /// 获取所有服务订阅者信息。
        /// </summary>
        /// <returns>服务路由集合。</returns>
        public abstract Task<IEnumerable<ServiceSubscriber>> GetSubscribersAsync();

        /// <summary>
        /// 设置服务订阅者。
        /// </summary>
        /// <param name="routes">服务路由集合。</param>
        /// <returns>一个任务。</returns>
        public virtual Task SetSubscribersAsync(IEnumerable<ServiceSubscriber> subscibers)
        {
            if (subscibers == null)
                throw new ArgumentNullException(nameof(subscibers));

            var descriptors = subscibers.Where(route => route != null).Select(route => new ServiceSubscriberDescriptor
            {
                AddressDescriptors = route.Address?.Select(address => new ServiceAddressDescriptor
                {
                    Type = address.GetType().FullName,
                    Value = _serializer.Serialize(address)
                }) ?? Enumerable.Empty<ServiceAddressDescriptor>(),
                ServiceDescriptor = route.ServiceDescriptor
            });

            return SetSubscribersAsync(descriptors);
        }

        /// <summary>
        /// 清空所有的服务订阅者。
        /// </summary>
        /// <returns>一个任务。</returns>
        public abstract Task ClearAsync();

        #endregion Implementation of IServiceSubscriberManager

        /// <summary>
        /// 设置服务订阅者。
        /// </summary>
        /// <param name="routes">服务订阅者集合。</param>
        /// <returns>一个任务。</returns>
        protected abstract Task SetSubscribersAsync(IEnumerable<ServiceSubscriberDescriptor> subscribers);

     
    }
}
