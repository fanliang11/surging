
using Microsoft.OpenApi.Models;
using Surging.Core.CPlatform.Filters.Implementation;
using Surging.Core.Swagger_V5.Swagger.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Swagger_V5.SwaggerGen.Filters
{
    public class AddAuthorizationOperationFilter : IOperationFilter
    {

        public AddAuthorizationOperationFilter()
        {
        }
 

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
            {
                operation.Parameters = new List<OpenApiParameter>();
            }


            var attribute =
                 context.ServiceEntry.Attributes.Where(p => p is AuthorizationAttribute)
                 .Select(p => p as AuthorizationAttribute).FirstOrDefault();
            if (attribute != null && attribute.AuthType == AuthorizationType.JWT)
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Required = false,
                    Schema =  new OpenApiSchema { 
                    
                     Type= "string"
                    }
                });
            }
            else if (attribute != null && attribute.AuthType == AuthorizationType.AppSecret)
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Required = false,
                    Schema = new OpenApiSchema
                    {
                        Type = "string"
                    }
                });
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "timeStamp",
                    In = ParameterLocation.Query,
                    Required = false,
                    Schema = new OpenApiSchema
                    {
                        Type = "string"
                    }
                });
            }
        }
    }
}
