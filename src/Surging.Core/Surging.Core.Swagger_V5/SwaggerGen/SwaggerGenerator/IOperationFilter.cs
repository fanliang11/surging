using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Surging.Core.CPlatform.Runtime.Server;

namespace Surging.Core.Swagger_V5.SwaggerGen
{
    public interface IOperationFilter
    {
        void Apply(OpenApiOperation operation, OperationFilterContext context);
    }

    public class OperationFilterContext
    {
        public OperationFilterContext(
            ApiDescription apiDescription,
            ISchemaGenerator schemaRegistry,
            SchemaRepository schemaRepository,
            MethodInfo methodInfo):this(apiDescription, schemaRegistry, schemaRepository, methodInfo,null)
        { 
        }

        public OperationFilterContext(
    ApiDescription apiDescription,
    ISchemaGenerator schemaRegistry,
    SchemaRepository schemaRepository,
    MethodInfo methodInfo,ServiceEntry serviceEntry)
        {
            ApiDescription = apiDescription;
            SchemaGenerator = schemaRegistry;
            SchemaRepository = schemaRepository;
            MethodInfo = methodInfo;
            ServiceEntry = serviceEntry;
        }

        public ServiceEntry ServiceEntry { get; set; }

        public ApiDescription ApiDescription { get; }

        public ISchemaGenerator SchemaGenerator { get; }

        public SchemaRepository SchemaRepository { get; }

        public MethodInfo MethodInfo { get; }

        public string DocumentName => SchemaRepository.DocumentName;
    }
}
