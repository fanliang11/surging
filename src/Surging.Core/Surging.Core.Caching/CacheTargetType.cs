using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Caching
{
    #region 枚举

    /// <summary>
    /// Defines the CacheTargetType
    /// </summary>
    public enum CacheTargetType
    {
        /// <summary>
        /// Defines the Redis
        /// </summary>
        Redis,

        /// <summary>
        /// Defines the CouchBase
        /// </summary>
        CouchBase,

        /// <summary>
        /// Defines the Memcached
        /// </summary>
        Memcached,

        /// <summary>
        /// Defines the MemoryCache
        /// </summary>
        MemoryCache,
    }

    #endregion 枚举
}