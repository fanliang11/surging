using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.KestrelHttpServer
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IActionResult" />
    /// </summary>
    public interface IActionResult
    {
        #region 方法

        /// <summary>
        /// The ExecuteResultAsync
        /// </summary>
        /// <param name="context">The context<see cref="ActionContext"/></param>
        /// <returns>The <see cref="Task"/></returns>
        Task ExecuteResultAsync(ActionContext context);

        #endregion 方法
    }

    #endregion 接口
}