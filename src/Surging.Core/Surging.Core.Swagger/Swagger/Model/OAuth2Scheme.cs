using System.Collections.Generic;

namespace Surging.Core.Swagger
{
    /// <summary>
    /// Defines the <see cref="OAuth2Scheme" />
    /// </summary>
    public class OAuth2Scheme : SecurityScheme
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuth2Scheme"/> class.
        /// </summary>
        public OAuth2Scheme()
        {
            Type = "oauth2";
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the AuthorizationUrl
        /// </summary>
        public string AuthorizationUrl { get; set; }

        /// <summary>
        /// Gets or sets the Flow
        /// </summary>
        public string Flow { get; set; }

        /// <summary>
        /// Gets or sets the Scopes
        /// </summary>
        public IDictionary<string, string> Scopes { get; set; }

        /// <summary>
        /// Gets or sets the TokenUrl
        /// </summary>
        public string TokenUrl { get; set; }

        #endregion 属性
    }
}