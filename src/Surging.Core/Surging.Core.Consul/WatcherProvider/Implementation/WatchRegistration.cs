using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Collections.Concurrent;
using System.Threading;
using System.Linq.Expressions;

namespace Surging.Core.Consul.WatcherProvider.Implementation
{
   public  abstract class WatchRegistration
    {
        private ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim();
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
            cacheLock.EnterWriteLock();
            try
            {
                var watches = GetWatches();

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
            finally { cacheLock.ExitWriteLock(); }

        }

        public void UnRegister()
        {
            cacheLock.EnterWriteLock();
            try
            {
                var watches = GetWatches();
                HashSet<Watcher> watchers;
                watches.Remove(clientPath, out watchers);
            }
            finally { cacheLock.ExitWriteLock(); }
        }
    }
}
