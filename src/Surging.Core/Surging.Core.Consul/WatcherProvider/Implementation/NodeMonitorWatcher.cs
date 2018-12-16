using Consul;
using Surging.Core.Consul.Utilitys;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Consul.WatcherProvider.Implementation
{
    class NodeMonitorWatcher : WatcherBase
    {
        private readonly Action<byte[], byte[]> _action;
        private readonly IClientWatchManager _manager;
        private readonly ConsulClient _client;
        private readonly string _path;
        private byte[] _currentData = new byte[0];
        Func<string,bool> _allowChange;
        public NodeMonitorWatcher(ConsulClient client, IClientWatchManager manager, string path,
            Action<byte[], byte[]> action,Func<string,bool> allowChange)
        {
            this._action = action;
            _manager = manager;
            _client = client;
            _path = path;
            _allowChange = allowChange;
            RegisterWatch();
        }

        public NodeMonitorWatcher SetCurrentData(byte[] currentData)
        {
            _currentData = currentData;
            return this;
        }

        protected override async Task ProcessImpl()
        {
            RegisterWatch(this);
            if (_allowChange!=null&&! _allowChange(_path)) return;
            var result = await _client.GetDataAsync(_path);
            if (result != null)
            {
                _action(_currentData, result);
                this.SetCurrentData(result);
            }
        }

        private void RegisterWatch(Watcher watcher = null)
        {
            ChildWatchRegistration wcb = null;
            if (watcher != null)
            {
                wcb = new ChildWatchRegistration(_manager, watcher, _path);
            }
            else
            {
                wcb = new ChildWatchRegistration(_manager, this, _path);
            }
            wcb.Register();
        }
    }
}