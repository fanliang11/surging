using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Surging.Core.SwaggerGen
{
    /// <summary>
    /// Defines the <see cref="JsonPropertyExtensions" />
    /// </summary>
    internal static class JsonPropertyExtensions
    {
        #region 方法

        /// <summary>
        /// The HasAttribute
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonProperty">The jsonProperty<see cref="JsonProperty"/></param>
        /// <returns>The <see cref="bool"/></returns>
        internal static bool HasAttribute<T>(this JsonProperty jsonProperty)
            where T : Attribute
        {
            if (!jsonProperty.TryGetMemberInfo(out MemberInfo memberInfo))
                return false;

            return memberInfo.GetCustomAttribute<T>() != null;
        }

        /// <summary>
        /// The IsObsolete
        /// </summary>
        /// <param name="jsonProperty">The jsonProperty<see cref="JsonProperty"/></param>
        /// <returns>The <see cref="bool"/></returns>
        internal static bool IsObsolete(this JsonProperty jsonProperty)
        {
            return jsonProperty.HasAttribute<ObsoleteAttribute>();
        }

        /// <summary>
        /// The IsRequired
        /// </summary>
        /// <param name="jsonProperty">The jsonProperty<see cref="JsonProperty"/></param>
        /// <returns>The <see cref="bool"/></returns>
        internal static bool IsRequired(this JsonProperty jsonProperty)
        {
            if (jsonProperty.Required == Newtonsoft.Json.Required.AllowNull)
                return true;

            if (jsonProperty.Required == Newtonsoft.Json.Required.Always)
                return true;

            if (jsonProperty.HasAttribute<RequiredAttribute>())
                return true;

            return false;
        }

        /// <summary>
        /// The TryGetMemberInfo
        /// </summary>
        /// <param name="jsonProperty">The jsonProperty<see cref="JsonProperty"/></param>
        /// <param name="memberInfo">The memberInfo<see cref="MemberInfo"/></param>
        /// <returns>The <see cref="bool"/></returns>
        internal static bool TryGetMemberInfo(this JsonProperty jsonProperty, out MemberInfo memberInfo)
        {
            if (jsonProperty.UnderlyingName == null)
            {
                memberInfo = null;
                return false;
            }

            var metadataAttribute = jsonProperty.DeclaringType.GetTypeInfo()
                .GetCustomAttributes(typeof(ModelMetadataTypeAttribute), true)
                .FirstOrDefault();

            var typeToReflect = (metadataAttribute != null)
                ? ((ModelMetadataTypeAttribute)metadataAttribute).MetadataType
                : jsonProperty.DeclaringType;

            memberInfo = typeToReflect.GetMember(jsonProperty.UnderlyingName).FirstOrDefault();

            return (memberInfo != null);
        }

        #endregion 方法
    }
}