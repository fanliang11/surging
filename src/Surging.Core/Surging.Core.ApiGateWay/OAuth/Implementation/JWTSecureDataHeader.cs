using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.ApiGateWay.OAuth
{
    /// <summary>
    /// Defines the <see cref="JWTSecureDataHeader" />
    /// </summary>
    public class JWTSecureDataHeader
    {
        #region 属性

        /// <summary>
        /// Gets or sets the EncryptMode
        /// </summary>
        public EncryptMode EncryptMode { get; set; }

        /// <summary>
        /// Gets or sets the TimeStamp
        /// </summary>
        public string TimeStamp { get; set; }

        /// <summary>
        /// Gets or sets the Type
        /// </summary>
        public JWTSecureDataType Type { get; set; }

        #endregion 属性
    }
}