using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.ProxyGenerator.Interceptors.Implementation.Metadatas
{
    public enum CacheTargetType
    {
        Redis,
        CouchBase,
        Memcached,
        MemoryCache,
    }
}
