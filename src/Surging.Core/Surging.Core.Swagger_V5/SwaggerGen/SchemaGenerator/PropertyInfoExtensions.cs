using System;
using System.Reflection;

namespace Surging.Core.Swagger_V5.SwaggerGen
{
    public static class PropertyInfoExtensions
    {
        public static bool HasAttribute<TAttribute>(this PropertyInfo property)
            where TAttribute : Attribute
        {
            return property.GetCustomAttribute<TAttribute>() != null;
        }

        public static bool IsPubliclyReadable(this PropertyInfo property)
        {
            return property.GetMethod?.IsPublic == true;
        }

        public static bool IsPubliclyWritable(this PropertyInfo property)
        {
            return property.SetMethod?.IsPublic == true;
        }
    }
}
