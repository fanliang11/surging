using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Surging.Core.Swagger;
using System.Reflection;

namespace Surging.Core.SwaggerGen
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IParameterFilter" />
    /// </summary>
    public interface IParameterFilter
    {
        #region 方法

        /// <summary>
        /// The Apply
        /// </summary>
        /// <param name="parameter">The parameter<see cref="IParameter"/></param>
        /// <param name="context">The context<see cref="ParameterFilterContext"/></param>
        void Apply(IParameter parameter, ParameterFilterContext context);

        #endregion 方法
    }

    #endregion 接口

    /// <summary>
    /// Defines the <see cref="ParameterFilterContext" />
    /// </summary>
    public class ParameterFilterContext
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterFilterContext"/> class.
        /// </summary>
        /// <param name="apiParameterDescription">The apiParameterDescription<see cref="ApiParameterDescription"/></param>
        /// <param name="schemaRegistry">The schemaRegistry<see cref="ISchemaRegistry"/></param>
        /// <param name="parameterInfo">The parameterInfo<see cref="ParameterInfo"/></param>
        /// <param name="propertyInfo">The propertyInfo<see cref="PropertyInfo"/></param>
        public ParameterFilterContext(
            ApiParameterDescription apiParameterDescription,
            ISchemaRegistry schemaRegistry,
            ParameterInfo parameterInfo,
            PropertyInfo propertyInfo)
        {
            ApiParameterDescription = apiParameterDescription;
            SchemaRegistry = schemaRegistry;
            ParameterInfo = parameterInfo;
            PropertyInfo = propertyInfo;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the ApiParameterDescription
        /// </summary>
        public ApiParameterDescription ApiParameterDescription { get; }

        /// <summary>
        /// Gets the ParameterInfo
        /// </summary>
        public ParameterInfo ParameterInfo { get; }

        /// <summary>
        /// Gets the PropertyInfo
        /// </summary>
        public PropertyInfo PropertyInfo { get; }

        /// <summary>
        /// Gets the SchemaRegistry
        /// </summary>
        public ISchemaRegistry SchemaRegistry { get; }

        #endregion 属性
    }
}