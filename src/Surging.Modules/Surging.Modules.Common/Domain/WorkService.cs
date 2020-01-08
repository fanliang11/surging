using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Ioc;
using Surging.Core.ProxyGenerator;
using Surging.Core.ServiceHosting.Extensions.Runtime;
using Surging.IModuleServices.Common;
using Surging.IModuleServices.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Modules.Common.Domain
{
    public class WorkService : BackgroundServiceBehavior, IWorkService, ISingleInstance
    {
        private readonly ILogger<WorkService> _logger;
        private   readonly Queue<Message> _queue = new Queue<Message>();
        private readonly IServiceProxyProvider _serviceProxyProvider;
        private CancellationToken _token;

        public WorkService(ILogger<WorkService> logger, IServiceProxyProvider serviceProxyProvider)
        {
            _logger = logger;
            _serviceProxyProvider = serviceProxyProvider;
        }

        public  Task<bool> AddWork(Message message)
        {
            _queue.Enqueue(message);
            return Task.FromResult(true);
        }

        protected override async  Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _token = stoppingToken;
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                _queue.TryDequeue(out Message message);
                if (message != null)
                {
                    var result = await _serviceProxyProvider.Invoke<object>(message.Parameters, message.RoutePath, message.ServiceKey);
                    _logger.LogInformation("Invoke Service at: {time},Invoke result:{result}", DateTimeOffset.Now, result);
                }
                if (!_token.IsCancellationRequested)
                    await Task.Delay(1000, stoppingToken);
            }
            catch (Exception ex){
                _logger.LogError("WorkService execute error, message：{message} ,trace info:{trace} ", ex.Message, ex.StackTrace);
            }
        }

        public async Task StartAsync()
        {
            if (_token.IsCancellationRequested)
            { 
                await base.StartAsync(_token);
            }
        }

        public async Task StopAsync()
        {
            if (!_token.IsCancellationRequested)
            {
               await  base.StopAsync(_token);
            }
        }
    }
}
