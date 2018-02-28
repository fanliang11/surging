using Surging.Core.CPlatform.Cache;
using Surging.Core.CPlatform.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Caching.Internal.Implementation
{
    public class DefaultServiceCacheFactory : IServiceCacheFactory
    {
        private readonly ISerializer<string> _serializer;
        private readonly ConcurrentDictionary<string, CacheEndpoint> _addressModel =
               new ConcurrentDictionary<string, CacheEndpoint>();

        public DefaultServiceCacheFactory(ISerializer<string> serializer)
        {
            _serializer = serializer;
        }

        public Task<IEnumerable<ServiceCache>> CreateServiceCachesAsync(IEnumerable<ServiceCacheDescriptor> descriptors)
        {
            if (descriptors == null)
                throw new ArgumentNullException(nameof(descriptors));

            descriptors = descriptors.ToArray();
            var routes = new List<ServiceCache>(descriptors.Count());

            routes.AddRange(descriptors.Select(descriptor => new ServiceCache
            {
                 CacheEndpoint = CreateAddress(descriptor.AddressDescriptors),
                 CacheDescriptor = descriptor.CacheDescriptor
            }));

            return Task.FromResult(routes.AsEnumerable());
        }
   

        private IEnumerable<CacheEndpoint> CreateAddress(IEnumerable<CacheEndpointDescriptor> descriptors)
        {
            if (descriptors == null)
                yield break;

            foreach (var descriptor in descriptors)
            {
                _addressModel.TryGetValue(descriptor.Value, out CacheEndpoint address);
                if (address == null)
                {
                    var addressType = Type.GetType(descriptor.Type);
                    address = (CacheEndpoint)_serializer.Deserialize(descriptor.Value, addressType);
                    _addressModel.TryAdd(descriptor.Value, address);
                }
                yield return address;
            }
        }
    }
}
