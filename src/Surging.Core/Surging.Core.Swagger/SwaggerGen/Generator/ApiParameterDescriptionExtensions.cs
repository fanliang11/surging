using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Linq;
using System.Reflection;

namespace Surging.Core.SwaggerGen
{
    /// <summary>
    /// Defines the <see cref="ApiParameterDescriptionExtensions" />
    /// </summary>
    public static class ApiParameterDescriptionExtensions
    {
        #region 方法

        /// <summary>
        /// The TryGetParameterInfo
        /// </summary>
        /// <param name="apiParameterDescription">The apiParameterDescription<see cref="ApiParameterDescription"/></param>
        /// <param name="apiDescription">The apiDescription<see cref="ApiDescription"/></param>
        /// <param name="parameterInfo">The parameterInfo<see cref="ParameterInfo"/></param>
        /// <returns>The <see cref="bool"/></returns>
        internal static bool TryGetParameterInfo(
            this ApiParameterDescription apiParameterDescription,
            ApiDescription apiDescription,
            out ParameterInfo parameterInfo)
        {
            var controllerParameterDescriptor = apiDescription.ActionDescriptor.Parameters
                .OfType<ControllerParameterDescriptor>()
                .FirstOrDefault(descriptor =>
                {
                    return (apiParameterDescription.Name == descriptor.BindingInfo?.BinderModelName)
                        || (apiParameterDescription.Name == descriptor.Name);
                });

            parameterInfo = controllerParameterDescriptor?.ParameterInfo;

            return (parameterInfo != null);
        }

        /// <summary>
        /// The TryGetPropertyInfo
        /// </summary>
        /// <param name="apiParameterDescription">The apiParameterDescription<see cref="ApiParameterDescription"/></param>
        /// <param name="propertyInfo">The propertyInfo<see cref="PropertyInfo"/></param>
        /// <returns>The <see cref="bool"/></returns>
        internal static bool TryGetPropertyInfo(
            this ApiParameterDescription apiParameterDescription,
            out PropertyInfo propertyInfo)
        {
            var modelMetadata = apiParameterDescription.ModelMetadata;

            propertyInfo = (modelMetadata?.ContainerType != null)
                ? modelMetadata.ContainerType.GetProperty(modelMetadata.PropertyName)
                : null;

            return (propertyInfo != null);
        }

        #endregion 方法
    }
}