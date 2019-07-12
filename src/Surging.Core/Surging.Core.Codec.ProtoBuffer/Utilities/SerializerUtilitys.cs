using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Surging.Core.Codec.ProtoBuffer.Utilities
{
    /// <summary>
    /// Defines the <see cref="SerializerUtilitys" />
    /// </summary>
    public static class SerializerUtilitys
    {
        #region 方法

        /// <summary>
        /// The Deserialize
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The data<see cref="byte[]"/></param>
        /// <returns>The <see cref="T"/></returns>
        public static T Deserialize<T>(byte[] data)
        {
            if (data == null)
                return default(T);
            using (var stream = new MemoryStream(data))
            {
                return Serializer.Deserialize<T>(stream);
            }
        }

        /// <summary>
        /// The Deserialize
        /// </summary>
        /// <param name="data">The data<see cref="byte[]"/></param>
        /// <param name="type">The type<see cref="Type"/></param>
        /// <returns>The <see cref="object"/></returns>
        public static object Deserialize(byte[] data, Type type)
        {
            if (data == null)
                return null;
            using (var stream = new MemoryStream(data))
            {
                return Serializer.Deserialize(type, stream);
            }
        }

        /// <summary>
        /// The Serialize
        /// </summary>
        /// <param name="instance">The instance<see cref="object"/></param>
        /// <returns>The <see cref="byte[]"/></returns>
        public static byte[] Serialize(object instance)
        {
            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, instance);
                return stream.ToArray();
            }
        }

        #endregion 方法
    }
}