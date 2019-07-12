using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Caching.Models
{
    /// <summary>
    /// Defines the <see cref="CachingProvider" />
    /// </summary>
    public class CachingProvider
    {
        #region 属性

        /// <summary>
        /// Gets or sets the CachingSettings
        /// </summary>
        public List<Binding> CachingSettings { get; set; }

        #endregion 属性
    }
}