using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.Consul.WatcherProvider
{
    public abstract class WatcherBase : Watcher
    {

        protected WatcherBase()
        {
         
        }
        
        public override async Task Process()
        {
                await ProcessImpl();
        }

        protected abstract Task ProcessImpl();
    }
}
