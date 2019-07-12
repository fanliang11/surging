using Surging.Core.CPlatform.Ioc;
using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.IModuleServices.Common
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IDnsService" />
    /// </summary>
    [ServiceBundle("Dns/{Service}")]
    public interface IDnsService : IServiceKey
    {
    }

    #endregion 接口
}