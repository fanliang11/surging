using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.XPath;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.Swagger;
using Surging.Core.SwaggerGen;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SwaggerGenOptionsExtensions
    {
        /// <summary>
        /// Define one or more documents to be created by the Swagger generator
        /// </summary>
        /// <param name="swaggerGenOptions"></param>
        /// <param name="name">A URI-friendly name that uniquely identifies the document</param>
        /// <param name="info">Global metadata to be included in the Swagger output</param>
        public static void SwaggerDoc(
            this SwaggerGenOptions swaggerGenOptions,
            string name,
            Info info)
        {
            swaggerGenOptions.SwaggerGeneratorOptions.SwaggerDocs.Add(name, info);
        }

        /// <summary>
        /// Provide a custom strategy for selecting actions.
        /// </summary>
        /// <param name="swaggerGenOptions"></param>
        /// <param name="predicate">
        /// A lambda that returns true/false based on document name and ApiDescription
        /// </param>
        public static void DocInclusionPredicate(
            this SwaggerGenOptions swaggerGenOptions,
            Func<string, ApiDescription, bool> predicate)
        {
            swaggerGenOptions.SwaggerGeneratorOptions.DocInclusionPredicate = predicate;
        }

        public static void DocInclusionPredicateV2(
      this SwaggerGenOptions swaggerGenOptions,
      Func<string, ServiceEntry, bool> predicate)
        {
            swaggerGenOptions.SwaggerGeneratorOptions.DocInclusionPredicateV2 = predicate;
        }

        public static void GenerateSwaggerDoc(
      this SwaggerGenOptions swaggerGenOptions, IEnumerable<ServiceEntry> entries)
        {

            var result = new Dictionary<string, Info>();
            var assemblies = entries.Select(p => p.Type.Assembly).Distinct();
            foreach (var assembly in assemblies)
            {
                var version = assembly
                    .GetCustomAttributes(true)
                    .OfType<AssemblyFileVersionAttribute>().FirstOrDefault();

                var title = assembly
                  .GetCustomAttributes(true)
                  .OfType<AssemblyTitleAttribute>().FirstOrDefault();

                var des = assembly
                  .GetCustomAttributes(true)
                  .OfType<AssemblyDescriptionAttribute>().FirstOrDefault();

                if (version == null || title == null)
                    continue;
                swaggerGenOptions.SwaggerDoc(title.Title, new Info()
                {
                    Title = title.Title,
                    Version = version.Version,
                    Description = des?.Description,

                });
            }
        }

        /// <summary>
        /// Ignore any actions that are decorated with the ObsoleteAttribute
        /// </summary>
        public static void IgnoreObsoleteActions(this SwaggerGenOptions swaggerGenOptions)
        {
            swaggerGenOptions.SwaggerGeneratorOptions.IgnoreObsoleteActions = true;
        }

        /// <summary>
        /// Merge actions that have conflicting HTTP methods and paths (must be unique for Swagger 2.0)
        /// </summary>
        /// <param name="swaggerGenOptions"></param>
        /// <param name="resolver"></param>
        public static void ResolveConflictingActions(
            this SwaggerGenOptions swaggerGenOptions,
            Func<IEnumerable<ApiDescription>, ApiDescription> resolver)
        {
            swaggerGenOptions.SwaggerGeneratorOptions.ConflictingActionsResolver = resolver;
        }

        /// <summary>
        /// Provide a custom strategy for assigning "operationId" to operations
        /// </summary>
        public static void CustomOperationIds(
            this SwaggerGenOptions swaggerGenOptions,
            Func<ApiDescription, string> operationIdSelector)
        {
            swaggerGenOptions.SwaggerGeneratorOptions.OperationIdSelector = operationIdSelector;
        }

        /// <summary>
        /// Provide a custom strategy for assigning a default "tag" to operations
        /// </summary>
        /// <param name="swaggerGenOptions"></param>
        /// <param name="tagSelector"></param>
        [Obsolete("Deprecated: Use the overload that accepts a Func that returns a list of tags")]
        public static void TagActionsBy(
            this SwaggerGenOptions swaggerGenOptions,
            Func<ApiDescription, string> tagSelector)
        {
            swaggerGenOptions.SwaggerGeneratorOptions.TagsSelector = (apiDesc) => new[] { tagSelector(apiDesc) };
        }

        /// <summary>
        /// Provide a custom strategy for assigning "tags" to actions
        /// </summary>
        /// <param name="swaggerGenOptions"></param>
        /// <param name="tagsSelector"></param>
        public static void TagActionsBy(
            this SwaggerGenOptions swaggerGenOptions,
            Func<ApiDescription, IList<string>> tagsSelector)
        {
            swaggerGenOptions.SwaggerGeneratorOptions.TagsSelector = tagsSelector;
        }

        /// <summary>
        /// Provide a custom strategy for sorting actions before they're transformed into the Swagger format
        /// </summary>
        /// <param name="swaggerGenOptions"></param>
        /// <param name="sortKeySelector"></param>
        public static void OrderActionsBy(
            this SwaggerGenOptions swaggerGenOptions,
            Func<ApiDescription, string> sortKeySelector)
        {
            swaggerGenOptions.SwaggerGeneratorOptions.SortKeySelector = sortKeySelector;
        }

        /// <summary>
        /// Describe all parameters, regardless of how they appear in code, in camelCase
        /// </summary>
        public static void DescribeAllParametersInCamelCase(this SwaggerGenOptions swaggerGenOptions)
        {
            swaggerGenOptions.SwaggerGeneratorOptions.DescribeAllParametersInCamelCase = true;
        }

        /// <summary>
        /// Add one or more "securityDefinitions", describing how your API is protected, to the generated Swagger
        /// </summary>
        /// <param name="swaggerGenOptions"></param>
        /// <param name="name">A unique name for the scheme, as per the Swagger spec.</param>
        /// <param name="securityScheme">
        /// A description of the scheme - can be an instance of BasicAuthScheme, ApiKeyScheme or OAuth2Scheme
        /// </param>
        public static void AddSecurityDefinition(
            this SwaggerGenOptions swaggerGenOptions,
            string name,
            SecurityScheme securityScheme)
        {
            swaggerGenOptions.SwaggerGeneratorOptions.SecurityDefinitions.Add(name, securityScheme);
        }

        /// <summary>
        /// Adds a global security requirement
        /// </summary>
        /// <param name="swaggerGenOptions"></param>
        /// <param name="requirement">
        /// A dictionary of required schemes (logical AND). Keys must correspond to schemes defined through AddSecurityDefinition
        /// If the scheme is of type "oauth2", then the value is a list of scopes, otherwise it MUST be an empty array
        /// </param>
        public static void AddSecurityRequirement(
            this SwaggerGenOptions swaggerGenOptions,
            IDictionary<string, IEnumerable<string>> requirement)
        {
            swaggerGenOptions.SwaggerGeneratorOptions.SecurityRequirements.Add(requirement);
        }

        /// <summary>
        /// Provide a custom mapping, for a given type, to the Swagger-flavored JSONSchema
        /// </summary>
        /// <param name="swaggerGenOptions"></param>
        /// <param name="type">System type</param>
        /// <param name="schemaFactory">A factory method that generates Schema's for the provided type</param>
        public static void MapType(
            this SwaggerGenOptions swaggerGenOptions,
            Type type,
            Func<Schema> schemaFactory)
        {
            swaggerGenOptions.SchemaRegistryOptions.CustomTypeMappings.Add(type, schemaFactory);
        }

        /// <summary>
        /// Provide a custom mapping, for a given type, to the Swagger-flavored JSONSchema
        /// </summary>
        /// <typeparam name="T">System type</typeparam>
        /// <param name="swaggerGenOptions"></param>
        /// <param name="schemaFactory">A factory method that generates Schema's for the provided type</param>
        public static void MapType<T>(
            this SwaggerGenOptions swaggerGenOptions,
            Func<Schema> schemaFactory)
        {
            swaggerGenOptions.MapType(typeof(T), schemaFactory);
        }

        /// <summary>
        /// Use the enum names, as opposed to their integer values, when describing enum types
        /// </summary>
        public static void DescribeAllEnumsAsStrings(this SwaggerGenOptions swaggerGenOptions)
        {
            swaggerGenOptions.SchemaRegistryOptions.DescribeAllEnumsAsStrings = true;
        }

        /// <summary>
        /// If applicable, describe all enum names, regardless of how they appear in code, in camelCase.
        /// </summary>
        public static void DescribeStringEnumsInCamelCase(this SwaggerGenOptions swaggerGenOptions)
        {
            swaggerGenOptions.SchemaRegistryOptions.DescribeStringEnumsInCamelCase = true;
        }

        /// <summary>
        /// Use referenced definitions for enum types within body parameter and response schemas
        /// </summary>
        public static void UseReferencedDefinitionsForEnums(this SwaggerGenOptions swaggerGenOptions)
        {
            swaggerGenOptions.SchemaRegistryOptions.UseReferencedDefinitionsForEnums = true;
        }

        /// <summary>
        /// Provide a custom strategy for generating the unique Id's that are used to reference object Schema's
        /// </summary>
        /// <param name="swaggerGenOptions"></param>
        /// <param name="schemaIdSelector">
        /// A lambda that returns a unique identifier for the provided system type
        /// </param>
        public static void CustomSchemaIds(
            this SwaggerGenOptions swaggerGenOptions,
            Func<Type, string> schemaIdSelector)
        {
            swaggerGenOptions.SchemaRegistryOptions.SchemaIdSelector = schemaIdSelector;
        }

        /// <summary>
        /// Ignore any properties that are decorated with the ObsoleteAttribute
        /// </summary>
        public static void IgnoreObsoleteProperties(this SwaggerGenOptions swaggerGenOptions)
        {
            swaggerGenOptions.SchemaRegistryOptions.IgnoreObsoleteProperties = true;
        }

        public static void IgnoreFullyQualified(this SwaggerGenOptions swaggerGenOptions)
        {
            swaggerGenOptions.SchemaRegistryOptions.IgnoreFullyQualified = true;
        }

        /// <summary>
        /// Extend the Swagger Generator with "filters" that can modify Schemas after they're initially generated
        /// </summary>
        /// <typeparam name="TFilter">A type that derives from ISchemaFilter</typeparam>
        /// <param name="swaggerGenOptions"></param>
        /// <param name="arguments">Optionally inject parameters through filter constructors</param>
        public static void SchemaFilter<TFilter>(
            this SwaggerGenOptions swaggerGenOptions,
            params object[] arguments)
            where TFilter : ISchemaFilter
        {
            swaggerGenOptions.SchemaFilterDescriptors.Add(new FilterDescriptor
            {
                Type = typeof(TFilter),
                Arguments = arguments
            });
        }

        /// <summary>
        /// Extend the Swagger Generator with "filters" that can modify Parameters after they're initially generated
        /// </summary>
        /// <typeparam name="TFilter">A type that derives from IParameterFilter</typeparam>
        /// <param name="swaggerGenOptions"></param>
        /// <param name="arguments">Optionally inject parameters through filter constructors</param>
        public static void ParameterFilter<TFilter>(
            this SwaggerGenOptions swaggerGenOptions,
            params object[] arguments)
            where TFilter : IParameterFilter
        {
            swaggerGenOptions.ParameterFilterDescriptors.Add(new FilterDescriptor
            {
                Type = typeof(TFilter),
                Arguments = arguments
            });
        }

        /// <summary>
        /// Extend the Swagger Generator with "filters" that can modify Operations after they're initially generated
        /// </summary>
        /// <typeparam name="TFilter">A type that derives from IOperationFilter</typeparam>
        /// <param name="swaggerGenOptions"></param>
        /// <param name="arguments">Optionally inject parameters through filter constructors</param>
        public static void OperationFilter<TFilter>(
            this SwaggerGenOptions swaggerGenOptions,
            params object[] arguments)
            where TFilter : IOperationFilter
        {
            swaggerGenOptions.OperationFilterDescriptors.Add(new FilterDescriptor
            {
                Type = typeof(TFilter),
                Arguments = arguments
            });
        }

        /// <summary>
        /// Extend the Swagger Generator with "filters" that can modify SwaggerDocuments after they're initially generated
        /// </summary>
        /// <typeparam name="TFilter">A type that derives from IDocumentFilter</typeparam>
        /// <param name="swaggerGenOptions"></param>
        /// <param name="arguments">Optionally inject parameters through filter constructors</param>
        public static void DocumentFilter<TFilter>(
            this SwaggerGenOptions swaggerGenOptions,
            params object[] arguments)
            where TFilter : IDocumentFilter
        {
            swaggerGenOptions.DocumentFilterDescriptors.Add(new FilterDescriptor
            {
                Type = typeof(TFilter),
                Arguments = arguments
            });
        }

        /// <summary>
        /// Inject human-friendly descriptions for Operations, Parameters and Schemas based on XML Comment files
        /// </summary>
        /// <param name="swaggerGenOptions"></param>
        /// <param name="xmlDocFactory">A factory method that returns XML Comments as an XPathDocument</param>
        /// <param name="includeControllerXmlComments">
        /// Flag to indicate if controller XML comments (i.e. summary) should be used to assign Tag descriptions.
        /// Don't set this flag if you're customizing the default tag for operations via TagActionsBy.
        /// </param>
        public static void IncludeXmlComments(
            this SwaggerGenOptions swaggerGenOptions,
            Func<XPathDocument> xmlDocFactory,
            bool includeControllerXmlComments = false)
        {
            var xmlDoc = xmlDocFactory();
            swaggerGenOptions.OperationFilter<XmlCommentsOperationFilter>(xmlDoc);
            swaggerGenOptions.SchemaFilter<XmlCommentsSchemaFilter>(xmlDoc);

            if (includeControllerXmlComments)
                swaggerGenOptions.DocumentFilter<XmlCommentsDocumentFilter>(xmlDoc);
        }

        /// <summary>
        /// Inject human-friendly descriptions for Operations, Parameters and Schemas based on XML Comment files
        /// </summary>
        /// <param name="swaggerGenOptions"></param>
        /// <param name="filePath">An abolsute path to the file that contains XML Comments</param>
        /// <param name="includeControllerXmlComments">
        /// Flag to indicate if controller XML comments (i.e. summary) should be used to assign Tag descriptions.
        /// Don't set this flag if you're customizing the default tag for operations via TagActionsBy.
        /// </param>
        public static void IncludeXmlComments(
            this SwaggerGenOptions swaggerGenOptions,
            string filePath,
            bool includeControllerXmlComments = false)
        {
            swaggerGenOptions.IncludeXmlComments(() => new XPathDocument(filePath), includeControllerXmlComments);
        }
    }
}