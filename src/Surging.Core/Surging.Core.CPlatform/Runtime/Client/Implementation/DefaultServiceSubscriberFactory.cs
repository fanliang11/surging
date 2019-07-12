using Surging.Core.CPlatform.Address;
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
    /// Defines the <see cref="DefaultServiceSubscriberFactory" />
    /// </summary>
    internal class DefaultServiceSubscriberFactory : IServiceSubscriberFactory
    {
        #region 字段

        /// <summary>
        /// Defines the _serializer
        /// </summary>
        private readonly ISerializer<string> _serializer;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultServiceSubscriberFactory"/> class.
        /// </summary>
        /// <param name="serializer">The serializer<see cref="ISerializer{string}"/></param>
        public DefaultServiceSubscriberFactory(ISerializer<string> serializer)
        {
            _serializer = serializer;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The CreateServiceSubscribersAsync
        /// </summary>
        /// <param name="descriptors">The descriptors<see cref="IEnumerable{ServiceSubscriberDescriptor}"/></param>
        /// <returns>The <see cref="Task{IEnumerable{ServiceSubscriber}}"/></returns>
        public Task<IEnumerable<ServiceSubscriber>> CreateServiceSubscribersAsync(IEnumerable<ServiceSubscriberDescriptor> descriptors)
        {
            if (descriptors == null)
                throw new ArgumentNullException(nameof(descriptors));

            descriptors = descriptors.ToArray();
            var subscribers = new List<ServiceSubscriber>(descriptors.Count());
            subscribers.AddRange(descriptors.Select(descriptor => new ServiceSubscriber
            {
                Address = CreateAddress(descriptor.AddressDescriptors),
                ServiceDescriptor = descriptor.ServiceDescriptor
            }));
            return Task.FromResult(subscribers.AsEnumerable());
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
                yield return (AddressModel)_serializer.Deserialize(descriptor.Value, typeof(IpAddressModel));
            }
        }

        #endregion 方法
    }
}