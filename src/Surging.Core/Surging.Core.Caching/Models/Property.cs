using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Caching.Models
{
    /// <summary>
    /// Defines the <see cref="Property" />
    /// </summary>
    public class Property
    {
        #region 属性

        /// <summary>
        /// Gets or sets the Maps
        /// </summary>
        public List<Map> Maps { get; set; }

        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Ref
        /// </summary>
        public string Ref { get; set; }

        /// <summary>
        /// Gets or sets the Value
        /// </summary>
        public string Value { get; set; }

        #endregion 属性
    }
}