using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Surging.Core.SwaggerGen
{
    /// <summary>
    /// Defines the <see cref="ApiDescriptionExtensions" />
    /// </summary>
    public static class ApiDescriptionExtensions
    {
        #region 方法

        /// <summary>
        /// The ActionAttributes
        /// </summary>
        /// <param name="apiDescription">The apiDescription<see cref="ApiDescription"/></param>
        /// <returns>The <see cref="IEnumerable{object}"/></returns>
        [Obsolete("Deprecated: Use TryGetMethodInfo")]
        public static IEnumerable<object> ActionAttributes(this ApiDescription apiDescription)
        {
            var controllerActionDescriptor = apiDescription.ActionDescriptor as ControllerActionDescriptor;
            return (controllerActionDescriptor == null)
                ? Enumerable.Empty<object>()
                : controllerActionDescriptor.MethodInfo.GetCustomAttributes(true);
        }

        /// <summary>
        /// The ControllerAttributes
        /// </summary>
        /// <param name="apiDescription">The apiDescription<see cref="ApiDescription"/></param>
        /// <returns>The <see cref="IEnumerable{object}"/></returns>
        [Obsolete("Deprecated: Use TryGetMethodInfo")]
        public static IEnumerable<object> ControllerAttributes(this ApiDescription apiDescription)
        {
            var controllerActionDescriptor = apiDescription.ActionDescriptor as ControllerActionDescriptor;
            return (controllerActionDescriptor == null)
                ? Enumerable.Empty<object>()
                : controllerActionDescriptor.ControllerTypeInfo.GetCustomAttributes(true);
        }

        /// <summary>
        /// The TryGetMethodInfo
        /// </summary>
        /// <param name="apiDescription">The apiDescription<see cref="ApiDescription"/></param>
        /// <param name="methodInfo">The methodInfo<see cref="MethodInfo"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public static bool TryGetMethodInfo(this ApiDescription apiDescription, out MethodInfo methodInfo)
        {
            var controllerActionDescriptor = apiDescription.ActionDescriptor as ControllerActionDescriptor;

            methodInfo = controllerActionDescriptor?.MethodInfo;

            return (methodInfo != null);
        }

        /// <summary>
        /// The IsObsolete
        /// </summary>
        /// <param name="apiDescription">The apiDescription<see cref="ApiDescription"/></param>
        /// <returns>The <see cref="bool"/></returns>
        internal static bool IsObsolete(this ApiDescription apiDescription)
        {
            if (!apiDescription.TryGetMethodInfo(out MethodInfo methodInfo))
                return false;

            return methodInfo.GetCustomAttributes(true)
                .Union(methodInfo.DeclaringType.GetTypeInfo().GetCustomAttributes(true))
                .Any(attr => attr.GetType() == typeof(ObsoleteAttribute));
        }

        /// <summary>
        /// The RelativePathSansQueryString
        /// </summary>
        /// <param name="apiDescription">The apiDescription<see cref="ApiDescription"/></param>
        /// <returns>The <see cref="string"/></returns>
        internal static string RelativePathSansQueryString(this ApiDescription apiDescription)
        {
            return apiDescription.RelativePath.Split('?').First();
        }

        #endregion 方法
    }
}