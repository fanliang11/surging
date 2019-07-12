using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Surging.Core.ServiceHosting.Internal
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IApplicationLifetime" />
    /// </summary>
    public interface IApplicationLifetime
    {
        #region 属性

        /// <summary>
        /// Gets the ApplicationStarted
        /// </summary>
        CancellationToken ApplicationStarted { get; }

        /// <summary>
        /// Gets the ApplicationStopped
        /// </summary>
        CancellationToken ApplicationStopped { get; }

        /// <summary>
        /// Gets the ApplicationStopping
        /// </summary>
        CancellationToken ApplicationStopping { get; }

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