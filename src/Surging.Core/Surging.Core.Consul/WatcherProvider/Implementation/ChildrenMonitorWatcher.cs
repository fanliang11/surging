using Consul;
using Surging.Core.Consul.Utilitys;
using Surging.Core.Consul.WatcherProvider.Implementation;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Surging.Core.Consul.WatcherProvider
{
    /// <summary>
    /// Defines the <see cref="ChildrenMonitorWatcher" />
    /// </summary>
    public class ChildrenMonitorWatcher : WatcherBase
    {
        #region 字段

        /// <summary>
        /// Defines the _action
        /// </summary>
        private readonly Action<string[], string[]> _action;

        /// <summary>
        /// Defines the _clientCall
        /// </summary>
        private readonly Func<ValueTask<ConsulClient>> _clientCall;

        /// <summary>
        /// Defines the _func
        /// </summary>
        private readonly Func<string[], string[]> _func;

        /// <summary>
        /// Defines the _manager
        /// </summary>
        private readonly IClientWatchManager _manager;

        /// <summary>
        /// Defines the _path
        /// </summary>
        private readonly string _path;

        /// <summary>
        /// Defines the _currentData
        /// </summary>
        private string[] _currentData = new string[0];

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildrenMonitorWatcher"/> class.
        /// </summary>
        /// <param name="clientCall">The clientCall<see cref="Func{ValueTask{ConsulClient}}"/></param>
        /// <param name="manager">The manager<see cref="IClientWatchManager"/></param>
        /// <param name="path">The path<see cref="string"/></param>
        /// <param name="action">The action<see cref="Action{string[], string[]}"/></param>
        /// <param name="func">The func<see cref="Func{string[], string[]}"/></param>
        public ChildrenMonitorWatcher(Func<ValueTask<ConsulClient>> clientCall, IClientWatchManager manager, string path,
            Action<string[], string[]> action, Func<string[], string[]> func)
        {
            this._action = action;
            _manager = manager;
            _clientCall = clientCall;
            _path = path;
            _func = func;
            RegisterWatch();
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The SetCurrentData
        /// </summary>
        /// <param name="currentData">The currentData<see cref="string[]"/></param>
        /// <returns>The <see cref="ChildrenMonitorWatcher"/></returns>
        public ChildrenMonitorWatcher SetCurrentData(string[] currentData)
        {
            _currentData = currentData ?? new string[0];
            return this;
        }

        /// <summary>
        /// The ProcessImpl
        /// </summary>
        /// <returns>The <see cref="Task"/></returns>
        protected override async Task ProcessImpl()
        {
            RegisterWatch(this);
            var client = await _clientCall();
            var result = await client.GetChildrenAsync(_path);
            if (result != null)
            {
                var convertResult = _func.Invoke(result).Select(key => $"{_path}{key}").ToArray();
                _action(_currentData, convertResult);
                this.SetCurrentData(convertResult);
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