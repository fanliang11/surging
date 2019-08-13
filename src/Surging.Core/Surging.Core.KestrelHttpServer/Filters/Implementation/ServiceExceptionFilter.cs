using Surging.Core.CPlatform.Exceptions;
using Surging.Core.CPlatform.Filters.Implementation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.KestrelHttpServer.Filters.Implementation
{
    public class ServiceExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(RpcActionExecutedContext context)
        {
            if (context.Exception is CPlatformCommunicationException)
                throw new Exception(context.Exception.Message, context.Exception);
        }
    }
}
