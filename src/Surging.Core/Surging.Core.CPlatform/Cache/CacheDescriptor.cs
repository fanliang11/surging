using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Surging.Core.CPlatform.Cache
{
    /// <summary>
    /// 服务描述符。
    /// </summary>
    [Serializable]
    public class CacheDescriptor
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheDescriptor"/> class.
        /// </summary>
        public CacheDescriptor()
        {
            Metadatas = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the Id
        /// 缓存Id。
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the Metadatas
        /// 元数据。
        /// </summary>
        public IDictionary<string, object> Metadatas { get; set; }

        /// <summary>
        /// Gets or sets the Prefix
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// Gets or sets the Type
        /// 类型
        /// </summary>
        public string Type { get; set; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The Equals
        /// </summary>
        /// <param name="obj">The object to compare with the current object. </param>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            var model = obj as CacheDescriptor;
            if (model == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            if (model.Id != Id)
                return false;

            return model.Metadatas.Count == Metadatas.Count && model.Metadatas.All(metadata =>
            {
                object value;
                if (!Metadatas.TryGetValue(metadata.Key, out value))
                    return false;

                if (metadata.Value == null && value == null)
                    return true;
                if (metadata.Value == null || value == null)
                    return false;

                return metadata.Value.Equals(value);
            });
        }

        /// <summary>
        /// The GetHashCode
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        /// <summary>
        /// 获取一个元数据。
        /// </summary>
        /// <typeparam name="T">元数据类型。</typeparam>
        /// <param name="name">元数据名称。</param>
        /// <param name="def">如果指定名称的元数据不存在则返回这个参数。</param>
        /// <returns>元数据值。</returns>
        public T GetMetadata<T>(string name, T def = default(T))
        {
            if (!Metadatas.ContainsKey(name))
                return def;

            return (T)Metadatas[name];
        }

        #endregion 方法

        public static bool operator ==(CacheDescriptor model1, CacheDescriptor model2)
        {
            return Equals(model1, model2);
        }

        public static bool operator !=(CacheDescriptor model1, CacheDescriptor model2)
        {
            return !Equals(model1, model2);
        }
    }

    /// <summary>
    /// 服务描述符扩展方法。
    /// </summary>
    public static class CacheDescriptorExtensions
    {
        #region 方法

        /// <summary>
        /// 获取连接超时时间。
        /// </summary>
        /// <param name="descriptor">缓存描述述符。</param>
        /// <returns>缓存描述符。</returns>
        public static int ConnectTimeout(this CacheDescriptor descriptor)
        {
            return descriptor.GetMetadata<int>("ConnectTimeout", 60);
        }

        /// <summary>
        /// 设置连接超时时间。
        /// </summary>
        /// <param name="descriptor">缓存描述述符。</param>
        /// <param name="connectTimeout">The connectTimeout<see cref="int"/></param>
        /// <returns>缓存描述符。</returns>
        public static CacheDescriptor ConnectTimeout(this CacheDescriptor descriptor, int connectTimeout)
        {
            descriptor.Metadatas["ConnectTimeout"] = connectTimeout;
            return descriptor;
        }

        /// <summary>
        /// 获取默认失效时间 。
        /// </summary>
        /// <param name="descriptor">缓存描述符。</param>
        /// <returns>失效时间。</returns>
        public static int DefaultExpireTime(this CacheDescriptor descriptor)
        {
            return descriptor.GetMetadata<int>("DefaultExpireTime", 60);
        }

        /// <summary>
        /// 设置默认失效时间。
        /// </summary>
        /// <param name="descriptor">缓存描述述符。</param>
        /// <param name="defaultExpireTime">The defaultExpireTime<see cref="int"/></param>
        /// <returns>缓存描述符。</returns>
        public static CacheDescriptor DefaultExpireTime(this CacheDescriptor descriptor, int defaultExpireTime)
        {
            descriptor.Metadatas["DefaultExpireTime"] = defaultExpireTime;
            return descriptor;
        }

        #endregion 方法
    }
}