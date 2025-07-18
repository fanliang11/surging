using Surging.Core.CPlatform.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Filters.Implementation
{
    public class ActionFilterAttribute : BaseActionFilterAttribute
    {
        public  override  Task OnActionExecutingAsync(ServiceRouteContext actionExecutedContext, CancellationToken cancellationToken)
        { 
            return Task.CompletedTask;
        }
    }
}
