using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Stage.Configurations
{
    /// <summary>
    /// Defines the <see cref="StageOption" />
    /// </summary>
    public class StageOption
    {
        #region 属性

        /// <summary>
        /// Gets or sets the CertificateFileName
        /// </summary>
        public string CertificateFileName { get; set; }

        /// <summary>
        /// Gets or sets the CertificateLocation
        /// </summary>
        public string CertificateLocation { get; set; }

        /// <summary>
        /// Gets or sets the CertificatePassword
        /// </summary>
        public string CertificatePassword { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether EnableHttps
        /// </summary>
        public bool EnableHttps { get; set; }

        /// <summary>
        /// Gets or sets the HttpPorts
        /// </summary>
        public string HttpPorts { get; set; }

        /// <summary>
        /// Gets or sets the HttpsPort
        /// </summary>
        public string HttpsPort { get; set; }

        #endregion 属性
    }
}