using org.apache.zookeeper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Zookeeper.WatcherProvider
{
    internal class ReconnectionWatcher : Watcher
    {
        private readonly Action _connectioned;
        private readonly Action _disconnect;
        private readonly Action _reconnection;

        public ReconnectionWatcher(Action connectioned, Action disconnect, Action reconnection)
        {
            _connectioned = connectioned;
            _disconnect = disconnect;
            _reconnection = reconnection;
        }

        #region Overrides of Watcher

        /// <summary>Processes the specified event.</summary>
        /// <param name="watchedEvent">The event.</param>
        /// <returns></returns>
        public override async Task process(WatchedEvent watchedEvent)
        {
            var state = watchedEvent.getState();
            switch (state)
            {
                case Event.KeeperState.Expired:
                    {
                        _reconnection();
                        break;
                    }
                case Event.KeeperState.AuthFailed:
                    {
                        _disconnect();
                        break;
                    }
                case Event.KeeperState.Disconnected:
                    {
                        _reconnection();
                        break;
                    }
                default:
                    {
                        _connectioned();
                        break;
                    }
            }

#if NET
                await Task.FromResult(1);
#else
            await Task.CompletedTask;
#endif
        }

        #endregion Overrides of Watcher
    }
}
