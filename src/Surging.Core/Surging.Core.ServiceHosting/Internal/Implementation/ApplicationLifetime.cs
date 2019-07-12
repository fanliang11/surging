using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Surging.Core.ServiceHosting.Internal.Implementation
{
    /// <summary>
    /// Defines the <see cref="ApplicationLifetime" />
    /// </summary>
    public class ApplicationLifetime : IApplicationLifetime
    {
        #region 字段

        /// <summary>
        /// Defines the _logger
        /// </summary>
        private readonly ILogger<ApplicationLifetime> _logger;

        /// <summary>
        /// Defines the _startedSource
        /// </summary>
        private readonly CancellationTokenSource _startedSource = new CancellationTokenSource();

        /// <summary>
        /// Defines the _stoppedSource
        /// </summary>
        private readonly CancellationTokenSource _stoppedSource = new CancellationTokenSource();

        /// <summary>
        /// Defines the _stoppingSource
        /// </summary>
        private readonly CancellationTokenSource _stoppingSource = new CancellationTokenSource();

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationLifetime"/> class.
        /// </summary>
        /// <param name="logger">The logger<see cref="ILogger{ApplicationLifetime}"/></param>
        public ApplicationLifetime(ILogger<ApplicationLifetime> logger)
        {
            _logger = logger;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the ApplicationStarted
        /// </summary>
        public CancellationToken ApplicationStarted => _startedSource.Token;

        /// <summary>
        /// Gets the ApplicationStopped
        /// </summary>
        public CancellationToken ApplicationStopped => _stoppedSource.Token;

        /// <summary>
        /// Gets the ApplicationStopping
        /// </summary>
        public CancellationToken ApplicationStopping => _stoppingSource.Token;

        #endregion 属性

        #region 方法

        /// <summary>
        /// The NotifyStarted
        /// </summary>
        public void NotifyStarted()
        {
            try
            {
                ExecuteHandlers(_startedSource);
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred starting the application",
                                         ex);
            }
        }

        /// <summary>
        /// The NotifyStopped
        /// </summary>
        public void NotifyStopped()
        {
            try
            {
                ExecuteHandlers(_stoppedSource);
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred stopping the application",
                                         ex);
            }
        }

        /// <summary>
        /// The StopApplication
        /// </summary>
        public void StopApplication()
        {
            lock (_stoppingSource)
            {
                try
                {
                    ExecuteHandlers(_stoppedSource);
                }
                catch (Exception ex)
                {
                    _logger.LogError("An error occurred stopping the application",
                                             ex);
                }
            }
        }

        /// <summary>
        /// The ExecuteHandlers
        /// </summary>
        /// <param name="cancel">The cancel<see cref="CancellationTokenSource"/></param>
        private void ExecuteHandlers(CancellationTokenSource cancel)
        {
            if (cancel.IsCancellationRequested)
            {
                return;
            }
            cancel.Cancel(throwOnFirstException: false);
        }

        #endregion 方法
    }
}