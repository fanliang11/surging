using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.ApiGateWay.Configurations
{
    /// <summary>
    /// Defines the <see cref="ServiceAggregation" />
    /// </summary>
    public class ServiceAggregation
    {
        #region 属性

        /// <summary>
        /// Gets or sets the Key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the Params
        /// </summary>
        public Dictionary<string, object> Params { get; set; }

        /// <summary>
        /// Gets or sets the RoutePath
        /// </summary>
        public string RoutePath { get; set; }

        /// <summary>
        /// Gets or sets the ServiceKey
        /// </summary>
        public string ServiceKey { get; set; }

        #endregion 属性
    }
}