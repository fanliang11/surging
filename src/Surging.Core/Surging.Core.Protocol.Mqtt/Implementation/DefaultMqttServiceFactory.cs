using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Mqtt;
using Surging.Core.CPlatform.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Mqtt.Implementation
{
    /// <summary>
    /// Defines the <see cref="DefaultMqttServiceFactory" />
    /// </summary>
    public class DefaultMqttServiceFactory : IMqttServiceFactory
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
        /// Initializes a new instance of the <see cref="DefaultMqttServiceFactory"/> class.
        /// </summary>
        /// <param name="serializer">The serializer<see cref="ISerializer{string}"/></param>
        public DefaultMqttServiceFactory(ISerializer<string> serializer)
        {
            _serializer = serializer;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The CreateMqttServiceRoutesAsync
        /// </summary>
        /// <param name="descriptors">The descriptors<see cref="IEnumerable{MqttServiceDescriptor}"/></param>
        /// <returns>The <see cref="Task{IEnumerable{MqttServiceRoute}}"/></returns>
        public Task<IEnumerable<MqttServiceRoute>> CreateMqttServiceRoutesAsync(IEnumerable<MqttServiceDescriptor> descriptors)
        {
            if (descriptors == null)
                throw new ArgumentNullException(nameof(descriptors));

            descriptors = descriptors.ToArray();
            var routes = new List<MqttServiceRoute>(descriptors.Count());

            routes.AddRange(descriptors.Select(descriptor => new MqttServiceRoute
            {
                MqttEndpoint = CreateAddress(descriptor.AddressDescriptors),
                MqttDescriptor = descriptor.MqttDescriptor
            }));

            return Task.FromResult(routes.AsEnumerable());
        }

        /// <summary>
        /// The CreateAddress
        /// </summary>
        /// <param name="descriptors">The descriptors<see cref="IEnumerable{MqttEndpointDescriptor}"/></param>
        /// <returns>The <see cref="IEnumerable{AddressModel}"/></returns>
        private IEnumerable<AddressModel> CreateAddress(IEnumerable<MqttEndpointDescriptor> descriptors)
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