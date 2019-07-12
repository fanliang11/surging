using Newtonsoft.Json;
using System.Collections.Generic;

namespace Surging.Core.Swagger
{
    /// <summary>
    /// Defines the <see cref="SecurityScheme" />
    /// </summary>
    public abstract class SecurityScheme
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityScheme"/> class.
        /// </summary>
        public SecurityScheme()
        {
            Extensions = new Dictionary<string, object>();
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets the Extensions
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, object> Extensions { get; private set; }

        /// <summary>
        /// Gets or sets the Type
        /// </summary>
        public string Type { get; set; }

        #endregion 属性
    }
}