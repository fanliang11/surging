using DotNetty.Buffers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Tools.Cli.Internal.Implementation
{
    public class GCNettyModuleProvider : IGCModuleProvider
    {
        public void Collect()
        {
            var alloc = PooledByteBufferAllocator.Default;
            var buffer = alloc.Buffer(); 
            buffer.Release();
        }
    }
}
