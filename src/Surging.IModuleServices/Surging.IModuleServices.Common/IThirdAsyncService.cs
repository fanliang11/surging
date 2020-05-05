using Surging.Core.CPlatform.Ioc;
using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using Surging.Core.CPlatform.Support.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.IModuleServices.Common
{
    [ServiceBundle("api/{Service}/{Method}")]
    public interface IThirdAsyncService : ThriftCore.ThirdCalculator.IAsync, IServiceKey
    {
        Task<int> @AddAsync(int num1, int num2, CancellationToken cancellationToken = default(CancellationToken));

        Task<string> SayHelloAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}

