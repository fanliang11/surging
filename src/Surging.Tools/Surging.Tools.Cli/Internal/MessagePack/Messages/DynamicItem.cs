using MessagePack;
using Newtonsoft.Json;
using Surging.Tools.Cli.Utilities;
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Surging.Tools.Cli.Internal.MessagePack.Messages
{
    [MessagePackObject]
    public class DynamicItem
    {
        #region Constructor

        public DynamicItem()
        { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DynamicItem(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var valueType = value.GetType();
            var code = Type.GetTypeCode(valueType);

            if (code != TypeCode.Object && valueType.BaseType!=typeof(Enum))
                TypeName = valueType.FullName;
            else
                TypeName = valueType.AssemblyQualifiedName;

            if (valueType == UtilityType.JObjectType || valueType == UtilityType.JArrayType)
                Content = SerializerUtilitys.Serialize(value.ToString());
            else if(valueType != typeof(CancellationToken))
                Content = SerializerUtilitys.Serialize(value);
        }

        #endregion Constructor

        #region Property

        [Key(0)]
        public string TypeName { get; set; }

        [Key(1)]
        public byte[] Content { get; set; }
        #endregion Property

        #region Public Method
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        #endregion Public Method
    }
}
