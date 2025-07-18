﻿using System;
using Newtonsoft.Json.Serialization;
using Surging.Core.Swagger;

namespace Surging.Core.SwaggerGen
{
    public interface ISchemaFilter
    {
        void Apply(Schema schema, SchemaFilterContext context);
    }

    public class SchemaFilterContext
    {
        public SchemaFilterContext(
            Type systemType,
            JsonContract jsonContract,
            ISchemaRegistry schemaRegistry)
        {
            SystemType = systemType;
            JsonContract = jsonContract;
            SchemaRegistry = schemaRegistry;
        }

        public Type SystemType { get; private set; }

        public JsonContract JsonContract { get; private set; }

        public ISchemaRegistry SchemaRegistry { get; private set; }
    }
}