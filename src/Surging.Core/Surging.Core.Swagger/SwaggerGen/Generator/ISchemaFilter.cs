using Newtonsoft.Json.Serialization;
using Surging.Core.Swagger;
using System;

namespace Surging.Core.SwaggerGen
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="ISchemaFilter" />
    /// </summary>
    public interface ISchemaFilter
    {
        #region 方法

        /// <summary>
        /// The Apply
        /// </summary>
        /// <param name="schema">The schema<see cref="Schema"/></param>
        /// <param name="context">The context<see cref="SchemaFilterContext"/></param>
        void Apply(Schema schema, SchemaFilterContext context);

        #endregion 方法
    }

    #endregion 接口

    /// <summary>
    /// Defines the <see cref="SchemaFilterContext" />
    /// </summary>
    public class SchemaFilterContext
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaFilterContext"/> class.
        /// </summary>
        /// <param name="systemType">The systemType<see cref="Type"/></param>
        /// <param name="jsonContract">The jsonContract<see cref="JsonContract"/></param>
        /// <param name="schemaRegistry">The schemaRegistry<see cref="ISchemaRegistry"/></param>
        public SchemaFilterContext(
            Type systemType,
            JsonContract jsonContract,
            ISchemaRegistry schemaRegistry)
        {
            SystemType = systemType;
            JsonContract = jsonContract;
            SchemaRegistry = schemaRegistry;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the JsonContract
        /// </summary>
        public JsonContract JsonContract { get; private set; }

        /// <summary>
        /// Gets the SchemaRegistry
        /// </summary>
        public ISchemaRegistry SchemaRegistry { get; private set; }

        /// <summary>
        /// Gets the SystemType
        /// </summary>
        public Type SystemType { get; private set; }

        #endregion 属性
    }
}