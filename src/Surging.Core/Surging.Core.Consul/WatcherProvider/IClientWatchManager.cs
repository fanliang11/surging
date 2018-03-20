using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Consul.WatcherProvider
{
    public interface IClientWatchManager
    {
        Dictionary<string, HashSet<Watcher>> DataWatches { get; set; }
    }
}
