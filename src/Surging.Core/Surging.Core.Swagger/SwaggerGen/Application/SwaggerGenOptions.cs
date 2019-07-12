using System;
using System.Collections.Generic;

namespace Surging.Core.SwaggerGen
{
    /// <summary>
    /// Defines the <see cref="FilterDescriptor" />
    /// </summary>
    public class FilterDescriptor
    {
        #region 属性

        /// <summary>
        /// Gets or sets the Arguments
        /// </summary>
        public object[] Arguments { get; set; }

        /// <summary>
        /// Gets or sets the Type
        /// </summary>
        public Type Type { get; set; }

        #endregion 属性
    }

    /// <summary>
    /// Defines the <see cref="SwaggerGenOptions" />
    /// </summary>
    public class SwaggerGenOptions
    {
        #region 属性

        /// <summary>
        /// Gets or sets the DocumentFilterDescriptors
        /// </summary>
        public List<FilterDescriptor> DocumentFilterDescriptors { get; set; } = new List<FilterDescriptor>();

        /// <summary>
        /// Gets or sets the OperationFilterDescriptors
        /// </summary>
        public List<FilterDescriptor> OperationFilterDescriptors { get; set; } = new List<FilterDescriptor>();

        // NOTE: Filter instances can be added directly to the options exposed above OR they can be specified in
        // the following lists. In the latter case, they will be instantiated and added when options are injected
        // into their target services. This "deferred instantiation" allows the filters to be created from the
        // DI container, thus supporting contructor injection of services within filters.
        /// <summary>
        /// Gets or sets the ParameterFilterDescriptors
        /// </summary>
        public List<FilterDescriptor> ParameterFilterDescriptors { get; set; } = new List<FilterDescriptor>();

        /// <summary>
        /// Gets or sets the SchemaFilterDescriptors
        /// </summary>
        public List<FilterDescriptor> SchemaFilterDescriptors { get; set; } = new List<FilterDescriptor>();

        /// <summary>
        /// Gets or sets the SchemaRegistryOptions
        /// </summary>
        public SchemaRegistryOptions SchemaRegistryOptions { get; set; } = new SchemaRegistryOptions();

        /// <summary>
        /// Gets or sets the SwaggerGeneratorOptions
        /// </summary>
        public SwaggerGeneratorOptions SwaggerGeneratorOptions { get; set; } = new SwaggerGeneratorOptions();

        #endregion 属性
    }
}