using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.ApiGateWay
{
    #region 枚举

    /// <summary>
    /// Defines the ServiceStatusCode
    /// </summary>
    public enum ServiceStatusCode
    {
        /// <summary>
        /// Defines the Success
        /// </summary>
        Success = 200,

        /// <summary>
        /// Defines the RequestError
        /// </summary>
        RequestError = 400,

        /// <summary>
        /// Defines the AuthorizationFailed
        /// </summary>
        AuthorizationFailed = 401,
    }

    #endregion 枚举
}