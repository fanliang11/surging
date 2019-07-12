using Newtonsoft.Json;
using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Mqtt
{
    /// <summary>
    /// Mqtt地址描述符。
    /// </summary>
    public class MqttEndpointDescriptor
    {
        #region 属性

        /// <summary>
        /// Gets or sets the Type
        /// 地址类型。
        /// </summary>
        [JsonIgnore]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the Value
        /// 地址值。
        /// </summary>
        public string Value { get; set; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// 创建一个描述符。
        /// </summary>
        /// <typeparam name="T">地址模型类型。</typeparam>
        /// <param name="address">地址模型实例。</param>
        /// <param name="serializer">序列化器。</param>
        /// <returns>Mqtt地址描述符。</returns>
        public static MqttEndpointDescriptor CreateDescriptor<T>(T address, ISerializer<string> serializer) where T : AddressModel, new()
        {
            return new MqttEndpointDescriptor
            {
                Type = typeof(T).FullName,
                Value = serializer.Serialize(address)
            };
        }

        #endregion 方法
    }

    /// <summary>
    /// Defines the <see cref="MqttServiceDescriptor" />
    /// </summary>
    public class MqttServiceDescriptor
    {
        #region 属性

        /// <summary>
        /// Gets or sets the AddressDescriptors
        /// Mqtt地址描述符集合。
        /// </summary>
        public IEnumerable<MqttEndpointDescriptor> AddressDescriptors { get; set; }

        /// <summary>
        /// Gets or sets the MqttDescriptor
        /// Mqtt描述符。
        /// </summary>
        public MqttDescriptor MqttDescriptor { get; set; }

        #endregion 属性
    }
}