using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Surging.Core.Swagger
{
    /// <summary>
    /// Defines the <see cref="SwaggerSerializerFactory" />
    /// </summary>
    public class SwaggerSerializerFactory
    {
        #region 方法

        /// <summary>
        /// The Create
        /// </summary>
        /// <param name="applicationJsonOptions">The applicationJsonOptions<see cref="IOptions{MvcJsonOptions}"/></param>
        /// <returns>The <see cref="JsonSerializer"/></returns>
        public static JsonSerializer Create(IOptions<MvcJsonOptions> applicationJsonOptions)
        {
            // TODO: Should this handle case where mvcJsonOptions.Value == null?
            return new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = applicationJsonOptions.Value.SerializerSettings.Formatting,
                ContractResolver = new SwaggerContractResolver(applicationJsonOptions.Value.SerializerSettings)
            };
        }

        #endregion 方法
    }
}