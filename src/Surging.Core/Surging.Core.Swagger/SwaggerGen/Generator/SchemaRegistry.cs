using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Surging.Core.Swagger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Surging.Core.SwaggerGen
{
    /// <summary>
    /// Defines the <see cref="SchemaRegistry" />
    /// </summary>
    public class SchemaRegistry : ISchemaRegistry
    {
        #region 字段

        /// <summary>
        /// Defines the PrimitiveTypeMap
        /// </summary>
        private static readonly Dictionary<Type, Func<Schema>> PrimitiveTypeMap = new Dictionary<Type, Func<Schema>>
        {
            { typeof(short), () => new Schema { Type = "integer", Format = "int32" } },
            { typeof(ushort), () => new Schema { Type = "integer", Format = "int32" } },
            { typeof(int), () => new Schema { Type = "integer", Format = "int32" } },
            { typeof(uint), () => new Schema { Type = "integer", Format = "int32" } },
            { typeof(long), () => new Schema { Type = "integer", Format = "int64" } },
            { typeof(ulong), () => new Schema { Type = "integer", Format = "int64" } },
            { typeof(float), () => new Schema { Type = "number", Format = "float" } },
            { typeof(double), () => new Schema { Type = "number", Format = "double" } },
            { typeof(decimal), () => new Schema { Type = "number", Format = "double" } },
            { typeof(byte), () => new Schema { Type = "integer", Format = "int32" } },
            { typeof(sbyte), () => new Schema { Type = "integer", Format = "int32" } },
            { typeof(byte[]), () => new Schema { Type = "string", Format = "byte" } },
            { typeof(sbyte[]), () => new Schema { Type = "string", Format = "byte" } },
            { typeof(bool), () => new Schema { Type = "boolean" } },
            { typeof(DateTime), () => new Schema { Type = "string", Format = "date-time" } },
            { typeof(DateTimeOffset), () => new Schema { Type = "string", Format = "date-time" } },
            { typeof(Guid), () => new Schema { Type = "string", Format = "uuid" } }
        };

        /// <summary>
        /// Defines the _jsonContractResolver
        /// </summary>
        private readonly IContractResolver _jsonContractResolver;

        /// <summary>
        /// Defines the _jsonSerializerSettings
        /// </summary>
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        /// <summary>
        /// Defines the _options
        /// </summary>
        private readonly SchemaRegistryOptions _options;

        /// <summary>
        /// Defines the _schemaIdManager
        /// </summary>
        private readonly SchemaIdManager _schemaIdManager;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaRegistry"/> class.
        /// </summary>
        /// <param name="jsonSerializerSettings">The jsonSerializerSettings<see cref="JsonSerializerSettings"/></param>
        /// <param name="options">The options<see cref="SchemaRegistryOptions"/></param>
        public SchemaRegistry(
            JsonSerializerSettings jsonSerializerSettings,
            SchemaRegistryOptions options = null)
        {
            _jsonSerializerSettings = jsonSerializerSettings;
            _jsonContractResolver = _jsonSerializerSettings.ContractResolver ?? new DefaultContractResolver();
            _options = options ?? new SchemaRegistryOptions();
            _schemaIdManager = new SchemaIdManager(_options.SchemaIdSelector);
            Definitions = new Dictionary<string, Schema>();
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the Definitions
        /// </summary>
        public IDictionary<string, Schema> Definitions { get; private set; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The GetOrRegister
        /// </summary>
        /// <param name="paramName">The paramName<see cref="string"/></param>
        /// <param name="type">The type<see cref="Type"/></param>
        /// <returns>The <see cref="Schema"/></returns>
        public Schema GetOrRegister(string paramName, Type type)
        {
            var referencedTypes = new Queue<Type>();
            var schema = CreateSchema(paramName, type, referencedTypes);

            // Ensure all referenced types have a corresponding definition
            while (referencedTypes.Any())
            {
                var referencedType = referencedTypes.Dequeue();
                var schemaId = _schemaIdManager.IdFor(referencedType);
                if (Definitions.ContainsKey(schemaId)) continue;

                // NOTE: Add the schemaId first with a null value. This indicates a work-in-progress
                // and prevents a stack overflow by ensuring the above condition is met if the same
                // type ends up back on the referencedTypes queue via recursion within 'CreateInlineSchema'
                Definitions.Add(schemaId, null);
                Definitions[schemaId] = CreateInlineSchema(paramName, referencedType, referencedTypes);
            }

            return schema;
        }

        /// <summary>
        /// The GetOrRegister
        /// </summary>
        /// <param name="type">The type<see cref="Type"/></param>
        /// <returns>The <see cref="Schema"/></returns>
        public Schema GetOrRegister(Type type)
       => GetOrRegister(null, type);

        /// <summary>
        /// The CreateArraySchema
        /// </summary>
        /// <param name="arrayContract">The arrayContract<see cref="JsonArrayContract"/></param>
        /// <param name="referencedTypes">The referencedTypes<see cref="Queue{Type}"/></param>
        /// <returns>The <see cref="Schema"/></returns>
        private Schema CreateArraySchema(JsonArrayContract arrayContract, Queue<Type> referencedTypes)
        {
            var type = arrayContract.UnderlyingType;
            var itemType = arrayContract.CollectionItemType ?? typeof(object);

            var isASet = (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(ISet<>)
                || type.GetInterfaces().Any(i => i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(ISet<>)));

            return new Schema
            {
                Type = "array",
                Items = CreateSchema(itemType, referencedTypes),
                UniqueItems = isASet
            };
        }

        /// <summary>
        /// The CreateDictionarySchema
        /// </summary>
        /// <param name="paramName">The paramName<see cref="string"/></param>
        /// <param name="dictionaryContract">The dictionaryContract<see cref="JsonDictionaryContract"/></param>
        /// <param name="referencedTypes">The referencedTypes<see cref="Queue{Type}"/></param>
        /// <returns>The <see cref="Schema"/></returns>
        private Schema CreateDictionarySchema(string paramName, JsonDictionaryContract dictionaryContract, Queue<Type> referencedTypes)
        {
            var keyType = dictionaryContract.DictionaryKeyType ?? typeof(object);
            var valueType = dictionaryContract.DictionaryValueType ?? typeof(object);

            if (keyType.GetTypeInfo().IsEnum)
            {
                return new Schema
                {
                    Type = "object",
                    Properties = Enum.GetNames(keyType).ToDictionary(
                        (name) => dictionaryContract.DictionaryKeyResolver(name),
                        (name) => CreateSchema(valueType, referencedTypes)
                    )
                };
            }
            else if (!string.IsNullOrEmpty(paramName))
            {
                return new Schema
                {
                    Type = "object",
                    Properties = new Dictionary<string, Schema> { {paramName,
                       CreateSchema(valueType, referencedTypes) } }
                };
            }
            else
            {
                return new Schema
                {
                    Type = "object",
                    AdditionalProperties = CreateSchema(valueType, referencedTypes)
                };
            }
        }

        /// <summary>
        /// The CreateEnumSchema
        /// </summary>
        /// <param name="primitiveContract">The primitiveContract<see cref="JsonPrimitiveContract"/></param>
        /// <param name="type">The type<see cref="Type"/></param>
        /// <returns>The <see cref="Schema"/></returns>
        private Schema CreateEnumSchema(JsonPrimitiveContract primitiveContract, Type type)
        {
            var stringEnumConverter = primitiveContract.Converter as StringEnumConverter
                ?? _jsonSerializerSettings.Converters.OfType<StringEnumConverter>().FirstOrDefault();

            if (_options.DescribeAllEnumsAsStrings || stringEnumConverter != null)
            {
                var camelCase = _options.DescribeStringEnumsInCamelCase
                    || (stringEnumConverter != null && stringEnumConverter.CamelCaseText);

                var enumNames = type.GetFields(BindingFlags.Public | BindingFlags.Static)
                    .Select(f =>
                    {
                        var name = f.Name;

                        var enumMemberAttribute = f.GetCustomAttributes().OfType<EnumMemberAttribute>().FirstOrDefault();
                        if (enumMemberAttribute != null && enumMemberAttribute.Value != null)
                        {
                            name = enumMemberAttribute.Value;
                        }

                        return camelCase ? name.ToCamelCase() : name;
                    });

                return new Schema
                {
                    Type = "string",
                    Enum = enumNames.ToArray()
                };
            }

            return new Schema
            {
                Type = "integer",
                Format = "int32",
                Enum = Enum.GetValues(type).Cast<object>().ToArray()
            };
        }

        /// <summary>
        /// The CreateInlineSchema
        /// </summary>
        /// <param name="paramName">The paramName<see cref="string"/></param>
        /// <param name="type">The type<see cref="Type"/></param>
        /// <param name="referencedTypes">The referencedTypes<see cref="Queue{Type}"/></param>
        /// <returns>The <see cref="Schema"/></returns>
        private Schema CreateInlineSchema(string paramName, Type type, Queue<Type> referencedTypes)
        {
            Schema schema;

            var jsonContract = _jsonContractResolver.ResolveContract(type);

            if (_options.CustomTypeMappings.ContainsKey(type))
            {
                schema = _options.CustomTypeMappings[type]();
            }
            else
            {
                // TODO: Perhaps a "Chain of Responsibility" would clean this up a little?
                if (jsonContract is JsonPrimitiveContract)
                    schema = CreatePrimitiveSchema((JsonPrimitiveContract)jsonContract);
                else if (jsonContract is JsonDictionaryContract)
                    schema = CreateDictionarySchema(paramName, (JsonDictionaryContract)jsonContract, referencedTypes);
                else if (jsonContract is JsonArrayContract)
                    schema = CreateArraySchema((JsonArrayContract)jsonContract, referencedTypes);
                else if (jsonContract is JsonObjectContract && type != typeof(object))
                    schema = CreateObjectSchema((JsonObjectContract)jsonContract, referencedTypes);
                else
                    // None of the above, fallback to abstract "object"
                    schema = new Schema { Type = "object" };
            }

            var filterContext = new SchemaFilterContext(type, jsonContract, this);
            foreach (var filter in _options.SchemaFilters)
            {
                filter.Apply(schema, filterContext);
            }

            return schema;
        }

        /// <summary>
        /// The CreateObjectSchema
        /// </summary>
        /// <param name="jsonContract">The jsonContract<see cref="JsonObjectContract"/></param>
        /// <param name="referencedTypes">The referencedTypes<see cref="Queue{Type}"/></param>
        /// <returns>The <see cref="Schema"/></returns>
        private Schema CreateObjectSchema(JsonObjectContract jsonContract, Queue<Type> referencedTypes)
        {
            var applicableJsonProperties = jsonContract.Properties
                .Where(prop => !prop.Ignored)
                .Where(prop => !(_options.IgnoreObsoleteProperties && prop.IsObsolete()))
                .Select(prop => prop);

            var required = applicableJsonProperties
                .Where(prop => prop.IsRequired())
                .Select(propInfo => propInfo.PropertyName)
                .ToList();

            var hasExtensionData = jsonContract.ExtensionDataValueType != null;

            var properties = applicableJsonProperties
                .ToDictionary(
                    prop => prop.PropertyName,
                    prop => CreatePropertySchema(prop, referencedTypes));

            var schema = new Schema
            {
                Required = required.Any() ? required : null, // required can be null but not empty
                Properties = properties,
                AdditionalProperties = hasExtensionData ? new Schema { Type = "object" } : null,
                Type = "object",
            };

            return schema;
        }

        /// <summary>
        /// The CreatePrimitiveSchema
        /// </summary>
        /// <param name="primitiveContract">The primitiveContract<see cref="JsonPrimitiveContract"/></param>
        /// <returns>The <see cref="Schema"/></returns>
        private Schema CreatePrimitiveSchema(JsonPrimitiveContract primitiveContract)
        {
            // If Nullable<T>, use the type argument
            var type = primitiveContract.UnderlyingType.IsNullable()
                ? Nullable.GetUnderlyingType(primitiveContract.UnderlyingType)
                : primitiveContract.UnderlyingType;

            if (type.GetTypeInfo().IsEnum)
                return CreateEnumSchema(primitiveContract, type);

            if (PrimitiveTypeMap.ContainsKey(type))
                return PrimitiveTypeMap[type]();

            // None of the above, fallback to string
            return new Schema { Type = "string" };
        }

        /// <summary>
        /// The CreatePropertySchema
        /// </summary>
        /// <param name="jsonProperty">The jsonProperty<see cref="JsonProperty"/></param>
        /// <param name="referencedTypes">The referencedTypes<see cref="Queue{Type}"/></param>
        /// <returns>The <see cref="Schema"/></returns>
        private Schema CreatePropertySchema(JsonProperty jsonProperty, Queue<Type> referencedTypes)
        {
            var schema = CreateSchema(jsonProperty.PropertyType, referencedTypes);

            if (!jsonProperty.Writable)
                schema.ReadOnly = true;

            if (jsonProperty.TryGetMemberInfo(out MemberInfo memberInfo))
                schema.AssignAttributeMetadata(memberInfo.GetCustomAttributes(true));

            return schema;
        }

        /// <summary>
        /// The CreateReferenceSchema
        /// </summary>
        /// <param name="type">The type<see cref="Type"/></param>
        /// <param name="referencedTypes">The referencedTypes<see cref="Queue{Type}"/></param>
        /// <returns>The <see cref="Schema"/></returns>
        private Schema CreateReferenceSchema(Type type, Queue<Type> referencedTypes)
        {
            referencedTypes.Enqueue(type);
            return new Schema { Ref = "#/definitions/" + _schemaIdManager.IdFor(type) };
        }

        /// <summary>
        /// The CreateSchema
        /// </summary>
        /// <param name="paramName">The paramName<see cref="string"/></param>
        /// <param name="type">The type<see cref="Type"/></param>
        /// <param name="referencedTypes">The referencedTypes<see cref="Queue{Type}"/></param>
        /// <returns>The <see cref="Schema"/></returns>
        private Schema CreateSchema(string paramName, Type type, Queue<Type> referencedTypes)
        {
            // If Option<T> (F#), use the type argument
            if (type.IsFSharpOption())
                type = type.GetGenericArguments()[0];

            var jsonContract = _jsonContractResolver.ResolveContract(type);

            var createReference = !_options.CustomTypeMappings.ContainsKey(type)
                && type != typeof(object)
                && (// Type describes an object
                    jsonContract is JsonObjectContract ||
                    // Type is self-referencing
                    jsonContract.IsSelfReferencingArrayOrDictionary() ||
                    // Type is enum and opt-in flag set
                    (type.GetTypeInfo().IsEnum && _options.UseReferencedDefinitionsForEnums));

            return createReference
                ? CreateReferenceSchema(type, referencedTypes)
                : CreateInlineSchema(paramName, type, referencedTypes);
        }

        /// <summary>
        /// The CreateSchema
        /// </summary>
        /// <param name="type">The type<see cref="Type"/></param>
        /// <param name="referencedTypes">The referencedTypes<see cref="Queue{Type}"/></param>
        /// <returns>The <see cref="Schema"/></returns>
        private Schema CreateSchema(Type type, Queue<Type> referencedTypes) =>
            CreateSchema(null, type, referencedTypes);

        #endregion 方法
    }
}