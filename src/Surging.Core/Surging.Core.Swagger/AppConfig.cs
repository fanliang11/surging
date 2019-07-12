using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Swagger
{
    /// <summary>
    /// Defines the <see cref="AppConfig" />
    /// </summary>
    public class AppConfig
    {
        #region 属性

        /// <summary>
        /// Gets or sets the SwaggerConfig
        /// </summary>
        public static DocumentConfiguration SwaggerConfig { get; internal set; }

        /// <summary>
        /// Gets or sets the SwaggerOptions
        /// </summary>
        public static Info SwaggerOptions { get; internal set; }

        #endregion 属性
    }
}