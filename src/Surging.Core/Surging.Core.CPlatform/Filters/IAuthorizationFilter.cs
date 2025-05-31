using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Runtime.Client.Address.Resolvers.Implementation.Selectors;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Filters
{
    public interface IAuthorizationFilter: IFilter
    {
        Task ExecuteAuthorizationFilterAsync(ServiceRouteContext serviceRouteContext,CancellationToken cancellationToken);
    }
}
