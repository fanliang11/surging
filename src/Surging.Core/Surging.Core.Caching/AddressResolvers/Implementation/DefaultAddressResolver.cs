using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Surging.Core.Caching.HealthChecks;
using Surging.Core.Caching.RedisCache;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Cache;

namespace Surging.Core.Caching.AddressResolvers.Implementation
{
    public class DefaultAddressResolver : IAddressResolver
    {

        #region Field  
        private readonly ILogger<DefaultAddressResolver> _logger;
        private readonly IHealthCheckService _healthCheckService;
        private readonly CPlatformContainer _container;
        #endregion

        public DefaultAddressResolver(ILogger<DefaultAddressResolver> logger, CPlatformContainer container, IHealthCheckService healthCheckService)
        {
            _logger = logger;
            _container = container;
            _healthCheckService = healthCheckService;
        }

        public ValueTask<CacheEndpoint> Resolver(string CacheId, CacheTargetType hashCode)
        {
           var redisContext= _container.GetInstances<RedisContext>(CacheId);
            throw new NotImplementedException();
        }
    }
}
