using Surging.Core.CPlatform.Filters.Implementation;
using Surging.Core.CPlatform.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Filters
{
    public interface IActionFilter : IFilter
    {
        Task OnActionExecutingAsync(ServiceRouteContext actionExecutedContext, CancellationToken cancellationToken);
    }
}
