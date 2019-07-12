using System;

namespace Surging.Core.CPlatform.Serialization.Implementation
{
    /// <summary>
    /// 基于string类型的object序列化器。
    /// </summary>
    public class StringObjectSerializer : ISerializer<object>
    {
        #region 字段

        /// <summary>
        /// Defines the _serializer
        /// </summary>
        private readonly ISerializer<string> _serializer;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="StringObjectSerializer"/> class.
        /// </summary>
        /// <param name="serializer">The serializer<see cref="ISerializer{string}"/></param>
        public StringObjectSerializer(ISerializer<string> serializer)
        {
            _serializer = serializer;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// 反序列化。
        /// </summary>
        /// <param name="content">序列化的内容。</param>
        /// <param name="type">对象类型。</param>
        /// <returns>一个对象实例。</returns>
        public object Deserialize(object content, Type type)
        {
            return _serializer.Deserialize(content.ToString(), type);
        }

        /// <summary>
        /// 序列化。
        /// </summary>
        /// <param name="instance">需要序列化的对象。</param>
        /// <returns>序列化之后的结果。</returns>
        public object Serialize(object instance)
        {
            return _serializer.Serialize(instance);
        }

        #endregion 方法
    }
}