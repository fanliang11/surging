using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.Swagger;
using System.Reflection;

namespace Surging.Core.SwaggerGen
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IOperationFilter" />
    /// </summary>
    public interface IOperationFilter
    {
        #region 方法

        /// <summary>
        /// The Apply
        /// </summary>
        /// <param name="operation">The operation<see cref="Operation"/></param>
        /// <param name="context">The context<see cref="OperationFilterContext"/></param>
        void Apply(Operation operation, OperationFilterContext context);

        #endregion 方法
    }

    #endregion 接口

    /// <summary>
    /// Defines the <see cref="OperationFilterContext" />
    /// </summary>
    public class OperationFilterContext
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationFilterContext"/> class.
        /// </summary>
        /// <param name="apiDescription">The apiDescription<see cref="ApiDescription"/></param>
        /// <param name="schemaRegistry">The schemaRegistry<see cref="ISchemaRegistry"/></param>
        /// <param name="methodInfo">The methodInfo<see cref="MethodInfo"/></param>
        public OperationFilterContext(
            ApiDescription apiDescription,
            ISchemaRegistry schemaRegistry,
            MethodInfo methodInfo) : this(apiDescription, schemaRegistry, methodInfo, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationFilterContext"/> class.
        /// </summary>
        /// <param name="apiDescription">The apiDescription<see cref="ApiDescription"/></param>
        /// <param name="schemaRegistry">The schemaRegistry<see cref="ISchemaRegistry"/></param>
        /// <param name="methodInfo">The methodInfo<see cref="MethodInfo"/></param>
        /// <param name="serviceEntry">The serviceEntry<see cref="ServiceEntry"/></param>
        public OperationFilterContext(
       ApiDescription apiDescription,
       ISchemaRegistry schemaRegistry,
       MethodInfo methodInfo, ServiceEntry serviceEntry)
        {
            ApiDescription = apiDescription;
            SchemaRegistry = schemaRegistry;
            MethodInfo = methodInfo;
            ServiceEntry = serviceEntry;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the ApiDescription
        /// </summary>
        public ApiDescription ApiDescription { get; private set; }

        /// <summary>
        /// Gets the MethodInfo
        /// </summary>
        public MethodInfo MethodInfo { get; }

        /// <summary>
        /// Gets the SchemaRegistry
        /// </summary>
        public ISchemaRegistry SchemaRegistry { get; private set; }

        /// <summary>
        /// Gets or sets the ServiceEntry
        /// </summary>
        public ServiceEntry ServiceEntry { get; set; }

        #endregion 属性
    }
}