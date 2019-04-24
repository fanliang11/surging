using Surging.Core.CPlatform.Filters.Implementation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Filters
{
   public interface IExceptionFilter: IFilter
    {
        Task ExecuteExceptionFilterAsync(RpcActionExecutedContext actionExecutedContext, CancellationToken cancellationToken);
    }
}
