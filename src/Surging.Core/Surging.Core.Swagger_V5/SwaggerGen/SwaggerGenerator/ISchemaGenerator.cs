using System;
using System.Reflection;
using Microsoft.OpenApi.Models;

namespace Surging.Core.Swagger_V5.SwaggerGen
{
    public interface ISchemaGenerator
    {
        OpenApiSchema GenerateSchema(
            Type modelType,
            SchemaRepository schemaRepository,
            MemberInfo memberInfo = null,
            ParameterInfo parameterInfo = null);
    }
}
