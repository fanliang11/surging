using Surging.Core.CPlatform.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Filters.Implementation
{
    public abstract class BaseActionFilterAttribute : FilterAttribute, IActionFilter, IFilter
    {
        public abstract Task OnActionExecutingAsync(ServiceRouteContext actionExecutedContext, CancellationToken cancellationToken);
     
    }
}
