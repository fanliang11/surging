using Surging.Core.CPlatform.Runtime.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Mqtt.Internal.Runtime
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IMqttRemoteInvokeService" />
    /// </summary>
    public interface IMqttRemoteInvokeService
    {
        #region 方法

        /// <summary>
        /// The InvokeAsync
        /// </summary>
        /// <param name="context">The context<see cref="RemoteInvokeContext"/></param>
        /// <returns>The <see cref="Task"/></returns>
        Task InvokeAsync(RemoteInvokeContext context);

        /// <summary>
        /// The InvokeAsync
        /// </summary>
        /// <param name="context">The context<see cref="RemoteInvokeContext"/></param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task"/></returns>
        Task InvokeAsync(RemoteInvokeContext context, CancellationToken cancellationToken);

        /// <summary>
        /// The InvokeAsync
        /// </summary>
        /// <param name="context">The context<see cref="RemoteInvokeContext"/></param>
        /// <param name="requestTimeout">The requestTimeout<see cref="int"/></param>
        /// <returns>The <see cref="Task"/></returns>
        Task InvokeAsync(RemoteInvokeContext context, int requestTimeout);

        #endregion 方法
    }

    #endregion 接口
}