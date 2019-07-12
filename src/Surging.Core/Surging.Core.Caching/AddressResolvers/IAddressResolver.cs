using Surging.Core.Caching.HashAlgorithms;
using Surging.Core.CPlatform.Cache;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Caching.AddressResolvers
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IAddressResolver" />
    /// </summary>
    public interface IAddressResolver
    {
        #region 方法

        /// <summary>
        /// The Resolver
        /// </summary>
        /// <param name="cacheId">The cacheId<see cref="string"/></param>
        /// <param name="item">The item<see cref="string"/></param>
        /// <returns>The <see cref="ValueTask{ConsistentHashNode}"/></returns>
        ValueTask<ConsistentHashNode> Resolver(string cacheId, string item);

        #endregion 方法
    }

    #endregion 接口
}