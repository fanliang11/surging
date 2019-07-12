using Surging.Core.CPlatform.Routing;
using System.Collections.Generic;

namespace Surging.Core.CPlatform.Runtime.Client
{
    /// <summary>
    /// Defines the <see cref="ServiceSubscriberDescriptor" />
    /// </summary>
    public class ServiceSubscriberDescriptor
    {
        #region 属性

        /// <summary>
        /// Gets or sets the AddressDescriptors
        /// 服务地址描述符集合。
        /// </summary>
        public IEnumerable<ServiceAddressDescriptor> AddressDescriptors { get; set; }

        /// <summary>
        /// Gets or sets the ServiceDescriptor
        /// 服务描述符。
        /// </summary>
        public ServiceDescriptor ServiceDescriptor { get; set; }

        #endregion 属性
    }
}