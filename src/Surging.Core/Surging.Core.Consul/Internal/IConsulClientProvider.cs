using Consul;
using Surging.Core.Consul.Internal.Cluster.Implementation.Selectors.Implementation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Consul.Internal
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IConsulClientProvider" />
    /// </summary>
    public interface IConsulClientProvider
    {
        #region 方法

        /// <summary>
        /// The Check
        /// </summary>
        /// <returns>The <see cref="ValueTask"/></returns>
        ValueTask Check();

        /// <summary>
        /// The GetClient
        /// </summary>
        /// <returns>The <see cref="ValueTask{ConsulClient}"/></returns>
        ValueTask<ConsulClient> GetClient();

        /// <summary>
        /// The GetClients
        /// </summary>
        /// <returns>The <see cref="ValueTask{IEnumerable{ConsulClient}}"/></returns>
        ValueTask<IEnumerable<ConsulClient>> GetClients();

        #endregion 方法
    }

    #endregion 接口
}