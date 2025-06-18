using Microsoft.AspNetCore.Mvc.ApiExplorer;
using System;
using System.Reflection;

namespace Surging.Core.SwaggerGen
{
    public static class ApiResponseTypeExtensions
    {
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
    }
}
