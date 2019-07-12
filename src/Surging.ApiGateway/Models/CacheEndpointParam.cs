using Surging.Core.Caching.HashAlgorithms;
using Surging.Core.CPlatform.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surging.ApiGateway.Models
{
    /// <summary>
    /// Defines the <see cref="CacheEndpointParam" />
    /// </summary>
    public class CacheEndpointParam
    {
        #region 属性

        /// <summary>
        /// Gets or sets the CacheEndpoint
        /// </summary>
        public ConsistentHashNode CacheEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the CacheId
        /// </summary>
        public string CacheId { get; set; }

        /// <summary>
        /// Gets or sets the Endpoint
        /// </summary>
        public string Endpoint { get; set; }

        #endregion 属性
    }
}