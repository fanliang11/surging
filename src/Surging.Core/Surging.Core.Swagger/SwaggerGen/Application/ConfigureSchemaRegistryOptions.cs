using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Surging.Core.Swagger;
using System;
using System.Collections.Generic;

namespace Surging.Core.SwaggerGen
{
    /// <summary>
    /// Defines the <see cref="ConfigureSchemaRegistryOptions" />
    /// </summary>
    internal class ConfigureSchemaRegistryOptions : IConfigureOptions<SchemaRegistryOptions>
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
        /// Initializes a new instance of the <see cref="ConfigureSchemaRegistryOptions"/> class.
        /// </summary>
        /// <param name="serviceProvider">The serviceProvider<see cref="IServiceProvider"/></param>
        /// <param name="swaggerGenOptionsAccessor">The swaggerGenOptionsAccessor<see cref="IOptions{SwaggerGenOptions}"/></param>
        public ConfigureSchemaRegistryOptions(
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
        /// <param name="options">The options<see cref="SchemaRegistryOptions"/></param>
        public void Configure(SchemaRegistryOptions options)
        {
            DeepCopy(_swaggerGenOptions.SchemaRegistryOptions, options);

            // Create and add any filters that were specified through the FilterDescriptor lists
            _swaggerGenOptions.SchemaFilterDescriptors.ForEach(
                filterDescriptor => options.SchemaFilters.Add(CreateFilter<ISchemaFilter>(filterDescriptor)));
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

        /// <summary>
        /// The DeepCopy
        /// </summary>
        /// <param name="source">The source<see cref="SchemaRegistryOptions"/></param>
        /// <param name="target">The target<see cref="SchemaRegistryOptions"/></param>
        private void DeepCopy(SchemaRegistryOptions source, SchemaRegistryOptions target)
        {
            target.CustomTypeMappings = new Dictionary<Type, Func<Schema>>(source.CustomTypeMappings);
            target.DescribeAllEnumsAsStrings = source.DescribeAllEnumsAsStrings;
            target.IgnoreFullyQualified = source.IgnoreFullyQualified;
            target.DescribeStringEnumsInCamelCase = source.DescribeStringEnumsInCamelCase;
            target.UseReferencedDefinitionsForEnums = source.UseReferencedDefinitionsForEnums;
            target.SchemaIdSelector = source.SchemaIdSelector;
            target.IgnoreObsoleteProperties = source.IgnoreObsoleteProperties;
            target.SchemaFilters = new List<ISchemaFilter>(source.SchemaFilters);
        }

        #endregion 方法
    }
}