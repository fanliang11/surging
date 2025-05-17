using MessagePack;
using MessagePack.Resolvers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Tools.Cli.Utilities
{
    public class SerializerUtilitys
    {
        private static MessagePackSerializerOptions options = null;
        static SerializerUtilitys()
        {
            //   var resolver = CompositeResolver.Create(ContractlessStandardResolver.Instance, NativeDateTimeResolver.Instance, StandardResolver.Instance);
            //  options = MessagePackSerializerOptions.Standard.WithResolver(resolver);
            StaticCompositeResolver.Instance.Register(ContractlessStandardResolver.Instance, NativeDateTimeResolver.Instance, StandardResolver.Instance);
            var options = MessagePackSerializerOptions.Standard.WithResolver(StaticCompositeResolver.Instance);
            MessagePackSerializer.DefaultOptions = options;
        }

        public static byte[] Serialize<T>(T instance)
        {
            return MessagePackSerializer.Serialize(instance);
        }

        public static byte[] Serialize(object instance, Type type)
        {
            return MessagePackSerializer.Serialize(type, instance);
        }

        public static object Deserialize(byte[] data, Type type)
        {
            return data == null ? null : MessagePackSerializer.Deserialize(type, data);
        }

        public static T Deserialize<T>(byte[] data)
        {
            return data == null ? default(T) : MessagePackSerializer.Deserialize<T>(data);
        }
    }
}