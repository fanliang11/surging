using Surging.Core.ProxyGenerator;
using Surging.Core.Thrift.Attributes;
using Surging.IModuleServices.Common;
using System.Threading;
using System.Threading.Tasks;
using static ThriftCore.Calculator;

namespace Surging.Modules.Common.Domain
{
    [BindProcessor(typeof(AsyncProcessor))]
    public class AsyncService : ProxyServiceBase, IAsyncService
    {
        public Task<int> AddAsync(int num1, int num2, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(num1 + num2);
        }

        public Task<string> SayHelloAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult("hello world");
        }
    }
}
