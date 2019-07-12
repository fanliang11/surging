using AutoMapper;

namespace Surging.Core.AutoMapper
{
    /// <summary>
    /// Defines the <see cref="MapperExtensions" />
    /// </summary>
    public static class MapperExtensions
    {
        #region 方法

        /// <summary>
        /// The MapTo
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The obj<see cref="object"/></param>
        /// <returns>The <see cref="T"/></returns>
        public static T MapTo<T>(this object obj) where T : class
        {
            return Mapper.Map<T>(obj);
        }

        /// <summary>
        /// The MapTo
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="obj">The obj<see cref="TSource"/></param>
        /// <param name="entity">The entity<see cref="TDestination"/></param>
        /// <returns>The <see cref="TDestination"/></returns>
        public static TDestination MapTo<TSource, TDestination>(this TSource obj, TDestination entity) where TSource : class where TDestination : class
        {
            return Mapper.Map<TSource, TDestination>(obj, entity);
        }

        #endregion 方法
    }
}