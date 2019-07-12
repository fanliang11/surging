using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.Swagger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Surging.Core.SwaggerGen
{
    /// <summary>
    /// Defines the <see cref="SwaggerGeneratorOptions" />
    /// </summary>
    public class SwaggerGeneratorOptions
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="SwaggerGeneratorOptions"/> class.
        /// </summary>
        public SwaggerGeneratorOptions()
        {
            SwaggerDocs = new Dictionary<string, Info>();
            DocInclusionPredicate = DefaultDocInclusionPredicate;
            DocInclusionPredicateV2 = DefaultDocInclusionPredicateV2;
            OperationIdSelector = DefaultOperationIdSelector;
            TagsSelector = DefaultTagsSelector;
            SortKeySelector = DefaultSortKeySelector;
            SecurityDefinitions = new Dictionary<string, SecurityScheme>();
            SecurityRequirements = new List<IDictionary<string, IEnumerable<string>>>();
            ParameterFilters = new List<IParameterFilter>();
            OperationFilters = new List<IOperationFilter>();
            DocumentFilters = new List<IDocumentFilter>();
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the ConflictingActionsResolver
        /// </summary>
        public Func<IEnumerable<ApiDescription>, ApiDescription> ConflictingActionsResolver { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether DescribeAllParametersInCamelCase
        /// </summary>
        public bool DescribeAllParametersInCamelCase { get; set; }

        /// <summary>
        /// Gets or sets the DocInclusionPredicate
        /// </summary>
        public Func<string, ApiDescription, bool> DocInclusionPredicate { get; set; }

        /// <summary>
        /// Gets or sets the DocInclusionPredicateV2
        /// </summary>
        public Func<string, ServiceEntry, bool> DocInclusionPredicateV2 { get; set; }

        /// <summary>
        /// Gets or sets the DocumentFilters
        /// </summary>
        public IList<IDocumentFilter> DocumentFilters { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IgnoreObsoleteActions
        /// </summary>
        public bool IgnoreObsoleteActions { get; set; }

        /// <summary>
        /// Gets or sets the OperationFilters
        /// </summary>
        public List<IOperationFilter> OperationFilters { get; set; }

        /// <summary>
        /// Gets or sets the OperationIdSelector
        /// </summary>
        public Func<ApiDescription, string> OperationIdSelector { get; set; }

        /// <summary>
        /// Gets or sets the ParameterFilters
        /// </summary>
        public IList<IParameterFilter> ParameterFilters { get; set; }

        /// <summary>
        /// Gets or sets the SecurityDefinitions
        /// </summary>
        public IDictionary<string, SecurityScheme> SecurityDefinitions { get; set; }

        /// <summary>
        /// Gets or sets the SecurityRequirements
        /// </summary>
        public IList<IDictionary<string, IEnumerable<string>>> SecurityRequirements { get; set; }

        /// <summary>
        /// Gets or sets the SortKeySelector
        /// </summary>
        public Func<ApiDescription, string> SortKeySelector { get; set; }

        /// <summary>
        /// Gets or sets the SwaggerDocs
        /// </summary>
        public IDictionary<string, Info> SwaggerDocs { get; set; }

        /// <summary>
        /// Gets or sets the TagsSelector
        /// </summary>
        public Func<ApiDescription, IList<string>> TagsSelector { get; set; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The DefaultDocInclusionPredicate
        /// </summary>
        /// <param name="documentName">The documentName<see cref="string"/></param>
        /// <param name="apiDescription">The apiDescription<see cref="ApiDescription"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private bool DefaultDocInclusionPredicate(string documentName, ApiDescription apiDescription)
        {
            return apiDescription.GroupName == null || apiDescription.GroupName == documentName;
        }

        /// <summary>
        /// The DefaultDocInclusionPredicateV2
        /// </summary>
        /// <param name="documentName">The documentName<see cref="string"/></param>
        /// <param name="apiDescription">The apiDescription<see cref="ServiceEntry"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private bool DefaultDocInclusionPredicateV2(string documentName, ServiceEntry apiDescription)
        {
            var assembly = apiDescription.Type.Assembly;

            var versions = assembly
                        .GetCustomAttributes(true)
                        .OfType<AssemblyVersionAttribute>();
            return versions != null;
        }

        /// <summary>
        /// The DefaultOperationIdSelector
        /// </summary>
        /// <param name="apiDescription">The apiDescription<see cref="ApiDescription"/></param>
        /// <returns>The <see cref="string"/></returns>
        private string DefaultOperationIdSelector(ApiDescription apiDescription)
        {
            var routeName = apiDescription.ActionDescriptor.AttributeRouteInfo?.Name;
            if (routeName != null) return routeName;

            if (apiDescription.TryGetMethodInfo(out MethodInfo methodInfo)) return methodInfo.Name;

            return null;
        }

        /// <summary>
        /// The DefaultSortKeySelector
        /// </summary>
        /// <param name="apiDescription">The apiDescription<see cref="ApiDescription"/></param>
        /// <returns>The <see cref="string"/></returns>
        private string DefaultSortKeySelector(ApiDescription apiDescription)
        {
            return TagsSelector(apiDescription).First();
        }

        /// <summary>
        /// The DefaultTagsSelector
        /// </summary>
        /// <param name="apiDescription">The apiDescription<see cref="ApiDescription"/></param>
        /// <returns>The <see cref="IList{string}"/></returns>
        private IList<string> DefaultTagsSelector(ApiDescription apiDescription)
        {
            return new[] { apiDescription.ActionDescriptor.RouteValues["controller"] };
        }

        #endregion 方法
    }
}