using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Surging.Core.CPlatform.Engines.Implementation
{
   public  class ServiceEngineLifetime: IServiceEngineLifetime
    {
        private readonly CancellationTokenSource _startedSource = new CancellationTokenSource();
        private readonly CancellationTokenSource _stoppingSource = new CancellationTokenSource();
        private readonly CancellationTokenSource _stoppedSource = new CancellationTokenSource();
        private readonly ILogger<ServiceEngineLifetime> _logger;

        public ServiceEngineLifetime(ILogger<ServiceEngineLifetime> logger)
        {
            _logger = logger;
        }

        public CancellationToken ServiceEngineStarted => _startedSource.Token;

        public CancellationToken ServiceEngineStopping => _stoppingSource.Token;

        public CancellationToken ServiceEngineStopped => _stoppedSource.Token;

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

        private void ExecuteHandlers(CancellationTokenSource cancel)
        {
            if (cancel.IsCancellationRequested)
            {
                return;
            }
            cancel.Cancel(throwOnFirstException: false);
        }
    }
}
