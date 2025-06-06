using AutoMapper;

namespace Surging.Core.AutoMapper
{
    public static class MapperExtensions
    {
        public static T MapTo<T>(this object obj) where T : class
        {
            return Mapper.Map<T>(obj);
        }

        public static TDestination MapTo<TSource, TDestination>(this TSource obj, TDestination entity) where TSource : class where TDestination : class
        {
            return Mapper.Map<TSource, TDestination>(obj, entity);
        }
    }
}
