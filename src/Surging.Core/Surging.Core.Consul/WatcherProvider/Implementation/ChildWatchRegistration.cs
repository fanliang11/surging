using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Consul.WatcherProvider.Implementation
{
  public  class ChildWatchRegistration : WatchRegistration
    {
        private readonly IClientWatchManager watchManager;

        public ChildWatchRegistration(IClientWatchManager watchManager, Watcher watcher, string clientPath)
            : base(watcher, clientPath)
        {
            this.watchManager = watchManager;
        }

        protected override Dictionary<string, HashSet<Watcher>> GetWatches()
        {
            return watchManager.DataWatches;
        }
    }
}
