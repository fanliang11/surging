using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.IModuleServices.Common.Models
{
    /// <summary>
    /// Defines the <see cref="RoteModel" />
    /// </summary>
    [ProtoContract]
    public class RoteModel
    {
        #region 属性

        /// <summary>
        /// Gets or sets the ServiceId
        /// </summary>
        [ProtoMember(1)]
        public string ServiceId { get; set; }

        #endregion 属性
    }
}