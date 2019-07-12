using Microsoft.AspNetCore.Mvc.ApiExplorer;
using System;
using System.Reflection;

namespace Surging.Core.SwaggerGen
{
    /// <summary>
    /// Defines the <see cref="ApiResponseTypeExtensions" />
    /// </summary>
    public static class ApiResponseTypeExtensions
    {
        #region 方法

        /// <summary>
        /// The IsDefaultResponse
        /// </summary>
        /// <param name="apiResponseType">The apiResponseType<see cref="ApiResponseType"/></param>
        /// <returns>The <see cref="bool"/></returns>
        internal static bool IsDefaultResponse(this ApiResponseType apiResponseType)
        {
            var propertyInfo = apiResponseType.GetType().GetProperty("IsDefaultResponse");
            if (propertyInfo != null)
            {
                return (bool)propertyInfo.GetValue(apiResponseType);
            }

            // ApiExplorer < 2.1.0 does not support default response.
            return false;
        }

        #endregion 方法
    }
}