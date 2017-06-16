using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Caching
{
    public enum CacheTargetType
    {
        Redis,
        CouchBase,
        Memcached,
        MemoryCache,
    }
}
