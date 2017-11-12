using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Surging.Core.Consul.WatcherProvider.Implementation
{
   public  abstract class WatchRegistration
    {
        private readonly Watcher watcher;
        private readonly string clientPath;

        protected WatchRegistration(Watcher watcher, string clientPath)
        {
            this.watcher = watcher;
            this.clientPath = clientPath;
        }

        protected abstract Dictionary<string, HashSet<Watcher>> GetWatches();

        public void Register()
        {
            var watches = GetWatches();
            lock (watches)
            {
                HashSet<Watcher> watchers;
                watches.TryGetValue(clientPath, out watchers);
                if (watchers == null)
                {
                    watchers = new HashSet<Watcher>();
                    watches[clientPath] = watchers;
                }
               if (!watchers.Any(p => p.GetType() == watcher.GetType()))
                watchers.Add(watcher);
            }
        }
    }
}
