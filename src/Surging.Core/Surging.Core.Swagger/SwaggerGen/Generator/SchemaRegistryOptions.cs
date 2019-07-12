using Surging.Core.Swagger;
using System;
using System.Collections.Generic;

namespace Surging.Core.SwaggerGen
{
    /// <summary>
    /// Defines the <see cref="SchemaRegistryOptions" />
    /// </summary>
    public class SchemaRegistryOptions
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaRegistryOptions"/> class.
        /// </summary>
        public SchemaRegistryOptions()
        {
            CustomTypeMappings = new Dictionary<Type, Func<Schema>>();
            SchemaIdSelector = (type) => type.FriendlyId(IgnoreFullyQualified);
            SchemaFilters = new List<ISchemaFilter>();
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the CustomTypeMappings
        /// </summary>
        public IDictionary<Type, Func<Schema>> CustomTypeMappings { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether DescribeAllEnumsAsStrings
        /// </summary>
        public bool DescribeAllEnumsAsStrings { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether DescribeStringEnumsInCamelCase
        /// </summary>
        public bool DescribeStringEnumsInCamelCase { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IgnoreFullyQualified
        /// </summary>
        public bool IgnoreFullyQualified { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IgnoreObsoleteProperties
        /// </summary>
        public bool IgnoreObsoleteProperties { get; set; }

        /// <summary>
        /// Gets or sets the SchemaFilters
        /// </summary>
        public IList<ISchemaFilter> SchemaFilters { get; set; }

        /// <summary>
        /// Gets or sets the SchemaIdSelector
        /// </summary>
        public Func<Type, string> SchemaIdSelector { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether UseReferencedDefinitionsForEnums
        /// </summary>
        public bool UseReferencedDefinitionsForEnums { get; set; }

        #endregion 属性
    }
}