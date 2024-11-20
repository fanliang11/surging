using Surging.Core.ProxyGenerator;
using Surging.Core.Thrift.Attributes;
using Surging.IModuleServices.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ThriftCore.ThirdCalculator;

namespace Surging.Modules.Common.Domain
{
    [BindProcessor(typeof(AsyncProcessor))]
    public class ThirdAsyncService : ProxyServiceBase, IThirdAsyncService
    {
        public Task<int> AddAsync(int num1, int num2, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(num1 + num2);
        }

        public Task<string> SayHelloAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult("hello world,third");
        }
    }
}
