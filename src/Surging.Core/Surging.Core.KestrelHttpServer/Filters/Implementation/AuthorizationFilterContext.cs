using Microsoft.AspNetCore.Http;
using Surging.Core.CPlatform.Routing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.KestrelHttpServer.Filters.Implementation
{
    /// <summary>
    /// Defines the <see cref="AuthorizationFilterContext" />
    /// </summary>
    public class AuthorizationFilterContext
    {
        #region 属性

        /// <summary>
        /// Gets or sets the Context
        /// </summary>
        public HttpContext Context { get; internal set; }

        /// <summary>
        /// Gets or sets the Route
        /// </summary>
        public ServiceRoute Route { get; internal set; }

        #endregion 属性
    }
}