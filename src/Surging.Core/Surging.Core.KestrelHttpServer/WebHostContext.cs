using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Surging.Core.KestrelHttpServer
{
    /// <summary>
    /// Defines the <see cref="WebHostContext" />
    /// </summary>
    public class WebHostContext
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="WebHostContext"/> class.
        /// </summary>
        /// <param name="context">The context<see cref="WebHostBuilderContext"/></param>
        /// <param name="options">The options<see cref="KestrelServerOptions"/></param>
        /// <param name="ipAddress">The ipAddress<see cref="IPAddress"/></param>
        public WebHostContext(WebHostBuilderContext context, KestrelServerOptions options, IPAddress ipAddress)
        {
            WebHostBuilderContext = Check.NotNull(context, nameof(context));
            KestrelOptions = Check.NotNull(options, nameof(options));
            Address = ipAddress;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the Address
        /// </summary>
        public IPAddress Address { get; }

        /// <summary>
        /// Gets the KestrelOptions
        /// </summary>
        public KestrelServerOptions KestrelOptions { get; }

        /// <summary>
        /// Gets the WebHostBuilderContext
        /// </summary>
        public WebHostBuilderContext WebHostBuilderContext { get; }

        #endregion 属性
    }
}