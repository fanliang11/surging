using Consul;
using Microsoft.Extensions.Logging;
using Surging.Core.Consul.Configurations;
using Surging.Core.Consul.Utilitys;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Surging.Core.Consul.WatcherProvider.Implementation
{
    public class ClientWatchManager : IClientWatchManager
    {
        internal ConcurrentDictionary<string, HashSet<Watcher>> dataWatches =
            new ConcurrentDictionary<string, HashSet<Watcher>>();
        private readonly Timer _timer;
        private readonly ILogger<ClientWatchManager> _logger;

        public ClientWatchManager(ILogger<ClientWatchManager> logger,ConfigInfo config)
        {
            var timeSpan = TimeSpan.FromSeconds(config.WatchInterval);
            _logger = logger;
            _timer = new Timer(async s =>
            {
               await Watching();
            }, null, timeSpan, timeSpan);
        }

        public ConcurrentDictionary<string, HashSet<Watcher>> DataWatches { get
            {
                return dataWatches;
            }
            set
            {
                dataWatches = value;
            }
        }

        private HashSet<Watcher> Materialize()
        {
            HashSet<Watcher> result = new HashSet<Watcher>();

            foreach (HashSet<Watcher> ws in dataWatches.Values)
            {
                result.UnionWith(ws);
            }
            return result;
        }

        private async Task Watching()
        {
            try
            {
                var watches = Materialize();
                foreach (var watch in watches)
                {
                    await watch.Process();
                }
            }
            catch (Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError($"message:{ex.Message},Source:{ex.Source},Trace:{ex.StackTrace}");
            }
        }
    }
}

