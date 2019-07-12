using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Surging.Core.CPlatform.Runtime.Client.Implementation
{
    /// <summary>
    /// Defines the <see cref="DefaultServiceHeartbeatManager" />
    /// </summary>
    public class DefaultServiceHeartbeatManager : IServiceHeartbeatManager
    {
        #region 字段

        /// <summary>
        /// Defines the _whitelist
        /// </summary>
        private readonly ConcurrentBag<string> _whitelist = new ConcurrentBag<string>();

        #endregion 字段

        #region 方法

        /// <summary>
        /// The AddWhitelist
        /// </summary>
        /// <param name="serviceId">The serviceId<see cref="string"/></param>
        public void AddWhitelist(string serviceId)
        {
            if (!_whitelist.Contains(serviceId))
                _whitelist.Add(serviceId);
        }

        /// <summary>
        /// The ExistsWhitelist
        /// </summary>
        /// <param name="serviceId">The serviceId<see cref="string"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public bool ExistsWhitelist(string serviceId)
        {
            return _whitelist.Contains(serviceId);
        }

        #endregion 方法
    }
}