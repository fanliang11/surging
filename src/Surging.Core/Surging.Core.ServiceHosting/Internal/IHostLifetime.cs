using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.ServiceHosting.Internal
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IHostLifetime" />
    /// </summary>
    public interface IHostLifetime
    {
        #region 方法

        /// <summary>
        /// The StopAsync
        /// </summary>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task"/></returns>
        Task StopAsync(CancellationToken cancellationToken);

        /// <summary>
        /// The WaitForStartAsync
        /// </summary>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task"/></returns>
        Task WaitForStartAsync(CancellationToken cancellationToken);

        #endregion 方法
    }

    #endregion 接口
}