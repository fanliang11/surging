using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Surging.Core.CPlatform.Mqtt
{
    /// <summary>
    /// 服务描述符。
    /// </summary>
    [Serializable]
    public class MqttDescriptor
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="MqttDescriptor"/> class.
        /// </summary>
        public MqttDescriptor()
        {
            Metadatas = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the Metadatas
        /// 元数据。
        /// </summary>
        public IDictionary<string, object> Metadatas { get; set; }

        /// <summary>
        /// Gets or sets the Topic
        /// Topic。
        /// </summary>
        public string Topic { get; set; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The Equals
        /// </summary>
        /// <param name="obj">The object to compare with the current object. </param>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            var model = obj as MqttDescriptor;
            if (model == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            if (model.Topic != Topic)
                return false;

            return model.Metadatas.Count == Metadatas.Count && model.Metadatas.All(metadata =>
            {
                object value;
                if (!Metadatas.TryGetValue(metadata.Key, out value))
                    return false;

                if (metadata.Value == null && value == null)
                    return true;
                if (metadata.Value == null || value == null)
                    return false;

                return metadata.Value.Equals(value);
            });
        }

        /// <summary>
        /// The GetHashCode
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        /// <summary>
        /// 获取一个元数据。
        /// </summary>
        /// <typeparam name="T">元数据类型。</typeparam>
        /// <param name="name">元数据名称。</param>
        /// <param name="def">如果指定名称的元数据不存在则返回这个参数。</param>
        /// <returns>元数据值。</returns>
        public T GetMetadata<T>(string name, T def = default(T))
        {
            if (!Metadatas.ContainsKey(name))
                return def;

            return (T)Metadatas[name];
        }

        #endregion 方法

        public static bool operator ==(MqttDescriptor model1, MqttDescriptor model2)
        {
            return Equals(model1, model2);
        }

        public static bool operator !=(MqttDescriptor model1, MqttDescriptor model2)
        {
            return !Equals(model1, model2);
        }
    }

    /// <summary>
    /// 服务描述符扩展方法。
    /// </summary>
    public static class MqttDescriptorExtensions
    {
    }
}