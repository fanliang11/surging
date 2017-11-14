using Consul;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Consul.WatcherProvider
{
    public abstract class Watcher
    {
        protected Watcher()
        {
        }
        
        public abstract Task Process();

        public static class Event
        {
            public enum KeeperState
            {
                Disconnected = 0,
                SyncConnected = 3,
            }
        }
    }
}
