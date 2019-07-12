using ProtoBuf;
using Surging.Core.System.Intercept;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.IModuleServices.Common.Models
{
    /// <summary>
    /// Defines the <see cref="BaseModel" />
    /// </summary>
    [ProtoContract]
    public class BaseModel
    {
        #region 属性

        /// <summary>
        /// Gets the Id
        /// </summary>
        [ProtoMember(1)]
        public Guid Id => Guid.NewGuid();

        #endregion 属性
    }
}