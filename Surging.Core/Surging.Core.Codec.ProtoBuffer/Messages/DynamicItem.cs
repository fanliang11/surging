using ProtoBuf;
using Surging.Core.Codec.ProtoBuffer.Utilities;
using System;

namespace Surging.Core.Codec.ProtoBuffer.Messages
{
    [ProtoContract]
    public class DynamicItem
    {
        #region Constructor

        public DynamicItem()
        {
        }

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

            Content = SerializerUtilitys.Serialize(value);
        }

        #endregion Constructor

        #region Property

        [ProtoMember(1)]
        public string TypeName { get; set; }
        [ProtoMember(2)]
        public byte[] Content { get; set; }
        #endregion Property

        #region Public Method
        public object Get()
        {
            if (Content == null || TypeName == null)
                return null;

            return SerializerUtilitys.Deserialize(Content, Type.GetType(TypeName));
        }
        #endregion Public Method
    }
}