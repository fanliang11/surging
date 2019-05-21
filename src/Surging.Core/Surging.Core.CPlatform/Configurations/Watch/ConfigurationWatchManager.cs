using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Configurations.Watch
{
    public class ConfigurationWatchManager: IConfigurationWatchManager
    {
        internal   HashSet<ConfigurationWatch> dataWatches =
            new  HashSet<ConfigurationWatch>();
        private readonly Timer _timer;
        private readonly ILogger<ConfigurationWatchManager> _logger;

        public ConfigurationWatchManager(ILogger<ConfigurationWatchManager> logger)
        {
            _logger = logger;
            var timeSpan = TimeSpan.FromSeconds(AppConfig.ServerOptions.WatchInterval);
            _timer = new Timer(async s =>
            {
                await Watching();
            }, null, timeSpan, timeSpan);
        }

        public  HashSet<ConfigurationWatch> DataWatches
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

        public void Register(ConfigurationWatch watch)
        {
            lock (dataWatches)
            {
               if( !dataWatches.Contains(watch))
                dataWatches.Add(watch);
            }
        }

        private async Task Watching()
        { 
            foreach (var watch in dataWatches)
            {
                try
                {
                    var task= watch.Process();
                    if (!task.IsCompletedSuccessfully)
                        await task;
                    else
                        task.GetAwaiter().GetResult();
                }
                catch(Exception ex)
                { 
                    if (_logger.IsEnabled(LogLevel.Error))
                        _logger.LogError($"message:{ex.Message},Source:{ex.Source},Trace:{ex.StackTrace}");
                }
            }
        }
    }
}
