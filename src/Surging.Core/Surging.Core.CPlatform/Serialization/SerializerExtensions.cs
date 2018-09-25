namespace Surging.Core.CPlatform.Serialization
{
    /// <summary>
    /// 添加<see cref="ISerializer"/>扩展方法。
    /// </summary>
    public static class SerializerExtensions
    {
        /// <summary>
        /// 反序列化方法
        /// </summary>
        /// <typeparam name="T">序列化类型。</typeparam>
        /// <typeparam name="TResult">返回对象类型。</typeparam>
        /// <param name="serializer"><see cref="ISerializer"/>对象</param>
        /// <param name="content">源对象</param>
        /// <returns>返回反序列化对象</returns>
        public static TResult Deserialize<T, TResult>(this ISerializer<T> serializer, T content)
        {
            return (TResult)serializer.Deserialize(content, typeof(TResult));
        }
    }
}
