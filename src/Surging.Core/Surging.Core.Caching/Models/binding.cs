using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Caching.Models
{
    /// <summary>
    /// Defines the <see cref="Binding" />
    /// </summary>
    public class Binding
    {
        #region 属性

        /// <summary>
        /// Gets or sets the Class
        /// </summary>
        public string Class { get; set; }

        /// <summary>
        /// Gets or sets the Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the InitMethod
        /// </summary>
        public string InitMethod { get; set; }

        /// <summary>
        /// Gets or sets the Maps
        /// </summary>
        public List<Map> Maps { get; set; }

        /// <summary>
        /// Gets or sets the Properties
        /// </summary>
        public List<Property> Properties { get; set; }

        #endregion 属性
    }
}