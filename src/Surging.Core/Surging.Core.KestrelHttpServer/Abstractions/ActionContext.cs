using Microsoft.AspNetCore.Http;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Runtime.Server;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.KestrelHttpServer
{
    /// <summary>
    /// Defines the <see cref="ActionContext" />
    /// </summary>
    public class ActionContext
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionContext"/> class.
        /// </summary>
        public ActionContext()
        {
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the HttpContext
        /// </summary>
        public HttpContext HttpContext { get; set; }

        /// <summary>
        /// Gets or sets the Message
        /// </summary>
        public TransportMessage Message { get; set; }

        #endregion 属性
    }
}