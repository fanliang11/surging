using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Surging.Core.Swagger;
using System;
using System.Collections.Generic;

namespace Surging.Core.SwaggerGen
{
    /// <summary>
    /// Defines the <see cref="ConfigureSwaggerGeneratorOptions" />
    /// </summary>
    internal class ConfigureSwaggerGeneratorOptions : IConfigureOptions<SwaggerGeneratorOptions>
    {
        #region 字段

        /// <summary>
        /// Defines the _serviceProvider
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Defines the _swaggerGenOptions
        /// </summary>
        private readonly SwaggerGenOptions _swaggerGenOptions;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureSwaggerGeneratorOptions"/> class.
        /// </summary>
        /// <param name="serviceProvider">The serviceProvider<see cref="IServiceProvider"/></param>
        /// <param name="swaggerGenOptionsAccessor">The swaggerGenOptionsAccessor<see cref="IOptions{SwaggerGenOptions}"/></param>
        public ConfigureSwaggerGeneratorOptions(
            IServiceProvider serviceProvider,
            IOptions<SwaggerGenOptions> swaggerGenOptionsAccessor)
        {
            _serviceProvider = serviceProvider;
            _swaggerGenOptions = swaggerGenOptionsAccessor.Value;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The Configure
        /// </summary>
        /// <param name="options">The options<see cref="SwaggerGeneratorOptions"/></param>
        public void Configure(SwaggerGeneratorOptions options)
        {
            DeepCopy(_swaggerGenOptions.SwaggerGeneratorOptions, options);

            // Create and add any filters that were specified through the FilterDescriptor lists ...

            _swaggerGenOptions.ParameterFilterDescriptors.ForEach(
                filterDescriptor => options.ParameterFilters.Add(CreateFilter<IParameterFilter>(filterDescriptor)));

            _swaggerGenOptions.OperationFilterDescriptors.ForEach(
                filterDescriptor => options.OperationFilters.Add(CreateFilter<IOperationFilter>(filterDescriptor)));

            _swaggerGenOptions.DocumentFilterDescriptors.ForEach(
                filterDescriptor => options.DocumentFilters.Add(CreateFilter<IDocumentFilter>(filterDescriptor)));
        }

        /// <summary>
        /// The DeepCopy
        /// </summary>
        /// <param name="source">The source<see cref="SwaggerGeneratorOptions"/></param>
        /// <param name="target">The target<see cref="SwaggerGeneratorOptions"/></param>
        public void DeepCopy(SwaggerGeneratorOptions source, SwaggerGeneratorOptions target)
        {
            target.SwaggerDocs = new Dictionary<string, Info>(source.SwaggerDocs);
            target.DocInclusionPredicate = source.DocInclusionPredicate;
            target.IgnoreObsoleteActions = source.IgnoreObsoleteActions;
            target.DocInclusionPredicateV2 = source.DocInclusionPredicateV2;
            target.ConflictingActionsResolver = source.ConflictingActionsResolver;
            target.OperationIdSelector = source.OperationIdSelector;
            target.TagsSelector = source.TagsSelector;
            target.SortKeySelector = source.SortKeySelector;
            target.DescribeAllParametersInCamelCase = source.DescribeAllParametersInCamelCase;
            target.SecurityDefinitions = new Dictionary<string, SecurityScheme>(source.SecurityDefinitions);
            target.SecurityRequirements = new List<IDictionary<string, IEnumerable<string>>>(source.SecurityRequirements);
            target.ParameterFilters = new List<IParameterFilter>(source.ParameterFilters);
            target.OperationFilters = new List<IOperationFilter>(source.OperationFilters);
            target.DocumentFilters = new List<IDocumentFilter>(source.DocumentFilters);
        }

        /// <summary>
        /// The CreateFilter
        /// </summary>
        /// <typeparam name="TFilter"></typeparam>
        /// <param name="filterDescriptor">The filterDescriptor<see cref="FilterDescriptor"/></param>
        /// <returns>The <see cref="TFilter"/></returns>
        private TFilter CreateFilter<TFilter>(FilterDescriptor filterDescriptor)
        {
            return (TFilter)ActivatorUtilities
                .CreateInstance(_serviceProvider, filterDescriptor.Type, filterDescriptor.Arguments);
        }

        #endregion 方法
    }
}