using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Configurations.Watch
{
    /// <summary>
    /// Defines the <see cref="ConfigurationWatchManager" />
    /// </summary>
    public class ConfigurationWatchManager : IConfigurationWatchManager
    {
        #region 字段

        /// <summary>
        /// Defines the _logger
        /// </summary>
        private readonly ILogger<ConfigurationWatchManager> _logger;

        /// <summary>
        /// Defines the _timer
        /// </summary>
        private readonly Timer _timer;

        /// <summary>
        /// Defines the dataWatches
        /// </summary>
        internal HashSet<ConfigurationWatch> dataWatches =
            new HashSet<ConfigurationWatch>();

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationWatchManager"/> class.
        /// </summary>
        /// <param name="logger">The logger<see cref="ILogger{ConfigurationWatchManager}"/></param>
        public ConfigurationWatchManager(ILogger<ConfigurationWatchManager> logger)
        {
            _logger = logger;
            var timeSpan = TimeSpan.FromSeconds(AppConfig.ServerOptions.WatchInterval);
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
        public HashSet<ConfigurationWatch> DataWatches
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
        /// The Register
        /// </summary>
        /// <param name="watch">The watch<see cref="ConfigurationWatch"/></param>
        public void Register(ConfigurationWatch watch)
        {
            lock (dataWatches)
            {
                if (!dataWatches.Contains(watch))
                    dataWatches.Add(watch);
            }
        }

        /// <summary>
        /// The Watching
        /// </summary>
        /// <returns>The <see cref="Task"/></returns>
        private async Task Watching()
        {
            foreach (var watch in dataWatches)
            {
                try
                {
                    var task = watch.Process();
                    if (!task.IsCompletedSuccessfully)
                        await task;
                    else
                        task.GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    if (_logger.IsEnabled(LogLevel.Error))
                        _logger.LogError($"message:{ex.Message},Source:{ex.Source},Trace:{ex.StackTrace}");
                }
            }
        }

        #endregion 方法
    }
}