using Surging.Core.CPlatform.Exceptions;
using Surging.Core.CPlatform.Filters.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surging.ApiGateway
{
    /// <summary>
    /// Defines the <see cref="ServiceExceptionFilter" />
    /// </summary>
    public class ServiceExceptionFilter : ExceptionFilterAttribute
    {
        #region 方法

        /// <summary>
        /// The OnException
        /// </summary>
        /// <param name="context">The context<see cref="RpcActionExecutedContext"/></param>
        public override void OnException(RpcActionExecutedContext context)
        {
            if (context.Exception is CPlatformCommunicationException)
                throw new Exception(context.Exception.Message, context.Exception);
        }

        #endregion 方法
    }
}