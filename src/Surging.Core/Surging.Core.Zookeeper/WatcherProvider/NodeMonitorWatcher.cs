using org.apache.zookeeper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Zookeeper.WatcherProvider
{
    internal class NodeMonitorWatcher : WatcherBase
    {
        private readonly ZooKeeper _zooKeeper;
        private readonly Action<byte[], byte[]> _action;
        private byte[] _currentData;

        public NodeMonitorWatcher(ZooKeeper zooKeeper, string path, Action<byte[], byte[]> action) : base(path)
        {
            _zooKeeper = zooKeeper;
            _action = action;
        }

        public NodeMonitorWatcher SetCurrentData(byte[] currentData)
        {
            _currentData = currentData;

            return this;
        }

        #region Overrides of WatcherBase

        protected override async Task ProcessImpl(WatchedEvent watchedEvent)
        {
            var path = Path;
            switch (watchedEvent.get_Type())
            {
                case Event.EventType.NodeDataChanged:
                    var watcher = new NodeMonitorWatcher(_zooKeeper, path, _action);
                    var data = await _zooKeeper.getDataAsync(path, watcher);
                    var newData = data.Data;
                    _action(_currentData, newData);
                    watcher.SetCurrentData(newData);
                    break;
            }
        }

        #endregion Overrides of WatcherBase
    }
}