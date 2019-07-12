using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.ApiGateWay.Configurations
{
    /// <summary>
    /// Defines the <see cref="AccessPolicy" />
    /// </summary>
    public class AccessPolicy
    {
        #region 属性

        /// <summary>
        /// Gets or sets a value indicating whether AllowAnyHeader
        /// </summary>
        public bool AllowAnyHeader { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether AllowAnyMethod
        /// </summary>
        public bool AllowAnyMethod { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether AllowAnyOrigin
        /// </summary>
        public bool AllowAnyOrigin { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether AllowCredentials
        /// </summary>
        public bool AllowCredentials { get; set; }

        /// <summary>
        /// Gets or sets the Origins
        /// </summary>
        public string[] Origins { get; set; }

        #endregion 属性
    }
}