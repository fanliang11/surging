using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProtoBuf;
using Surging.Core.Codec.ProtoBuffer.Utilities;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Reflection;

namespace Surging.Core.Codec.ProtoBuffer.Messages
{
    /// <summary>
    /// Defines the <see cref="DynamicItem" />
    /// </summary>
    [ProtoContract]
    public class DynamicItem
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicItem"/> class.
        /// </summary>
        public DynamicItem()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicItem"/> class.
        /// </summary>
        /// <param name="value">The value<see cref="object"/></param>
        public DynamicItem(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var valueType = value.GetType();
            var code = Type.GetTypeCode(valueType);

            if (code != TypeCode.Object)
                TypeName = valueType.FullName;
            else
                TypeName = valueType.AssemblyQualifiedName;
            if (valueType == UtilityType.JObjectType || valueType == UtilityType.JArrayType)
                Content = SerializerUtilitys.Serialize(value.ToString());
            else
                Content = SerializerUtilitys.Serialize(value);
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the Content
        /// </summary>
        [ProtoMember(2)]
        public byte[] Content { get; set; }

        /// <summary>
        /// Gets or sets the TypeName
        /// </summary>
        [ProtoMember(1)]
        public string TypeName { get; set; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The Get
        /// </summary>
        /// <returns>The <see cref="object"/></returns>
        public object Get()
        {
            if (Content == null || TypeName == null)
                return null;
            var typeName = Type.GetType(TypeName);
            if (typeName == UtilityType.JObjectType || typeName == UtilityType.JArrayType)
            {
                var content = SerializerUtilitys.Deserialize<string>(Content);
                return JsonConvert.DeserializeObject(content, typeName);
            }
            else
            {
                return SerializerUtilitys.Deserialize(Content, typeName);
            }
        }

        #endregion 方法
    }
}