using MessagePack;
using MessagePack.Resolvers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Surging.Core.Codec.MessagePack.Utilities
{
    public class SerializerUtilitys
    {
        static SerializerUtilitys()
        {
            MessagePackSerializer.SetDefaultResolver(
            ContractlessStandardResolver.Instance);
        }

        public static byte[] Serialize<T>(T instance)
        {
            using (var stream = new MemoryStream())
            {
               MessagePackSerializer.Serialize(stream, instance);
                return stream.ToArray();
            }
        }

        public static byte[] Serialize(object instance, Type type)
        {
            using (var stream = new MemoryStream())
            {
                MessagePackSerializer.Serialize(stream, instance);
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
                return MessagePackSerializer.NonGeneric.Deserialize(type, stream);
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
                return MessagePackSerializer.Deserialize<T>(stream);
            }
        }
    }
}
