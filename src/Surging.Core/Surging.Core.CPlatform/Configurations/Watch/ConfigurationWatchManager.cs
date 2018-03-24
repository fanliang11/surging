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

        public ConfigurationWatchManager()
        {
            var timeSpan = TimeSpan.FromSeconds(AppConfig.ServerOptions.WatchInterval);
            _timer = new Timer(async s =>
            {
                await Watching();
            }, null, timeSpan, timeSpan);
        }

        public   HashSet<ConfigurationWatch> DataWatches
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
                await watch.Process();
            }
        }
    }
}
