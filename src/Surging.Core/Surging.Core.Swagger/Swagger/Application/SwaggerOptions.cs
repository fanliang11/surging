using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace Surging.Core.Swagger
{
    /// <summary>
    /// Defines the <see cref="SwaggerOptions" />
    /// </summary>
    public class SwaggerOptions
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="SwaggerOptions"/> class.
        /// </summary>
        public SwaggerOptions()
        {
            PreSerializeFilters = new List<Action<SwaggerDocument, HttpRequest>>();
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the PreSerializeFilters
        /// Actions that can be applied SwaggerDocument's before they're serialized to JSON.
        /// Useful for setting metadata that's derived from the current request
        /// </summary>
        public List<Action<SwaggerDocument, HttpRequest>> PreSerializeFilters { get; private set; }

        /// <summary>
        /// Gets or sets the RouteTemplate
        /// Sets a custom route for the Swagger JSON endpoint(s). Must include the {documentName} parameter
        /// </summary>
        public string RouteTemplate { get; set; } = "swagger/{documentName}/swagger.json";

        #endregion 属性
    }
}