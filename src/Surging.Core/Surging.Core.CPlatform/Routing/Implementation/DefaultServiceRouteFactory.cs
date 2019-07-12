using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Routing.Implementation
{
    /// <summary>
    /// 一个默认的服务路由工厂实现。
    /// </summary>
    public class DefaultServiceRouteFactory : IServiceRouteFactory
    {
        #region 字段

        /// <summary>
        /// Defines the _addressModel
        /// </summary>
        private readonly ConcurrentDictionary<string, AddressModel> _addressModel =
                new ConcurrentDictionary<string, AddressModel>();

        /// <summary>
        /// Defines the _serializer
        /// </summary>
        private readonly ISerializer<string> _serializer;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultServiceRouteFactory"/> class.
        /// </summary>
        /// <param name="serializer">The serializer<see cref="ISerializer{string}"/></param>
        public DefaultServiceRouteFactory(ISerializer<string> serializer)
        {
            _serializer = serializer;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// 根据服务路由描述符创建服务路由。
        /// </summary>
        /// <param name="descriptors">服务路由描述符。</param>
        /// <returns>服务路由集合。</returns>
        public Task<IEnumerable<ServiceRoute>> CreateServiceRoutesAsync(IEnumerable<ServiceRouteDescriptor> descriptors)
        {
            if (descriptors == null)
                throw new ArgumentNullException(nameof(descriptors));

            descriptors = descriptors.ToArray();
            var routes = new List<ServiceRoute>(descriptors.Count());

            routes.AddRange(descriptors.Select(descriptor => new ServiceRoute
            {
                Address = CreateAddress(descriptor.AddressDescriptors),
                ServiceDescriptor = descriptor.ServiceDescriptor
            }));

            return Task.FromResult(routes.AsEnumerable());
        }

        /// <summary>
        /// The CreateAddress
        /// </summary>
        /// <param name="descriptors">The descriptors<see cref="IEnumerable{ServiceAddressDescriptor}"/></param>
        /// <returns>The <see cref="IEnumerable{AddressModel}"/></returns>
        private IEnumerable<AddressModel> CreateAddress(IEnumerable<ServiceAddressDescriptor> descriptors)
        {
            if (descriptors == null)
                yield break;

            foreach (var descriptor in descriptors)
            {
                _addressModel.TryGetValue(descriptor.Value, out AddressModel address);
                if (address == null)
                {
                    address = (AddressModel)_serializer.Deserialize(descriptor.Value, typeof(IpAddressModel));
                    _addressModel.TryAdd(descriptor.Value, address);
                }
                yield return address;
            }
        }

        #endregion 方法
    }
}