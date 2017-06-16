using org.apache.zookeeper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Zookeeper.WatcherProvider
{
   public abstract class WatcherBase : Watcher
    {
        protected string Path { get; }

        protected WatcherBase(string path)
        {
            Path = path;
        }

        public override async Task process(WatchedEvent watchedEvent)
        {
            if (watchedEvent.getState() != Event.KeeperState.SyncConnected || watchedEvent.getPath() != Path)
                return;
            await ProcessImpl(watchedEvent);
        }

        protected abstract Task ProcessImpl(WatchedEvent watchedEvent);
    }
}
