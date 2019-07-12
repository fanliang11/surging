using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Runtime.Client.Address.Resolvers.Implementation.Selectors;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Consul.Internal.Cluster.Implementation.Selectors
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IConsulAddressSelector" />
    /// </summary>
    public interface IConsulAddressSelector : IAddressSelector
    {
    }

    #endregion 接口
}