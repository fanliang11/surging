using MessagePack;
using MessagePack.Resolvers;
using System;
using System.IO;

namespace Surging.Core.Codec.MessagePack.Utilities
{
    /// <summary>
    /// Defines the <see cref="SerializerUtilitys" />
    /// </summary>
    public class SerializerUtilitys
    {
        #region 构造函数

        /// <summary>
        /// Initializes static members of the <see cref="SerializerUtilitys"/> class.
        /// </summary>
        static SerializerUtilitys()
        {
            CompositeResolver.RegisterAndSetAsDefault(NativeDateTimeResolver.Instance, ContractlessStandardResolverAllowPrivate.Instance);
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The Deserialize
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The data<see cref="byte[]"/></param>
        /// <returns>The <see cref="T"/></returns>
        public static T Deserialize<T>(byte[] data)
        {
            return data == null ? default(T) : MessagePackSerializer.Deserialize<T>(data);
        }

        /// <summary>
        /// The Deserialize
        /// </summary>
        /// <param name="data">The data<see cref="byte[]"/></param>
        /// <param name="type">The type<see cref="Type"/></param>
        /// <returns>The <see cref="object"/></returns>
        public static object Deserialize(byte[] data, Type type)
        {
            return data == null ? null : MessagePackSerializer.NonGeneric.Deserialize(type, data);
        }

        /// <summary>
        /// The Serialize
        /// </summary>
        /// <param name="instance">The instance<see cref="object"/></param>
        /// <param name="type">The type<see cref="Type"/></param>
        /// <returns>The <see cref="byte[]"/></returns>
        public static byte[] Serialize(object instance, Type type)
        {
            return MessagePackSerializer.Serialize(instance);
        }

        /// <summary>
        /// The Serialize
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance">The instance<see cref="T"/></param>
        /// <returns>The <see cref="byte[]"/></returns>
        public static byte[] Serialize<T>(T instance)
        {
            return MessagePackSerializer.Serialize(instance);
        }

        #endregion 方法
    }
}