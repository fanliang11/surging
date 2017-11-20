using System;

namespace Surging.Core.CPlatform.Serialization.Implementation
{
    /// <summary>
    /// 基于string类型的object序列化器。
    /// </summary>
    public class StringObjectSerializer : ISerializer<object>
    {
        #region Field

        private readonly ISerializer<string> _serializer;

        #endregion Field

        #region Constructor

        public StringObjectSerializer(ISerializer<string> serializer)
        {
            _serializer = serializer;
        }

        #endregion Constructor

        #region Implementation of ISerializer<object>

        /// <summary>
        /// 序列化。
        /// </summary>
        /// <param name="instance">需要序列化的对象。</param>
        /// <returns>序列化之后的结果。</returns>
        public object Serialize(object instance)
        {
            return _serializer.Serialize(instance);
        }

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

        #endregion Implementation of ISerializer<object>
    }
}