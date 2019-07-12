using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Surging.Core.SwaggerGen
{
    /// <summary>
    /// Defines the <see cref="SchemaRegistryFactory" />
    /// </summary>
    public class SchemaRegistryFactory : ISchemaRegistryFactory
    {
        #region 字段

        /// <summary>
        /// Defines the _jsonSerializerSettings
        /// </summary>
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        /// <summary>
        /// Defines the _schemaRegistryOptions
        /// </summary>
        private readonly SchemaRegistryOptions _schemaRegistryOptions;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaRegistryFactory"/> class.
        /// </summary>
        /// <param name="mvcJsonOptionsAccessor">The mvcJsonOptionsAccessor<see cref="IOptions{MvcJsonOptions}"/></param>
        /// <param name="schemaRegistryOptionsAccessor">The schemaRegistryOptionsAccessor<see cref="IOptions{SchemaRegistryOptions}"/></param>
        public SchemaRegistryFactory(
            IOptions<MvcJsonOptions> mvcJsonOptionsAccessor,
            IOptions<SchemaRegistryOptions> schemaRegistryOptionsAccessor)
            : this(mvcJsonOptionsAccessor.Value.SerializerSettings, schemaRegistryOptionsAccessor.Value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaRegistryFactory"/> class.
        /// </summary>
        /// <param name="jsonSerializerSettings">The jsonSerializerSettings<see cref="JsonSerializerSettings"/></param>
        /// <param name="schemaRegistryOptions">The schemaRegistryOptions<see cref="SchemaRegistryOptions"/></param>
        public SchemaRegistryFactory(
            JsonSerializerSettings jsonSerializerSettings,
            SchemaRegistryOptions schemaRegistryOptions)
        {
            _jsonSerializerSettings = jsonSerializerSettings;
            _schemaRegistryOptions = schemaRegistryOptions;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The Create
        /// </summary>
        /// <returns>The <see cref="ISchemaRegistry"/></returns>
        public ISchemaRegistry Create()
        {
            return new SchemaRegistry(_jsonSerializerSettings, _schemaRegistryOptions);
        }

        #endregion 方法
    }
}