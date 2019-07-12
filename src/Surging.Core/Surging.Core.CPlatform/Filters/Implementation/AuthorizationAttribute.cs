using Surging.Core.CPlatform.Routing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Filters.Implementation
{
    /// <summary>
    /// Defines the <see cref="AuthorizationAttribute" />
    /// </summary>
    public class AuthorizationAttribute : AuthorizationFilterAttribute
    {
        #region 属性

        /// <summary>
        /// Gets or sets the AuthType
        /// </summary>
        public AuthorizationType AuthType { get; set; }

        #endregion 属性
    }
}