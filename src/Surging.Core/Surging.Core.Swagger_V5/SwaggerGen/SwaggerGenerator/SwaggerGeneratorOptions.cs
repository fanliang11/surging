using System;
using System.Collections.Generic;
using System.Linq;
#if NET6_0_OR_GREATER
using Microsoft.AspNetCore.Http.Metadata;
#endif
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Routing;
using Surging.Core.CPlatform.Runtime.Server;
using System.Reflection;

namespace Surging.Core.Swagger_V5.SwaggerGen
{
    public class SwaggerGeneratorOptions
    {
        public SwaggerGeneratorOptions()
        {
            SwaggerDocs = new Dictionary<string, OpenApiInfo>();
            DocInclusionPredicate = DefaultDocInclusionPredicate;
            OperationIdSelector = DefaultOperationIdSelector;
            EntryOperationIdSelector = DefaultEntryOperationIdSelector;
            DocInclusionPredicateV2 = DefaultDocInclusionPredicateV2;
            EntryTagsSelector = DefaultEntryTagsSelector;
            TagsSelector = DefaultTagsSelector;
            SortKeySelector = DefaultSortKeySelector;
            SchemaComparer = StringComparer.Ordinal;
            Servers = new List<OpenApiServer>();
            SecuritySchemes = new Dictionary<string, OpenApiSecurityScheme>();
            SecurityRequirements = new List<OpenApiSecurityRequirement>();
            ParameterFilters = new List<IParameterFilter>();
            RequestBodyFilters = new List<IRequestBodyFilter>();
            OperationFilters = new List<IOperationFilter>();
            DocumentFilters = new List<IDocumentFilter>();
        }

        public IDictionary<string, OpenApiInfo> SwaggerDocs { get; set; }

        public Func<string, ApiDescription, bool> DocInclusionPredicate { get; set; }

        public bool IgnoreObsoleteActions { get; set; }

        public Func<string, ServiceEntry, bool> DocInclusionPredicateV2 { get; set; }

        public Func<IEnumerable<ApiDescription>, ApiDescription> ConflictingActionsResolver { get; set; }

        public Func<ApiDescription, string> OperationIdSelector { get; set; }

        public Func<ServiceEntry, string> EntryOperationIdSelector { get; set; }

        public Func<ApiDescription, IList<string>> TagsSelector { get; set; }

        public Func<ServiceEntry, IList<string>> EntryTagsSelector { get; set; }

        public Func<ApiDescription, string> SortKeySelector { get; set; }

        public bool DescribeAllParametersInCamelCase { get; set; }

        public List<OpenApiServer> Servers { get; set; }
         
        public IDictionary<string, OpenApiSecurityScheme> SecuritySchemes { get; set; }

        public IList<OpenApiSecurityRequirement> SecurityRequirements { get; set; }

        public IComparer<string> SchemaComparer { get; set; }

        public IList<IParameterFilter> ParameterFilters { get; set; }

        public List<IRequestBodyFilter> RequestBodyFilters { get; set; }

        public List<IOperationFilter> OperationFilters { get; set; }

        public IList<IDocumentFilter> DocumentFilters { get; set; }

        private bool DefaultDocInclusionPredicate(string documentName, ApiDescription apiDescription)
        {
            return apiDescription.GroupName == null || apiDescription.GroupName == documentName;
        }

        private bool DefaultDocInclusionPredicateV2(string documentName, ServiceEntry apiDescription)
        {
            var assembly = apiDescription.Type.Assembly;

            var versions = assembly
                        .GetCustomAttributes(true)
                        .OfType<AssemblyVersionAttribute>();
            return versions != null;
        }

        private string DefaultOperationIdSelector(ApiDescription apiDescription)
        {
            var actionDescriptor = apiDescription.ActionDescriptor;

            // Resolve the operation ID from the route name and fallback to the
            // endpoint name if no route name is available. This allows us to
            // generate operation IDs for endpoints that are defined using
            // minimal APIs.
#if (!NETSTANDARD2_0)
            return
                actionDescriptor.AttributeRouteInfo?.Name
                ?? (actionDescriptor.EndpointMetadata?.LastOrDefault(m => m is IEndpointNameMetadata) as IEndpointNameMetadata)?.EndpointName;
#else
            return actionDescriptor.AttributeRouteInfo?.Name;
#endif
        }

        private string DefaultEntryOperationIdSelector(ServiceEntry entry)
        {
            return entry.RoutePath;
        }

        private IList<string> DefaultEntryTagsSelector(ServiceEntry entry)
        { 
            return new string[] { entry.Type.Name };
        }

        private IList<string> DefaultTagsSelector(ApiDescription apiDescription)
        {
#if (!NET6_0_OR_GREATER)
            return new[] { apiDescription.ActionDescriptor.RouteValues["controller"] };
#else
            var actionDescriptor = apiDescription.ActionDescriptor;
            var tagsMetadata = actionDescriptor.EndpointMetadata?.LastOrDefault(m => m is ITagsMetadata) as ITagsMetadata;
            if (tagsMetadata != null)
            {
                return new List<string>(tagsMetadata.Tags);
            }
            return new[] { apiDescription.ActionDescriptor.RouteValues["controller"] };
#endif
        }

        private string DefaultSortKeySelector(ApiDescription apiDescription)
        {
            return TagsSelector(apiDescription).First();
        }
    }
}
