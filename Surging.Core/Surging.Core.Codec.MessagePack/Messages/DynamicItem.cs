using System;
using Surging.Core.Codec.MessagePack.Utilities;

namespace Surging.Core.Codec.MessagePack.Messages
{
    public class DynamicItem
    {
        #region Constructor

        public DynamicItem()
        { }

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

        public string TypeName { get; set; }

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