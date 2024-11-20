using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Consul.WatcherProvider
{
    public interface IClientWatchManager
    {
        ConcurrentDictionary<string, HashSet<Watcher>> DataWatches { get; set; }
    }
}
