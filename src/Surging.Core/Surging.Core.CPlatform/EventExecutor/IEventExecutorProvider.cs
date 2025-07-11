using DotNetty.Common.Concurrency;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.EventExecutor
{
    public interface IEventExecutorProvider
    {
        public IEventLoopGroup GetBossEventExecutor();

        public IEventLoopGroup GetWorkEventExecutor();

    }
}
