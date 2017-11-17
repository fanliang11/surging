using System;
using System.IO;

namespace Surging.Core.Codec.MessagePack.Utilities
{
    public static class SerializerUtilitys
    {
        static SerializerUtilitys()
        {
            global::MessagePack.MessagePackSerializer.SetDefaultResolver(
                global::MessagePack.Resolvers.ContractlessStandardResolver.Instance);
        }

        public static byte[] Serialize<T>(T instance)
        {
            using (var stream = new MemoryStream())
            {
                global::MessagePack.MessagePackSerializer.Serialize(stream, instance);
                return stream.ToArray();
            }
        }

        public static byte[] Serialize(object instance, Type type)
        {
            using (var stream = new MemoryStream())
            {
                global::MessagePack.MessagePackSerializer.Serialize(stream, instance);
                return stream.ToArray();
            }
        }

        public static object Deserialize(byte[] data, Type type)
        {
            if (data == null)
            {
                return null;
            }

            using (var stream = new MemoryStream(data))
            {
                return global::MessagePack.MessagePackSerializer.NonGeneric.Deserialize(type, stream);
            }
        }

        public static T Deserialize<T>(byte[] data)
        {
            if (data == null)
            {
                return default(T);
            }

            using (var stream = new MemoryStream(data))
            {
                return global::MessagePack.MessagePackSerializer.Deserialize<T>(stream);
            }
        }
    }
}