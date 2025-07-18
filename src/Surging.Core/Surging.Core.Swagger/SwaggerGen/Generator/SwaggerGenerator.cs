﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Surging.Core.Swagger;
using Surging.Core.CPlatform.Runtime.Server;
using Microsoft.Extensions.DependencyInjection;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Utilities;
using Autofac;
using Microsoft.Extensions.Primitives;
using System.Threading.Tasks;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Surging.Core.SwaggerGen
{
    public class SwaggerGenerator : ISwaggerProvider
    {
        private readonly IApiDescriptionGroupCollectionProvider _apiDescriptionsProvider;
        private readonly ISchemaRegistryFactory _schemaRegistryFactory;
        private readonly SwaggerGeneratorOptions _options;
        private readonly IServiceEntryProvider _serviceEntryProvider;

        public SwaggerGenerator(
            IApiDescriptionGroupCollectionProvider apiDescriptionsProvider,
            ISchemaRegistryFactory schemaRegistryFactory,
            IOptions<SwaggerGeneratorOptions> optionsAccessor)
            : this (apiDescriptionsProvider, schemaRegistryFactory, optionsAccessor.Value)
        { }

        public SwaggerGenerator(
            IApiDescriptionGroupCollectionProvider apiDescriptionsProvider,
            ISchemaRegistryFactory schemaRegistryFactory,
            SwaggerGeneratorOptions options)
        {
            _apiDescriptionsProvider = apiDescriptionsProvider;
            _schemaRegistryFactory = schemaRegistryFactory;
            _options = options ?? new SwaggerGeneratorOptions();
            _serviceEntryProvider = ServiceLocator.Current.Resolve<IServiceEntryProvider>();
        }

        public SwaggerDocument GetSwagger(
            string documentName,
            string host = null,
            string basePath = null,
            string[] schemes = null)
        {
            if (!_options.SwaggerDocs.TryGetValue(documentName, out Info info))
                throw new UnknownSwaggerDocument(documentName);


            var mapRoutePaths = Swagger.AppConfig.SwaggerConfig.Options?.MapRoutePaths;
            var entries = _serviceEntryProvider.GetALLEntries();
            if (mapRoutePaths != null)
            {
                foreach (var path in mapRoutePaths)
                {
                    var entry = entries.Where(p => p.RoutePath == path.SourceRoutePath).FirstOrDefault();
                    if (entry != null)
                    {
                        entry.RoutePath = path.TargetRoutePath;
                        entry.Descriptor.RoutePath = path.TargetRoutePath;
                    }
                }
            }
            entries = entries
       .Where(apiDesc => _options.DocInclusionPredicateV2(documentName, apiDesc));

            var schemaRegistry = _schemaRegistryFactory.Create();

            var swaggerDoc = new SwaggerDocument
            {
                Info = info,
                Host = host,
                BasePath = basePath,
                Schemes = schemes,
                Paths = CreatePathItems(entries, schemaRegistry),
                Definitions = schemaRegistry.Definitions,
                SecurityDefinitions = _options.SecurityDefinitions.Any() ? _options.SecurityDefinitions : null,
                Security = _options.SecurityRequirements.Any() ? _options.SecurityRequirements : null
            };

            return swaggerDoc;
        }

        private Dictionary<string, PathItem> CreatePathItems(
            IEnumerable<ServiceEntry> apiDescriptions,
            ISchemaRegistry schemaRegistry)
        {

            return apiDescriptions
                .OrderBy(p => p.RoutePath)
                .GroupBy(apiDesc => apiDesc.Descriptor.RoutePath)
                .ToDictionary(entry =>
                     entry.Key.IndexOf("/") == 0 ? entry.Key : $"/{entry.Key}"
                     , entry => CreatePathItem(entry, schemaRegistry));
        }

        private Dictionary<string, PathItem> CreatePathItems(
          IEnumerable<ApiDescription> apiDescriptions,
          ISchemaRegistry schemaRegistry)
        {
            return apiDescriptions
                .OrderBy(_options.SortKeySelector)
                .GroupBy(apiDesc => apiDesc.RelativePathSansQueryString())
                .ToDictionary(group => "/" + group.Key, group => CreatePathItem(group, schemaRegistry));
        }

        private PathItem CreatePathItem(
            IEnumerable<ApiDescription> apiDescriptions,
            ISchemaRegistry schemaRegistry)
        {
            var pathItem = new PathItem();

            // Group further by http method
            var perMethodGrouping = apiDescriptions
                .GroupBy(apiDesc => apiDesc.HttpMethod);

            foreach (var group in perMethodGrouping)
            {
                var httpMethod = group.Key;

                if (httpMethod == null)
                    throw new NotSupportedException(string.Format(
                        "Ambiguous HTTP method for action - {0}. " +
                        "Actions require an explicit HttpMethod binding for Swagger 2.0",
                        group.First().ActionDescriptor.DisplayName));

                if (group.Count() > 1 && _options.ConflictingActionsResolver == null)
                    throw new NotSupportedException(string.Format(
                        "HTTP method \"{0}\" & path \"{1}\" overloaded by actions - {2}. " +
                        "Actions require unique method/path combination for Swagger 2.0. Use ConflictingActionsResolver as a workaround",
                        httpMethod,
                        group.First().RelativePathSansQueryString(),
                        string.Join(",", group.Select(apiDesc => apiDesc.ActionDescriptor.DisplayName))));

                var apiDescription = (group.Count() > 1) ? _options.ConflictingActionsResolver(group) : group.Single();

                switch (httpMethod)
                {
                    case "GET":
                        pathItem.Get = CreateOperation(apiDescription, schemaRegistry);
                        break;
                    case "PUT":
                        pathItem.Put = CreateOperation(apiDescription, schemaRegistry);
                        break;
                    case "POST":
                        pathItem.Post = CreateOperation(apiDescription, schemaRegistry);
                        break;
                    case "DELETE":
                        pathItem.Delete = CreateOperation(apiDescription, schemaRegistry);
                        break;
                    case "OPTIONS":
                        pathItem.Options = CreateOperation(apiDescription, schemaRegistry);
                        break;
                    case "HEAD":
                        pathItem.Head = CreateOperation(apiDescription, schemaRegistry);
                        break;
                    case "PATCH":
                        pathItem.Patch = CreateOperation(apiDescription, schemaRegistry);
                        break;
                }
            }

            return pathItem;
        }

        private PathItem CreatePathItem(
          IEnumerable<ServiceEntry> serviceEntries, ISchemaRegistry schemaRegistry)
        {
            var pathItem = new PathItem();
            foreach (var entry in serviceEntries)
            {
                var methodInfo = entry.Type.GetTypeInfo().DeclaredMethods.Where(p => p.Name == entry.MethodName).FirstOrDefault();
                var parameterInfo = methodInfo.GetParameters(); 

                if (entry.Methods.Count() ==0)
                {
                    if (parameterInfo != null && parameterInfo.Any(p =>
                  !UtilityType.ConvertibleType.GetTypeInfo().IsAssignableFrom(p.ParameterType)))
                        pathItem.Post = CreateOperation(entry, methodInfo, schemaRegistry);
                    else
                        pathItem.Get = CreateOperation(entry, methodInfo, schemaRegistry);
                }
                else
                {
                    foreach (var httpMethod in entry.Methods)
                    {
                        switch (httpMethod)
                        {
                            case "GET":
                                pathItem.Get = CreateOperation(entry, methodInfo, schemaRegistry);
                                break;
                            case "PUT":
                                pathItem.Put = CreateOperation(entry, methodInfo, schemaRegistry);
                                break;
                            case "POST":
                                pathItem.Post = CreateOperation(entry, methodInfo, schemaRegistry);
                                break;
                            case "DELETE":
                                pathItem.Delete = CreateOperation(entry, methodInfo, schemaRegistry);
                                break;
                            case "OPTIONS":
                                pathItem.Options = CreateOperation(entry, methodInfo, schemaRegistry);
                                break;
                            case "HEAD":
                                pathItem.Head = CreateOperation(entry, methodInfo, schemaRegistry);
                                break;
                            case "PATCH":
                                pathItem.Patch = CreateOperation(entry, methodInfo, schemaRegistry);
                                break;
                        }
                    }
                }
            }
            return pathItem;
        }

        private Operation CreateOperation(ServiceEntry serviceEntry, MethodInfo methodInfo, ISchemaRegistry schemaRegistry)
        { 
            var customAttributes = Enumerable.Empty<object>(); 
            if (methodInfo !=null)
            {
                customAttributes = methodInfo.GetCustomAttributes(true)
                    .Union(methodInfo.DeclaringType.GetTypeInfo().GetCustomAttributes(true));
            }
            var isDeprecated = customAttributes.Any(attr => attr.GetType() == typeof(ObsoleteAttribute));

            var operation = new Operation
            {
                Tags = new[] { serviceEntry.Type.Name },
                OperationId = serviceEntry.Descriptor.Id, 
                Parameters= CreateParameters(serviceEntry, methodInfo,schemaRegistry),
                Deprecated = isDeprecated ? true : (bool?)null,
                Responses = CreateResponses(serviceEntry,methodInfo, schemaRegistry),

            };

            var filterContext = new OperationFilterContext(
             null,
             schemaRegistry,
             methodInfo,serviceEntry);

            foreach (var filter in _options.OperationFilters)
            {
                filter.Apply(operation, filterContext);
            }
            return operation;
        }

            private Operation CreateOperation(
            ApiDescription apiDescription,
            ISchemaRegistry schemaRegistry)
        {
            // Try to retrieve additional metadata that's not provided by ApiExplorer
            MethodInfo methodInfo;
            var customAttributes = Enumerable.Empty<object>();

            if (apiDescription.TryGetMethodInfo(out methodInfo))
            {
                customAttributes = methodInfo.GetCustomAttributes(true)
                    .Union(methodInfo.DeclaringType.GetTypeInfo().GetCustomAttributes(true));
            }

            var isDeprecated = customAttributes.Any(attr => attr.GetType() == typeof(ObsoleteAttribute));

            var operation = new Operation
            {
                OperationId = _options.OperationIdSelector(apiDescription),
                Tags = _options.TagsSelector(apiDescription),
                Consumes = CreateConsumes(apiDescription, customAttributes),
                Produces = CreateProduces(apiDescription, customAttributes),
                Parameters = CreateParameters(apiDescription, schemaRegistry),
                Responses = CreateResponses(apiDescription, schemaRegistry),
                Deprecated = isDeprecated ? true : (bool?)null
            };

            // Assign default value for Consumes if not yet assigned AND operation contains form params
            if (operation.Consumes.Count() == 0 && operation.Parameters.Any(p => p.In == "formData"))
            {
                operation.Consumes.Add("multipart/form-data");
            }

            var filterContext = new OperationFilterContext(
                apiDescription,
                schemaRegistry,
                methodInfo);

            foreach (var filter in _options.OperationFilters)
            {
                filter.Apply(operation, filterContext);
            }

            return operation;
        }

        private IList<string> CreateConsumes(ApiDescription apiDescription, IEnumerable<object> customAttributes)
        {
            var consumesAttribute = customAttributes.OfType<ConsumesAttribute>().FirstOrDefault();

            var mediaTypes = (consumesAttribute != null)
                ? consumesAttribute.ContentTypes
                : apiDescription.SupportedRequestFormats
                    .Select(apiRequestFormat => apiRequestFormat.MediaType);

            return mediaTypes.ToList();
        }

        private IList<string> CreateProduces(ApiDescription apiDescription, IEnumerable<object> customAttributes)
        {
            var producesAttribute = customAttributes.OfType<ProducesAttribute>().FirstOrDefault();

            var mediaTypes = (producesAttribute != null)
                ? producesAttribute.ContentTypes
                : apiDescription.SupportedResponseTypes
                    .SelectMany(apiResponseType => apiResponseType.ApiResponseFormats)
                    .Select(apiResponseFormat => apiResponseFormat.MediaType)
                    .Distinct();

            return mediaTypes.ToList();
        }

        private IList<IParameter> CreateParameters(
            ApiDescription apiDescription,
            ISchemaRegistry schemaRegistry)
        {
            var applicableParamDescriptions = apiDescription.ParameterDescriptions
                .Where(paramDesc =>
                {
                    return paramDesc.Source.IsFromRequest
                        && (paramDesc.ModelMetadata == null || paramDesc.ModelMetadata.IsBindingAllowed);
                });

            return applicableParamDescriptions
                .Select(paramDesc => CreateParameter(apiDescription, paramDesc, schemaRegistry))
                .ToList();
        }

        private IList<IParameter> CreateParameters(ServiceEntry serviceEntry, MethodInfo methodInfo, ISchemaRegistry schemaRegistry)
        {
            ParameterInfo[] parameterInfo = null;
            if (methodInfo != null)
            {
                parameterInfo = methodInfo.GetParameters();

            };
            if (parameterInfo.Count() > 1)
            {
                return parameterInfo != null && parameterInfo.Any(p =>
                !UtilityType.ConvertibleType.GetTypeInfo().IsAssignableFrom(p.ParameterType) && p.ParameterType.Name != "HttpFormCollection")
               ? new List<IParameter> { CreateServiceKeyParameter() }.Union(new List<IParameter> { CreateBodyParameter(parameterInfo, schemaRegistry) }).ToList() :
              new List<IParameter> { CreateServiceKeyParameter() }.Union(parameterInfo.Select(p => CreateNonBodyParameter(serviceEntry, p, schemaRegistry))).ToList();
            }
            else
            {
                return parameterInfo != null && parameterInfo.Any(p =>
                 !UtilityType.ConvertibleType.GetTypeInfo().IsAssignableFrom(p.ParameterType) && p.ParameterType.Name != "HttpFormCollection")
                ? new List<IParameter> { CreateServiceKeyParameter() }.Union(parameterInfo.Select(p => CreateBodyParameter(p, schemaRegistry))).ToList() :
               new List<IParameter> { CreateServiceKeyParameter() }.Union(parameterInfo.Select(p => CreateNonBodyParameter(serviceEntry, p, schemaRegistry))).ToList();
            }
        }

        private IParameter CreateBodyParameter(ParameterInfo  parameterInfo, ISchemaRegistry schemaRegistry)
        {
            
            var schema = schemaRegistry.GetOrRegister(parameterInfo.Name,typeof(IDictionary<,>).MakeGenericType(typeof(string), parameterInfo.ParameterType));
            return  new BodyParameter { Name = parameterInfo.Name,Schema=schema, Required = true };
        }

        private IParameter CreateBodyParameter(ParameterInfo[] parameterInfo, ISchemaRegistry schemaRegistry)
        {
            var schema = schemaRegistry.GetOrRegister(parameterInfo[0].Name, typeof(IDictionary<,>).MakeGenericType(typeof(string), parameterInfo[0].ParameterType));
            for (int i = 1; i < parameterInfo.Length; i++)
                schema.Properties.Add(parameterInfo[i].Name, (schemaRegistry.GetOrRegister(null,  parameterInfo[i].ParameterType)));
            return new BodyParameter { Name = "parameters", Schema = schema, Required = true };
        }

        private IParameter CreateServiceKeyParameter()
        {
            var nonBodyParam = new NonBodyParameter
            {
                Name = "servicekey",
                In = "query",
                Required = false,
            };
            var schema = new Schema();
            schema.Description = "ServiceKey";
            nonBodyParam.PopulateFrom(schema);
            return nonBodyParam;
        }

        private IParameter CreateNonBodyParameter(ServiceEntry serviceEntry, ParameterInfo parameterInfo, ISchemaRegistry schemaRegistry)
        {
            string reg = @"(?<={)[^{}]*(?=})";
            var nonBodyParam = new NonBodyParameter
            {
                Name = parameterInfo.Name,
                In = "query",
                Required = true,
            };
            if (Regex.IsMatch(serviceEntry.RoutePath, reg) && GetParameters(serviceEntry.RoutePath).Contains(parameterInfo.Name))
            {
                nonBodyParam.In = "path";
            }

            if (parameterInfo.ParameterType == null)
            {
                nonBodyParam.Type = "string";
            }
            else if (typeof(IEnumerable<KeyValuePair<string, StringValues>>).IsAssignableFrom(parameterInfo.ParameterType) &&
                parameterInfo.ParameterType.Name== "HttpFormCollection")
            {
                nonBodyParam.Type = "file";
                nonBodyParam.In = "formData";
            }
            else
            {
                // Retrieve a Schema object for the type and copy common fields onto the parameter
                var schema = schemaRegistry.GetOrRegister(parameterInfo.ParameterType);

                // NOTE: While this approach enables re-use of SchemaRegistry logic, it introduces complexity
                // and constraints elsewhere (see below) and needs to be refactored!

                if (schema.Ref != null)
                {
                    // The registry created a referenced Schema that needs to be located. This means it's not neccessarily
                    // exclusive to this parameter and so, we can't assign any parameter specific attributes or metadata.
                    schema = schemaRegistry.Definitions[schema.Ref.Replace("#/definitions/", string.Empty)];
                }
                else
                {
                    // It's a value Schema. This means it's exclusive to this parameter and so, we can assign
                    // parameter specific attributes and metadata. Yep - it's hacky! 
                    schema.Default = (parameterInfo != null && parameterInfo.IsOptional)
                        ? parameterInfo.DefaultValue
                        : null;
                }

                nonBodyParam.PopulateFrom(schema);
            }
            return nonBodyParam;
        }

        private static List<string> GetParameters(string text)
        {
            var matchVale = new List<string>();
            string Reg = @"(?<={)[^{}]*(?=})";
            string key = string.Empty;
            foreach (Match m in Regex.Matches(text, Reg))
            {
                matchVale.Add(m.Value);
            }
            return matchVale;
        }

        private IParameter CreateParameter(
            ApiDescription apiDescription,
            ApiParameterDescription apiParameterDescription,
            ISchemaRegistry schemaRegistry)
        {
            // Try to retrieve additional metadata that's not directly provided by ApiExplorer
            ParameterInfo parameterInfo = null;
            PropertyInfo propertyInfo = null;
            var customAttributes = Enumerable.Empty<object>();

            if (apiParameterDescription.TryGetParameterInfo(apiDescription, out parameterInfo))
                customAttributes = parameterInfo.GetCustomAttributes(true);
            else if (apiParameterDescription.TryGetPropertyInfo(out propertyInfo))
                customAttributes = propertyInfo.GetCustomAttributes(true);

            var name = _options.DescribeAllParametersInCamelCase
                ? apiParameterDescription.Name.ToCamelCase()
                : apiParameterDescription.Name;

            var isRequired = customAttributes.Any(attr =>
                new[] { typeof(RequiredAttribute), typeof(BindRequiredAttribute) }.Contains(attr.GetType()));

            var parameter = (apiParameterDescription.Source == BindingSource.Body)
                ? CreateBodyParameter(
                    apiParameterDescription,
                    name,
                    isRequired,
                    schemaRegistry)
                : CreateNonBodyParameter(
                    apiParameterDescription,
                    parameterInfo,
                    customAttributes,
                    name,
                    isRequired,
                    schemaRegistry);

            var filterContext = new ParameterFilterContext(
                apiParameterDescription,
                schemaRegistry,
                parameterInfo,
                propertyInfo);

            foreach (var filter in _options.ParameterFilters)
            {
                filter.Apply(parameter, filterContext);
            }

            return parameter;
        }

        private IParameter CreateBodyParameter(
            ApiParameterDescription apiParameterDescription,
            string name,
            bool isRequired,
            ISchemaRegistry schemaRegistry)
        {
            var schema = schemaRegistry.GetOrRegister(apiParameterDescription.Type);

            return new BodyParameter { Name = name, Schema = schema, Required = isRequired };
        }

        private IParameter CreateNonBodyParameter(
            ApiParameterDescription apiParameterDescription,
            ParameterInfo parameterInfo,
            IEnumerable<object> customAttributes,
            string name,
            bool isRequired,
            ISchemaRegistry schemaRegistry)
        {
            var location = ParameterLocationMap.ContainsKey(apiParameterDescription.Source)
                ? ParameterLocationMap[apiParameterDescription.Source]
                : "query";

            var nonBodyParam = new NonBodyParameter
            {
                Name = name,
                In = location,
                Required = (location == "path") ? true : isRequired,
            };

            if (apiParameterDescription.Type == null)
            {
                nonBodyParam.Type = "string";
            }
            else if (typeof(IFormFile).IsAssignableFrom(apiParameterDescription.Type))
            {
                nonBodyParam.Type = "file";
            }
            else
            {
                // Retrieve a Schema object for the type and copy common fields onto the parameter
                var schema = schemaRegistry.GetOrRegister(apiParameterDescription.Type);

                // NOTE: While this approach enables re-use of SchemaRegistry logic, it introduces complexity
                // and constraints elsewhere (see below) and needs to be refactored!

                if (schema.Ref != null)
                {
                    // The registry created a referenced Schema that needs to be located. This means it's not neccessarily
                    // exclusive to this parameter and so, we can't assign any parameter specific attributes or metadata.
                    schema = schemaRegistry.Definitions[schema.Ref.Replace("#/definitions/", string.Empty)];
                }
                else
                {
                    // It's a value Schema. This means it's exclusive to this parameter and so, we can assign
                    // parameter specific attributes and metadata. Yep - it's hacky!
                    schema.AssignAttributeMetadata(customAttributes);
                    schema.Default = (parameterInfo != null && parameterInfo.IsOptional)
                        ? parameterInfo.DefaultValue
                        : null;
                }

                nonBodyParam.PopulateFrom(schema);
            }

            return nonBodyParam;
        }

        private IDictionary<string, Response> CreateResponses(
            ApiDescription apiDescription,
            ISchemaRegistry schemaRegistry)
        {
            var supportedApiResponseTypes = apiDescription.SupportedResponseTypes
                .DefaultIfEmpty(new ApiResponseType { StatusCode = 200 });

            return supportedApiResponseTypes
                .ToDictionary(
                    apiResponseType => apiResponseType.IsDefaultResponse() ? "default" : apiResponseType.StatusCode.ToString(),
                    apiResponseType => CreateResponse(apiResponseType, schemaRegistry));
        }

        private IDictionary<string, Response> CreateResponses(
    ServiceEntry apiDescription,
    MethodInfo methodInfo,
    ISchemaRegistry schemaRegistry)
        {
            return  new Dictionary<string, Response> {
                { "200", CreateResponse(apiDescription,methodInfo, schemaRegistry) }
            };
        }

        private Response CreateResponse(ServiceEntry apiResponseType,MethodInfo methodInfo, ISchemaRegistry schemaRegistry)
        {
            var description = ResponseDescriptionMap
                .FirstOrDefault((entry) => Regex.IsMatch("200", entry.Key))
                .Value;

            return new Response
            {
                Description = description,
                Schema = (methodInfo.ReturnType != typeof(Task) && methodInfo.ReturnType != typeof(void))
                    ? schemaRegistry.GetOrRegister(typeof(HttpResultMessage<>).MakeGenericType(methodInfo.ReturnType.GenericTypeArguments))
                    : null
            };
        }

        private Response CreateResponse(ApiResponseType apiResponseType, ISchemaRegistry schemaRegistry)
        {
            var description = ResponseDescriptionMap
                .FirstOrDefault((entry) => Regex.IsMatch(apiResponseType.StatusCode.ToString(), entry.Key))
                .Value;

            return new Response
            {
                Description = description,
                Schema = (apiResponseType.Type != null && apiResponseType.Type != typeof(void))
                    ? schemaRegistry.GetOrRegister(apiResponseType.Type)
                    : null
            };
        }

        private static Dictionary<BindingSource, string> ParameterLocationMap = new Dictionary<BindingSource, string>
        {
            { BindingSource.Form, "formData" },
            { BindingSource.FormFile, "formData" },
            { BindingSource.Body, "body" },
            { BindingSource.Header, "header" },
            { BindingSource.Path, "path" },
            { BindingSource.Query, "query" }
        };

        private static readonly Dictionary<string, string> ResponseDescriptionMap = new Dictionary<string, string>
        {
            { "1\\d{2}", "Information" },
            { "2\\d{2}", "Success" },
            { "3\\d{2}", "Redirect" },
            { "400", "Bad Request" },
            { "401", "Unauthorized" },
            { "403", "Forbidden" },
            { "404", "Not Found" },
            { "405", "Method Not Allowed" },
            { "406", "Not Acceptable" },
            { "408", "Request Timeout" },
            { "409", "Conflict" },
            { "4\\d{2}", "Client Error" },
            { "5\\d{2}", "Server Error" }
        };
    }
}