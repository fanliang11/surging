using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.KestrelHttpServer
{
    /// <summary>
    /// Defines the <see cref="ActionResult" />
    /// </summary>
    public abstract class ActionResult : IActionResult
    {
        #region 方法

        /// <summary>
        /// The ExecuteResult
        /// </summary>
        /// <param name="context">The context<see cref="ActionContext"/></param>
        public virtual void ExecuteResult(ActionContext context)
        {
        }

        /// <summary>
        /// The ExecuteResultAsync
        /// </summary>
        /// <param name="context">The context<see cref="ActionContext"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public virtual Task ExecuteResultAsync(ActionContext context)
        {
            ExecuteResult(context);
            return Task.CompletedTask;
        }

        #endregion 方法
    }
}