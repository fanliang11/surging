using Autofac;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.Swagger_V5.Swagger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Swagger_V5.SwaggerGen
{
    internal class SwaggerGenerator2 : ISwaggerProvider
    {
        private readonly IApiDescriptionGroupCollectionProvider _apiDescriptionsProvider;
        private readonly ISchemaGenerator _schemaGenerator;
        private readonly SwaggerGeneratorOptions _options;
        private readonly IServiceEntryProvider _serviceEntryProvider;

        public SwaggerGenerator2(
            SwaggerGeneratorOptions options,
            IApiDescriptionGroupCollectionProvider apiDescriptionsProvider,
            ISchemaGenerator schemaGenerator)
        {
            _options = options ?? new SwaggerGeneratorOptions();
            _apiDescriptionsProvider = apiDescriptionsProvider;
            _schemaGenerator = schemaGenerator;
            _serviceEntryProvider = ServiceLocator.Current.Resolve<IServiceEntryProvider>();
        }

        public OpenApiDocument GetSwagger(string documentName, string host = null, string basePath = null)
        {
            if (!_options.SwaggerDocs.TryGetValue(documentName, out OpenApiInfo info))
                throw new UnknownSwaggerDocument(documentName, _options.SwaggerDocs.Select(d => d.Key));

            var applicableApiDescriptions = _apiDescriptionsProvider.ApiDescriptionGroups.Items
                .SelectMany(group => group.Items)
                .Where(apiDesc => !(_options.IgnoreObsoleteActions && apiDesc.CustomAttributes().OfType<ObsoleteAttribute>().Any()))
                .Where(apiDesc => _options.DocInclusionPredicate(documentName, apiDesc));

            var schemaRepository = new SchemaRepository(documentName);
            var entries = _serviceEntryProvider.GetALLEntries();
            var mapRoutePaths = AppConfig.SwaggerConfig.Options?.MapRoutePaths;
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


            var swaggerDoc = new OpenApiDocument
            {
                Info = info,
                Servers = GenerateServers(host, basePath),
                Paths = new OpenApiPaths()
                {
                     

                },
                Components = new OpenApiComponents
                {
                    Schemas = schemaRepository.Schemas,
                    SecuritySchemes = new Dictionary<string, OpenApiSecurityScheme>(_options.SecuritySchemes)
                },
                SecurityRequirements = new List<OpenApiSecurityRequirement>(_options.SecurityRequirements)
            };

            var filterContext = new DocumentFilterContext(applicableApiDescriptions, _schemaGenerator, schemaRepository);
            foreach (var filter in _options.DocumentFilters)
            {
                filter.Apply(swaggerDoc, filterContext);
            }

            swaggerDoc.Components.Schemas = new SortedDictionary<string, OpenApiSchema>(swaggerDoc.Components.Schemas, _options.SchemaComparer);
            return swaggerDoc;
        }

        private IList<OpenApiServer> GenerateServers(string host, string basePath)
        {
            if (_options.Servers.Any())
            {
                return new List<OpenApiServer>(_options.Servers);
            }

            return (host == null && basePath == null)
                ? new List<OpenApiServer>()
                : new List<OpenApiServer> { new OpenApiServer { Url = $"{host}{basePath}" } };
        }

    }
}
