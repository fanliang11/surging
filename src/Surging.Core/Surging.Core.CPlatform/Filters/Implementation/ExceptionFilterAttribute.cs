using Surging.Core.CPlatform.Filters.Implementation;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Filters.Implementation
{
    /// <summary>
    /// Defines the <see cref="ExceptionFilterAttribute" />
    /// </summary>
    public abstract class ExceptionFilterAttribute : FilterAttribute, IExceptionFilter, IFilter
    {
        #region 方法

        /// <summary>
        /// The OnException
        /// </summary>
        /// <param name="actionExecutedContext">The actionExecutedContext<see cref="RpcActionExecutedContext"/></param>
        public virtual void OnException(RpcActionExecutedContext actionExecutedContext)
        {
        }

        /// <summary>
        /// The OnExceptionAsync
        /// </summary>
        /// <param name="actionExecutedContext">The actionExecutedContext<see cref="RpcActionExecutedContext"/></param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public virtual Task OnExceptionAsync(RpcActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            try
            {
                OnException(actionExecutedContext);
            }
            catch (Exception ex)
            {
                return TaskHelpers.FromError(ex);
            }

            return TaskHelpers.Completed();
        }

        /// <summary>
        /// The ExecuteExceptionFilterAsyncCore
        /// </summary>
        /// <param name="actionExecutedContext">The actionExecutedContext<see cref="RpcActionExecutedContext"/></param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task"/></returns>
        private async Task ExecuteExceptionFilterAsyncCore(RpcActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            await OnExceptionAsync(actionExecutedContext, cancellationToken);
        }

        /// <summary>
        /// The ExecuteExceptionFilterAsync
        /// </summary>
        /// <param name="actionExecutedContext">The actionExecutedContext<see cref="RpcActionExecutedContext"/></param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task"/></returns>
        Task IExceptionFilter.ExecuteExceptionFilterAsync(RpcActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            Check.NotNull(actionExecutedContext, "actionExecutedContext");
            return ExecuteExceptionFilterAsyncCore(actionExecutedContext, cancellationToken);
        }

        #endregion 方法
    }
}