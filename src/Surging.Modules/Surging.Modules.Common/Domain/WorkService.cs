using Microsoft.Extensions.Logging;
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
    public class WorkService : BackgroundServiceBehavior, IWorkService
    {
        private readonly ILogger<WorkService> _logger;
        private readonly Queue<Message> _queue = new Queue<Message>();
        private readonly IServiceProxyProvider _serviceProxyProvider;

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
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            _queue.TryDequeue(out Message message);
            if (message != null)
            {
               var result= await _serviceProxyProvider.Invoke<object>(message.Parameters, message.RoutePath, message.ServiceKey);
                _logger.LogInformation("Invoke Service at: {time}", DateTimeOffset.Now);
            }
            await Task.Delay(1000, stoppingToken);
        }
    }
}
