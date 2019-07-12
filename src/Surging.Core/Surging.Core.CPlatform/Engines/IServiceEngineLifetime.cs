using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Surging.Core.CPlatform.Engines
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IServiceEngineLifetime" />
    /// </summary>
    public interface IServiceEngineLifetime
    {
        #region 属性

        /// <summary>
        /// Gets the ServiceEngineStarted
        /// </summary>
        CancellationToken ServiceEngineStarted { get; }

        /// <summary>
        /// Gets the ServiceEngineStopped
        /// </summary>
        CancellationToken ServiceEngineStopped { get; }

        /// <summary>
        /// Gets the ServiceEngineStopping
        /// </summary>
        CancellationToken ServiceEngineStopping { get; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The NotifyStarted
        /// </summary>
        void NotifyStarted();

        /// <summary>
        /// The NotifyStopped
        /// </summary>
        void NotifyStopped();

        /// <summary>
        /// The StopApplication
        /// </summary>
        void StopApplication();

        #endregion 方法
    }

    #endregion 接口
}