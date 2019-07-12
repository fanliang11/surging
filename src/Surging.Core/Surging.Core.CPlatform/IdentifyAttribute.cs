using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform
{
    /// <summary>
    /// Defines the <see cref="IdentifyAttribute" />
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class IdentifyAttribute : Attribute
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentifyAttribute"/> class.
        /// </summary>
        /// <param name="name">The name<see cref="CommunicationProtocol"/></param>
        public IdentifyAttribute(CommunicationProtocol name)
        {
            this.Name = name;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        public CommunicationProtocol Name { get; set; }

        #endregion 属性
    }
}