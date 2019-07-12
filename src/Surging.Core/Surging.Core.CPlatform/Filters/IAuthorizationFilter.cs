using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Runtime.Client.Address.Resolvers.Implementation.Selectors;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Filters
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IAuthorizationFilter" />
    /// </summary>
    public interface IAuthorizationFilter : IFilter
    {
        #region 方法

        /// <summary>
        /// The ExecuteAuthorizationFilterAsync
        /// </summary>
        /// <param name="serviceRouteContext">The serviceRouteContext<see cref="ServiceRouteContext"/></param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        void ExecuteAuthorizationFilterAsync(ServiceRouteContext serviceRouteContext, CancellationToken cancellationToken);

        #endregion 方法
    }

    #endregion 接口
}