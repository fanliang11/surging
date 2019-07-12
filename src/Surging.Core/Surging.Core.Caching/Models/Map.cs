using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Caching.Models
{
    /// <summary>
    /// Defines the <see cref="Map" />
    /// </summary>
    public class Map
    {
        #region 属性

        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Properties
        /// </summary>
        public List<Property> Properties { get; set; }

        #endregion 属性
    }
}