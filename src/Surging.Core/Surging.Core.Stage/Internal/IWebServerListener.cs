using Surging.Core.KestrelHttpServer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Stage.Internal
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IWebServerListener" />
    /// </summary>
    public interface IWebServerListener
    {
        #region 方法

        /// <summary>
        /// The Listen
        /// </summary>
        /// <param name="context">The context<see cref="WebHostContext"/></param>
        void Listen(WebHostContext context);

        #endregion 方法
    }

    #endregion 接口
}