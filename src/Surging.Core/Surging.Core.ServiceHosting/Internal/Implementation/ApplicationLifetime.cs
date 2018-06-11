using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Surging.Core.ServiceHosting.Internal.Implementation
{
    public class ApplicationLifetime : IApplicationLifetime
    {
        private readonly CancellationTokenSource _startedSource = new CancellationTokenSource();
        private readonly CancellationTokenSource _stoppingSource = new CancellationTokenSource();
        private readonly CancellationTokenSource _stoppedSource = new CancellationTokenSource();
        private readonly ILogger<ApplicationLifetime> _logger;

        public ApplicationLifetime(ILogger<ApplicationLifetime> logger)
        {
            _logger = logger;
        }

        public CancellationToken ApplicationStarted => _startedSource.Token;

        public CancellationToken ApplicationStopping => _stoppingSource.Token;

        public CancellationToken ApplicationStopped => _stoppedSource.Token;

        public void NotifyStarted()
        {
            try
            {
                ExecuteHandlers(_startedSource);
            }
            catch (Exception ex)
            {
                _logger.LogError( "An error occurred starting the application",
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
