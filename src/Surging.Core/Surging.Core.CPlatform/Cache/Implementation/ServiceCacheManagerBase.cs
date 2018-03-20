using Surging.Core.CPlatform.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Cache.Implementation
{
    public class ServiceCacheEventArgs
    {
        public ServiceCacheEventArgs(ServiceCache cache)
        {
            Cache = cache;
        }
         
        public ServiceCache Cache { get; private set; }
    }
     
    public class ServiceCacheChangedEventArgs : ServiceCacheEventArgs
    {
        public ServiceCacheChangedEventArgs(ServiceCache cache, ServiceCache oldCache) : base(cache)
        {
            OldCache = oldCache;
        }
 
        public ServiceCache OldCache { get; set; }
    }

    public abstract class ServiceCacheManagerBase : IServiceCacheManager
    {
        private readonly ISerializer<string> _serializer;
        private  EventHandler<ServiceCacheEventArgs> _created;
        private EventHandler<ServiceCacheEventArgs> _removed;
        private EventHandler<ServiceCacheChangedEventArgs> _changed;

        protected ServiceCacheManagerBase(ISerializer<string> serializer)
        {
            _serializer = serializer;
        }
        
        public event EventHandler<ServiceCacheEventArgs> Created
        {
            add { _created += value; }
            remove { _created -= value; }
        }
        
        public event EventHandler<ServiceCacheEventArgs> Removed
        {
            add { _removed += value; }
            remove { _removed -= value; }
        }
        
        public event EventHandler<ServiceCacheChangedEventArgs> Changed
        {
            add { _changed += value; }
            remove { _changed -= value; }
        }

        public abstract Task ClearAsync();
        public abstract Task<IEnumerable<ServiceCache>> GetCachesAsync();
         
        public abstract Task RemveAddressAsync(IEnumerable<CacheEndpoint> endpoints);


        public virtual Task SetCachesAsync(IEnumerable<ServiceCache> caches)
        {
            if (caches == null)
                throw new ArgumentNullException(nameof(caches));

            var descriptors = caches.Where(cache => cache != null).Select(cache => new ServiceCacheDescriptor
            {
                AddressDescriptors = cache.CacheEndpoint?.Select(address => new CacheEndpointDescriptor
                {
                    Type = address.GetType().FullName,
                    Value = _serializer.Serialize(address)
                }) ?? Enumerable.Empty<CacheEndpointDescriptor>(),
                 CacheDescriptor = cache.CacheDescriptor
            });
            return SetCachesAsync(descriptors);
        }

        public abstract Task SetCachesAsync(IEnumerable<ServiceCacheDescriptor> cacheDescriptors);

        protected void OnCreated(params ServiceCacheEventArgs[] args)
        {
            if (_created == null)
                return;

            foreach (var arg in args)
                _created(this, arg);
        }

        protected void OnChanged(params ServiceCacheChangedEventArgs[] args)
        {
            if (_changed == null)
                return;

            foreach (var arg in args)
                _changed(this, arg);
        }

        protected void OnRemoved(params ServiceCacheEventArgs[] args)
        {
            if (_removed == null)
                return;

            foreach (var arg in args)
                _removed(this, arg);
        }
    }
}
