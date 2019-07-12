using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.ApiGateWay.Configurations
{
    /// <summary>
    /// Defines the <see cref="ServicePart" />
    /// </summary>
    public class ServicePart
    {
        #region 属性

        /// <summary>
        /// Gets or sets a value indicating whether EnableAuthorization
        /// </summary>
        public bool EnableAuthorization { get; set; }

        /// <summary>
        /// Gets or sets the MainPath
        /// </summary>
        public string MainPath { get; set; } = "part/service/aggregation";

        /// <summary>
        /// Gets or sets the Services
        /// </summary>
        public List<Services> Services { get; set; }

        #endregion 属性
    }
}