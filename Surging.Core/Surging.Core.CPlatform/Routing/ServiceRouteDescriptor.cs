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
        public string Type { get; set; }

        /// <summary>
        /// 地址值。
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 创建一个描述符。
        /// </summary>
        /// <typeparam name="T">地址模型类型。</typeparam>
        /// <param name="address">地址模型实例。</param>
        /// <param name="serializer">序列化器。</param>
        /// <returns>服务地址描述符。</returns>
        public static ServiceAddressDescriptor CreateDescriptor<T>(T address, ISerializer<string> serializer) where T : AddressModel, new()
        {
            return new ServiceAddressDescriptor
            {
                Type = typeof(T).FullName,
                Value = serializer.Serialize(address)
            };
        }
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
