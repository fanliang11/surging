using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Surging.Core.ApiGateWay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surging.ApiGateway
{
    /// <summary>
    /// Defines the <see cref="CustomExceptionFilterAttribute" />
    /// </summary>
    public class CustomExceptionFilterAttribute : ExceptionFilterAttribute
    {
        #region 字段

        /// <summary>
        /// Defines the _hostingEnvironment
        /// </summary>
        private readonly IHostingEnvironment _hostingEnvironment;

        /// <summary>
        /// Defines the _modelMetadataProvider
        /// </summary>
        private readonly IModelMetadataProvider _modelMetadataProvider;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomExceptionFilterAttribute"/> class.
        /// </summary>
        /// <param name="hostingEnvironment">The hostingEnvironment<see cref="IHostingEnvironment"/></param>
        /// <param name="modelMetadataProvider">The modelMetadataProvider<see cref="IModelMetadataProvider"/></param>
        public CustomExceptionFilterAttribute(
            IHostingEnvironment hostingEnvironment,
            IModelMetadataProvider modelMetadataProvider)
        {
            _hostingEnvironment = hostingEnvironment;
            _modelMetadataProvider = modelMetadataProvider;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The OnException
        /// </summary>
        /// <param name="context">The context<see cref="ExceptionContext"/></param>
        public override void OnException(ExceptionContext context)
        {
            if (!_hostingEnvironment.IsDevelopment())
            {
                return;
            }
            var result = ServiceResult<object>.Create(false, errorMessage: context.Exception.Message);
            result.StatusCode = 400;
            context.Result = new JsonResult(result);
        }

        #endregion 方法
    }
}