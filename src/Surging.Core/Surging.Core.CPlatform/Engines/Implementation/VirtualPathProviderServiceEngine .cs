using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Engines.Implementation
{
    /// <summary>
    /// Defines the <see cref="VirtualPathProviderServiceEngine" />
    /// </summary>
    public abstract class VirtualPathProviderServiceEngine : IServiceEngine
    {
        #region 属性

        /// <summary>
        /// Gets or sets the ComponentServiceLocationFormats
        /// </summary>
        public string[] ComponentServiceLocationFormats { get; set; }

        /// <summary>
        /// Gets or sets the ModuleServiceLocationFormats
        /// </summary>
        public string[] ModuleServiceLocationFormats { get; set; }

        #endregion 属性
    }
}