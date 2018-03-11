using Surging.Core.Caching.HashAlgorithms;
using Surging.Core.CPlatform.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surging.ApiGateway.Models
{
    public class CacheEndpointParam
    {
        public string CacheId { get; set; }

        public string Endpoint { get; set; }

        public ConsistentHashNode CacheEndpoint { get; set; }
    }
}
