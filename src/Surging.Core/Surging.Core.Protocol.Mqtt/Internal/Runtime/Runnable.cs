using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Surging.Core.Protocol.Mqtt.Internal.Runtime
{
    /// <summary>
    /// Defines the <see cref="Runnable" />
    /// </summary>
    public abstract class Runnable
    {
        #region 字段

        /// <summary>
        /// Defines the _timer
        /// </summary>
        private readonly Timer _timer;

        /// <summary>
        /// Defines the _runnableThread
        /// </summary>
        private volatile Thread _runnableThread;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="Runnable"/> class.
        /// </summary>
        public Runnable()
        {
            var timeSpan = TimeSpan.FromSeconds(3);
            _timer = new Timer(s =>
           {
               Run();
           }, null, timeSpan, timeSpan);
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The Run
        /// </summary>
        public abstract void Run();

        #endregion 方法
    }
}