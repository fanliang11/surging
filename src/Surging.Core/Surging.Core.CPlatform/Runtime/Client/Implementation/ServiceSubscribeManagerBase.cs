using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Runtime.Client.Implementation
{
    /// <summary>
    /// Defines the <see cref="ServiceSubscribeManagerBase" />
    /// </summary>
    public abstract class ServiceSubscribeManagerBase : IServiceSubscribeManager
    {
        #region 字段

        /// <summary>
        /// Defines the _serializer
        /// </summary>
        private readonly ISerializer<string> _serializer;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceSubscribeManagerBase"/> class.
        /// </summary>
        /// <param name="serializer">The serializer<see cref="ISerializer{string}"/></param>
        protected ServiceSubscribeManagerBase(ISerializer<string> serializer)
        {
            _serializer = serializer;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// 清空所有的服务订阅者。
        /// </summary>
        /// <returns>一个任务。</returns>
        public abstract Task ClearAsync();

        /// <summary>
        /// 获取所有服务订阅者信息。
        /// </summary>
        /// <returns>服务路由集合。</returns>
        public abstract Task<IEnumerable<ServiceSubscriber>> GetSubscribersAsync();

        /// <summary>
        /// 设置服务订阅者。
        /// </summary>
        /// <param name="subscibers">The subscibers<see cref="IEnumerable{ServiceSubscriber}"/></param>
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
        /// 设置服务订阅者。
        /// </summary>
        /// <param name="subscribers">The subscribers<see cref="IEnumerable{ServiceSubscriberDescriptor}"/></param>
        /// <returns>一个任务。</returns>
        protected abstract Task SetSubscribersAsync(IEnumerable<ServiceSubscriberDescriptor> subscribers);

        #endregion 方法
    }
}