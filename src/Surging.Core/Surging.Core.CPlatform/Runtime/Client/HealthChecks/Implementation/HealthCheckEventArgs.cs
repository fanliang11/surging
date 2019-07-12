using Surging.Core.CPlatform.Address;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Runtime.Client.HealthChecks.Implementation
{
    /// <summary>
    /// Defines the <see cref="HealthCheckEventArgs" />
    /// </summary>
    public class HealthCheckEventArgs
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="HealthCheckEventArgs"/> class.
        /// </summary>
        /// <param name="address">The address<see cref="AddressModel"/></param>
        public HealthCheckEventArgs(AddressModel address)
        {
            Address = address;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HealthCheckEventArgs"/> class.
        /// </summary>
        /// <param name="address">The address<see cref="AddressModel"/></param>
        /// <param name="health">The health<see cref="bool"/></param>
        public HealthCheckEventArgs(AddressModel address, bool health)
        {
            Address = address;
            Health = health;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the Address
        /// </summary>
        public AddressModel Address { get; private set; }

        /// <summary>
        /// Gets a value indicating whether Health
        /// </summary>
        public bool Health { get; private set; }

        #endregion 属性
    }
}