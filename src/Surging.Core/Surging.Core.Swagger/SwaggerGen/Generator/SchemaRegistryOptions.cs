using System;
using System.Collections.Generic;
using Surging.Core.Swagger;

namespace Surging.Core.SwaggerGen
{
    public class SchemaRegistryOptions
    {
        public SchemaRegistryOptions()
        {
            CustomTypeMappings = new Dictionary<Type, Func<Schema>>();
            SchemaIdSelector = (type) => type.FriendlyId(IgnoreFullyQualified);
            SchemaFilters = new List<ISchemaFilter>();
        }

        public IDictionary<Type, Func<Schema>> CustomTypeMappings { get; set; }

        public bool DescribeAllEnumsAsStrings { get; set; }

        public bool DescribeStringEnumsInCamelCase { get; set; }

        public bool UseReferencedDefinitionsForEnums { get; set; }

        public Func<Type, string> SchemaIdSelector { get; set; }

        public bool IgnoreFullyQualified { get; set; }

        public bool IgnoreObsoleteProperties { get; set; }

        public IList<ISchemaFilter> SchemaFilters { get; set; }
    }
}