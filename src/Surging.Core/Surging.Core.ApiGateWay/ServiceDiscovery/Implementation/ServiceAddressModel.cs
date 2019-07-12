using Surging.Core.CPlatform.Address;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.ApiGateWay.ServiceDiscovery.Implementation
{
    /// <summary>
    /// Defines the <see cref="ServiceAddressModel" />
    /// </summary>
    public class ServiceAddressModel
    {
        #region 属性

        /// <summary>
        /// Gets or sets the Address
        /// </summary>
        public AddressModel Address { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsHealth
        /// </summary>
        public bool IsHealth { get; set; }

        #endregion 属性
    }
}