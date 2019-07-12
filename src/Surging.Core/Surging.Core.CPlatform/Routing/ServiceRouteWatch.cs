using Surging.Core.CPlatform.Configurations;
using Surging.Core.CPlatform.Configurations.Watch;
using Surging.Core.CPlatform.Runtime.Server;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Routing
{
    /// <summary>
    /// Defines the <see cref="ServiceRouteWatch" />
    /// </summary>
    public class ServiceRouteWatch : ConfigurationWatch
    {
        #region 字段

        /// <summary>
        /// Defines the _action
        /// </summary>
        private readonly Action _action;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceRouteWatch"/> class.
        /// </summary>
        /// <param name="serviceProvider">The serviceProvider<see cref="CPlatformContainer"/></param>
        /// <param name="action">The action<see cref="Action"/></param>
        public ServiceRouteWatch(CPlatformContainer serviceProvider, Action action)
        {
            this._action = action;
            if (serviceProvider.IsRegistered<IConfigurationWatchManager>())
                serviceProvider.GetInstances<IConfigurationWatchManager>().Register(this);
            _action.Invoke();
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The Process
        /// </summary>
        /// <returns>The <see cref="Task"/></returns>
        public override async Task Process()
        {
            await Task.Run(_action);
        }

        #endregion 方法
    }
}