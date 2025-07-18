using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Surging.Core.Swagger_V5.SwaggerGen
{
    public static class TypeExtensions
    {
        public static string FriendlyId(this Type type, bool fullyQualified = false)
        {
            var typeName = fullyQualified
                ? type.FullNameSansTypeArguments().Replace("+", ".")
                : type.Name;

            if (type.GetTypeInfo().IsGenericType)
            {
                var genericArgumentIds = type.GetGenericArguments()
                    .Select(t => t.FriendlyId(fullyQualified))
                    .ToArray();

                return new StringBuilder(typeName)
                    .Replace(string.Format("`{0}", genericArgumentIds.Count()), string.Empty)
                    .Append(string.Format("[{0}]", string.Join(",", genericArgumentIds).TrimEnd(',')))
                    .ToString();
            }

            return typeName;
        }
        public static bool IsOneOf(this Type type, params Type[] possibleTypes)
        {
            return possibleTypes.Any(possibleType => possibleType == type);
        }

        public static bool IsAssignableTo(this Type type, Type baseType)
        {
            return baseType.IsAssignableFrom(type);
        }

        internal static bool IsFSharpOption(this Type type)
        {
            return type.FullNameSansTypeArguments() == "Microsoft.FSharp.Core.FSharpOption`1";
        }

        public static bool IsAssignableToOneOf(this Type type, params Type[] possibleBaseTypes)
        {
            return possibleBaseTypes.Any(possibleBaseType => possibleBaseType.IsAssignableFrom(type));
        }

        public static bool IsConstructedFrom(this Type type, Type genericType, out Type constructedType)
        {
            constructedType = new[] { type }
                .Union(type.GetInheritanceChain())
                .Union(type.GetInterfaces())
                .FirstOrDefault(i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == genericType);

            return (constructedType != null);
        }

        private static string FullNameSansTypeArguments(this Type type)
        {
            if (string.IsNullOrEmpty(type.FullName)) return string.Empty;

            var fullName = type.FullName;
            var chopIndex = fullName.IndexOf("[[");
            return (chopIndex == -1) ? fullName : fullName.Substring(0, chopIndex);
        }

        public static bool IsReferenceOrNullableType(this Type type)
        {
            return (!type.IsValueType || Nullable.GetUnderlyingType(type) != null);
        }

        public static object GetDefaultValue(this Type type)
        {
            return type.IsValueType
                ? Activator.CreateInstance(type)
                : null;
        }

        public static Type[] GetInheritanceChain(this Type type)
        {
            var inheritanceChain = new List<Type>();

            var current = type;
            while (current.BaseType != null)
            {
                inheritanceChain.Add(current.BaseType);
                current = current.BaseType;
            }

            return inheritanceChain.ToArray();
        }
    }
}
