using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.ApiGateWay.Configurations
{
    /// <summary>
    /// Defines the <see cref="Services" />
    /// </summary>
    public class Services
    {
        #region 属性

        /// <summary>
        /// Gets or sets the serviceAggregation
        /// </summary>
        public List<ServiceAggregation> serviceAggregation { get; set; }

        /// <summary>
        /// Gets or sets the UrlMapping
        /// </summary>
        public string UrlMapping { get; set; }

        #endregion 属性
    }
}