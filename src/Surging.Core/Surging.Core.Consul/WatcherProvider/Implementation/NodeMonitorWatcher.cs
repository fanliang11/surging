using Consul;
using Surging.Core.Consul.Utilitys;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Consul.WatcherProvider.Implementation
{
    /// <summary>
    /// Defines the <see cref="NodeMonitorWatcher" />
    /// </summary>
    internal class NodeMonitorWatcher : WatcherBase
    {
        #region 字段

        /// <summary>
        /// Defines the _action
        /// </summary>
        private readonly Action<byte[], byte[]> _action;

        /// <summary>
        /// Defines the _clientCall
        /// </summary>
        private readonly Func<ValueTask<ConsulClient>> _clientCall;

        /// <summary>
        /// Defines the _manager
        /// </summary>
        private readonly IClientWatchManager _manager;

        /// <summary>
        /// Defines the _path
        /// </summary>
        private readonly string _path;

        /// <summary>
        /// Defines the _allowChange
        /// </summary>
        internal Func<string, bool> _allowChange;

        /// <summary>
        /// Defines the _currentData
        /// </summary>
        private byte[] _currentData = new byte[0];

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeMonitorWatcher"/> class.
        /// </summary>
        /// <param name="clientCall">The clientCall<see cref="Func{ValueTask{ConsulClient}}"/></param>
        /// <param name="manager">The manager<see cref="IClientWatchManager"/></param>
        /// <param name="path">The path<see cref="string"/></param>
        /// <param name="action">The action<see cref="Action{byte[], byte[]}"/></param>
        /// <param name="allowChange">The allowChange<see cref="Func{string,bool}"/></param>
        public NodeMonitorWatcher(Func<ValueTask<ConsulClient>> clientCall, IClientWatchManager manager, string path,
            Action<byte[], byte[]> action, Func<string, bool> allowChange)
        {
            this._action = action;
            _manager = manager;
            _clientCall = clientCall;
            _path = path;
            _allowChange = allowChange;
            RegisterWatch();
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The SetCurrentData
        /// </summary>
        /// <param name="currentData">The currentData<see cref="byte[]"/></param>
        /// <returns>The <see cref="NodeMonitorWatcher"/></returns>
        public NodeMonitorWatcher SetCurrentData(byte[] currentData)
        {
            _currentData = currentData;
            return this;
        }

        /// <summary>
        /// The ProcessImpl
        /// </summary>
        /// <returns>The <see cref="Task"/></returns>
        protected override async Task ProcessImpl()
        {
            RegisterWatch(this);
            if (_allowChange != null && !_allowChange(_path)) return;
            var client = await _clientCall();
            var result = await client.GetDataAsync(_path);
            if (result != null)
            {
                _action(_currentData, result);
                this.SetCurrentData(result);
            }
        }

        /// <summary>
        /// The RegisterWatch
        /// </summary>
        /// <param name="watcher">The watcher<see cref="Watcher"/></param>
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

        #endregion 方法
    }
}