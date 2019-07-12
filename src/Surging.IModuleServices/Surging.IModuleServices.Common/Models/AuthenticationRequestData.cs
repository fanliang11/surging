using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.IModuleServices.Common.Models
{
    /// <summary>
    /// Defines the <see cref="AuthenticationRequestData" />
    /// </summary>
    [ProtoContract]
    public class AuthenticationRequestData
    {
        #region 属性

        /// <summary>
        /// Gets or sets the Password
        /// </summary>
        [ProtoMember(2)]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the UserName
        /// </summary>
        [ProtoMember(1)]
        public string UserName { get; set; }

        #endregion 属性
    }
}