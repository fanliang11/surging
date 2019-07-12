using Surging.Core.CPlatform.Filters.Implementation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Filters
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IExceptionFilter" />
    /// </summary>
    public interface IExceptionFilter : IFilter
    {
        #region 方法

        /// <summary>
        /// The ExecuteExceptionFilterAsync
        /// </summary>
        /// <param name="actionExecutedContext">The actionExecutedContext<see cref="RpcActionExecutedContext"/></param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task"/></returns>
        Task ExecuteExceptionFilterAsync(RpcActionExecutedContext actionExecutedContext, CancellationToken cancellationToken);

        #endregion 方法
    }

    #endregion 接口
}