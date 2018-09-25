using Newtonsoft.Json;
using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Serialization;
using System;
using System.Collections.Generic;

namespace Surging.Core.CPlatform.Routing
{
    /// <summary>
    /// 服务地址描述符。
    /// </summary>
    public class ServiceAddressDescriptor
    {
        /// <summary>
        /// 地址类型。
        /// </summary>
        [JsonIgnore]
        public string Type { get; set; }

        /// <summary>
        /// 地址值。
        /// </summary>
        public string Value { get; set; }

        
    }

    /// <summary>
    /// 服务路由描述符。
    /// </summary>
    public class ServiceRouteDescriptor
    {
        /// <summary>
        /// 服务地址描述符集合。
        /// </summary>
        public IEnumerable<ServiceAddressDescriptor> AddressDescriptors { get; set; }

        /// <summary>
        /// 服务描述符。
        /// </summary>
        public ServiceDescriptor ServiceDescriptor { get; set; }
    }
}
