using ProtoBuf;
using Surging.Core.CPlatform;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.IModuleServices.Common.Models
{
    /// <summary>
    /// Defines the <see cref="IdentityUser" />
    /// </summary>
    [ProtoContract]
    public class IdentityUser : RequestData
    {
        #region 属性

        /// <summary>
        /// Gets or sets the RoleId
        /// </summary>
        [ProtoMember(1)]
        public string RoleId { get; set; }

        #endregion 属性
    }
}