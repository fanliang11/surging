using Consul;
using Microsoft.Extensions.Logging;
using Surging.Core.Consul.Configurations;
using Surging.Core.Consul.Utilitys;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Surging.Core.Consul.WatcherProvider.Implementation
{
    /// <summary>
    /// Defines the <see cref="ClientWatchManager" />
    /// </summary>
    public class ClientWatchManager : IClientWatchManager
    {
        #region 字段

        /// <summary>
        /// Defines the _logger
        /// </summary>
        private readonly ILogger<ClientWatchManager> _logger;

        /// <summary>
        /// Defines the _timer
        /// </summary>
        private readonly Timer _timer;

        /// <summary>
        /// Defines the dataWatches
        /// </summary>
        internal Dictionary<string, HashSet<Watcher>> dataWatches =
            new Dictionary<string, HashSet<Watcher>>();

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientWatchManager"/> class.
        /// </summary>
        /// <param name="logger">The logger<see cref="ILogger{ClientWatchManager}"/></param>
        /// <param name="config">The config<see cref="ConfigInfo"/></param>
        public ClientWatchManager(ILogger<ClientWatchManager> logger, ConfigInfo config)
        {
            var timeSpan = TimeSpan.FromSeconds(config.WatchInterval);
            _logger = logger;
            _timer = new Timer(async s =>
            {
                await Watching();
            }, null, timeSpan, timeSpan);
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the DataWatches
        /// </summary>
        public Dictionary<string, HashSet<Watcher>> DataWatches
        {
            get
            {
                return dataWatches;
            }
            set
            {
                dataWatches = value;
            }
        }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The Materialize
        /// </summary>
        /// <returns>The <see cref="HashSet{Watcher}"/></returns>
        private HashSet<Watcher> Materialize()
        {
            HashSet<Watcher> result = new HashSet<Watcher>();
            lock (dataWatches)
            {
                foreach (HashSet<Watcher> ws in dataWatches.Values)
                {
                    result.UnionWith(ws);
                }
            }
            return result;
        }

        /// <summary>
        /// The Watching
        /// </summary>
        /// <returns>The <see cref="Task"/></returns>
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

        #endregion 方法
    }
}