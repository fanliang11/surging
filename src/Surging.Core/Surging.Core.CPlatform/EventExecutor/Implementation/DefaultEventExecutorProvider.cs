using DotNetty.Common.Concurrency;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Libuv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.EventExecutor.Implementation
{
    public class DefaultEventExecutorProvider : IEventExecutorProvider
    {
        private readonly IEventLoopGroup _workEventExecutor;
          
        private readonly IEventLoopGroup _bossEventExecutor;

        public DefaultEventExecutorProvider()
        {
            _bossEventExecutor = new MultithreadEventLoopGroup(AppConfig.ServerOptions.EventLoopCount);
            _workEventExecutor = new MultithreadEventLoopGroup(AppConfig.ServerOptions.EventLoopCount);
        }

        public IEventLoopGroup GetBossEventExecutor()
        {
            return _bossEventExecutor;
        }
         
        public IEventLoopGroup GetWorkEventExecutor()
        {
            return _workEventExecutor;
        }
    }
}
