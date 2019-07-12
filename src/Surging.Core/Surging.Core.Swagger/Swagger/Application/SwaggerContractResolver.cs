using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Reflection;

namespace Surging.Core.Swagger
{
    /// <summary>
    /// Defines the <see cref="SwaggerContractResolver" />
    /// </summary>
    public class SwaggerContractResolver : DefaultContractResolver
    {
        #region 字段

        /// <summary>
        /// Defines the _applicationTypeConverter
        /// </summary>
        private readonly JsonConverter _applicationTypeConverter;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="SwaggerContractResolver"/> class.
        /// </summary>
        /// <param name="applicationSerializerSettings">The applicationSerializerSettings<see cref="JsonSerializerSettings"/></param>
        public SwaggerContractResolver(JsonSerializerSettings applicationSerializerSettings)
        {
            NamingStrategy = new CamelCaseNamingStrategy { ProcessDictionaryKeys = false };
            _applicationTypeConverter = new ApplicationTypeConverter(applicationSerializerSettings);
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The CreateProperty
        /// </summary>
        /// <param name="member">The member<see cref="MemberInfo"/></param>
        /// <param name="memberSerialization">The memberSerialization<see cref="MemberSerialization"/></param>
        /// <returns>The <see cref="JsonProperty"/></returns>
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var jsonProperty = base.CreateProperty(member, memberSerialization);

            if (member.Name == "Example" || member.Name == "Examples" || member.Name == "Default")
                jsonProperty.Converter = _applicationTypeConverter;

            return jsonProperty;
        }

        #endregion 方法

        /// <summary>
        /// Defines the <see cref="ApplicationTypeConverter" />
        /// </summary>
        private class ApplicationTypeConverter : JsonConverter
        {
            #region 字段

            /// <summary>
            /// Defines the _applicationTypeSerializer
            /// </summary>
            private JsonSerializer _applicationTypeSerializer;

            #endregion 字段

            #region 构造函数

            /// <summary>
            /// Initializes a new instance of the <see cref="ApplicationTypeConverter"/> class.
            /// </summary>
            /// <param name="applicationSerializerSettings">The applicationSerializerSettings<see cref="JsonSerializerSettings"/></param>
            public ApplicationTypeConverter(JsonSerializerSettings applicationSerializerSettings)
            {
                _applicationTypeSerializer = JsonSerializer.Create(applicationSerializerSettings);
            }

            #endregion 构造函数

            #region 方法

            /// <summary>
            /// The CanConvert
            /// </summary>
            /// <param name="objectType">The objectType<see cref="Type"/></param>
            /// <returns>The <see cref="bool"/></returns>
            public override bool CanConvert(Type objectType)
            {
                return true;
            }

            /// <summary>
            /// The ReadJson
            /// </summary>
            /// <param name="reader">The reader<see cref="JsonReader"/></param>
            /// <param name="objectType">The objectType<see cref="Type"/></param>
            /// <param name="existingValue">The existingValue<see cref="object"/></param>
            /// <param name="serializer">The serializer<see cref="JsonSerializer"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// The WriteJson
            /// </summary>
            /// <param name="writer">The writer<see cref="JsonWriter"/></param>
            /// <param name="value">The value<see cref="object"/></param>
            /// <param name="serializer">The serializer<see cref="JsonSerializer"/></param>
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                _applicationTypeSerializer.Serialize(writer, value);
            }

            #endregion 方法
        }
    }
}