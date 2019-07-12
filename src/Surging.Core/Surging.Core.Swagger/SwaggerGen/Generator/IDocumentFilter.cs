using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Surging.Core.Swagger;
using System;
using System.Collections.Generic;

namespace Surging.Core.SwaggerGen
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IDocumentFilter" />
    /// </summary>
    public interface IDocumentFilter
    {
        #region 方法

        /// <summary>
        /// The Apply
        /// </summary>
        /// <param name="swaggerDoc">The swaggerDoc<see cref="SwaggerDocument"/></param>
        /// <param name="context">The context<see cref="DocumentFilterContext"/></param>
        void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context);

        #endregion 方法
    }

    #endregion 接口

    /// <summary>
    /// Defines the <see cref="DocumentFilterContext" />
    /// </summary>
    public class DocumentFilterContext
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentFilterContext"/> class.
        /// </summary>
        /// <param name="apiDescriptionsGroups">The apiDescriptionsGroups<see cref="ApiDescriptionGroupCollection"/></param>
        /// <param name="apiDescriptions">The apiDescriptions<see cref="IEnumerable{ApiDescription}"/></param>
        /// <param name="schemaRegistry">The schemaRegistry<see cref="ISchemaRegistry"/></param>
        public DocumentFilterContext(
            ApiDescriptionGroupCollection apiDescriptionsGroups,
            IEnumerable<ApiDescription> apiDescriptions,
            ISchemaRegistry schemaRegistry)
        {
            ApiDescriptionsGroups = apiDescriptionsGroups;
            ApiDescriptions = apiDescriptions;
            SchemaRegistry = schemaRegistry;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the ApiDescriptions
        /// </summary>
        public IEnumerable<ApiDescription> ApiDescriptions { get; private set; }

        /// <summary>
        /// Gets the ApiDescriptionsGroups
        /// </summary>
        [Obsolete("Deprecated: Use ApiDescriptions")]
        public ApiDescriptionGroupCollection ApiDescriptionsGroups { get; private set; }

        /// <summary>
        /// Gets the SchemaRegistry
        /// </summary>
        public ISchemaRegistry SchemaRegistry { get; private set; }

        #endregion 属性
    }
}