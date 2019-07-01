using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Runtime.Client;
using Surging.Core.CPlatform.Runtime.Client.Address.Resolvers.Implementation.Selectors.Implementation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Surging.Core.CPlatform.HashAlgorithms;
using Surging.Core.CPlatform.Transport.Implementation;
using Surging.Core.CPlatform.Filters;
using Autofac;
using System.Threading;
using Surging.Core.CPlatform.Filters.Implementation;
using System.Runtime.CompilerServices;

namespace Surging.Core.CPlatform.Support.Implementation
{
    public class BreakeRemoteInvokeService : IBreakeRemoteInvokeService
    {
        private readonly IServiceCommandProvider _commandProvider;
        private readonly IRemoteInvokeService _remoteInvokeService;
        private readonly ILogger<BreakeRemoteInvokeService> _logger;
        private readonly ConcurrentDictionary<string, ServiceInvokeListenInfo> _serviceInvokeListenInfo = new ConcurrentDictionary<string, ServiceInvokeListenInfo>();
        private readonly IHashAlgorithm _hashAlgorithm;
        private readonly IEnumerable<IExceptionFilter> exceptionFilters = new List<IExceptionFilter>();

        public BreakeRemoteInvokeService(IHashAlgorithm hashAlgorithm, IServiceCommandProvider commandProvider, ILogger<BreakeRemoteInvokeService> logger,
            IRemoteInvokeService remoteInvokeService,
             CPlatformContainer serviceProvider)
        {
            _commandProvider = commandProvider;
            _remoteInvokeService = remoteInvokeService;
            _logger = logger;
            _hashAlgorithm = hashAlgorithm;
            if (serviceProvider.Current.IsRegistered<IExceptionFilter>())
                exceptionFilters = serviceProvider.GetInstances<IEnumerable<IExceptionFilter>>();
        }

        public async Task<RemoteInvokeResultMessage> InvokeAsync(IDictionary<string, object> parameters, string serviceId, string serviceKey, bool decodeJOject)
        {
            var serviceInvokeInfos = _serviceInvokeListenInfo.GetOrAdd(serviceId,
                new ServiceInvokeListenInfo()
                {
                    FirstInvokeTime = DateTime.Now,
                    FinalRemoteInvokeTime = DateTime.Now
                });
            var vt = _commandProvider.GetCommand(serviceId);
            var command = vt.IsCompletedSuccessfully ? vt.Result : await vt;
            var intervalSeconds = (DateTime.Now - serviceInvokeInfos.FinalRemoteInvokeTime).TotalSeconds;
            bool reachConcurrentRequest() => serviceInvokeInfos.ConcurrentRequests > command.MaxConcurrentRequests;
            bool reachRequestVolumeThreshold() => intervalSeconds <= 10
                && serviceInvokeInfos.SinceFaultRemoteServiceRequests > command.BreakerRequestVolumeThreshold;
            bool reachErrorThresholdPercentage() =>
                (double)serviceInvokeInfos.FaultRemoteServiceRequests / (double)(serviceInvokeInfos.RemoteServiceRequests ?? 1) * 100 > command.BreakeErrorThresholdPercentage;
            var item = GetHashItem(command, parameters);
            if (command.BreakerForceClosed)
            {
                _serviceInvokeListenInfo.AddOrUpdate(serviceId, new ServiceInvokeListenInfo(), (k, v) => { v.LocalServiceRequests++; return v; });
                return null;
            }
            else
            {
                if (reachConcurrentRequest() || reachRequestVolumeThreshold() || reachErrorThresholdPercentage())
                {
                    if (intervalSeconds * 1000 > command.BreakeSleepWindowInMilliseconds)
                    {
                        return await MonitorRemoteInvokeAsync(parameters, serviceId, serviceKey, decodeJOject, command.ExecutionTimeoutInMilliseconds, item);
                    }
                    else
                    {
                        _serviceInvokeListenInfo.AddOrUpdate(serviceId, new ServiceInvokeListenInfo(), (k, v) => { v.LocalServiceRequests++; return v; });
                        return null;
                    }
                }
                else
                {
                    return await MonitorRemoteInvokeAsync(parameters, serviceId, serviceKey, decodeJOject, command.ExecutionTimeoutInMilliseconds, item);
                }
            }
        }

        private async Task<RemoteInvokeResultMessage> MonitorRemoteInvokeAsync(IDictionary<string, object> parameters, string serviceId, string serviceKey, bool decodeJOject, int requestTimeout, string item)
        {
            CancellationTokenSource source = new CancellationTokenSource();
            var token = source.Token;
            var invokeMessage = new RemoteInvokeMessage
            {
                Parameters = parameters,
                ServiceId = serviceId,
                ServiceKey = serviceKey,
                DecodeJOject = decodeJOject,
                Attachments = RpcContext.GetContext().GetContextParameters()
            };
            try
            {
                _serviceInvokeListenInfo.AddOrUpdate(serviceId, new ServiceInvokeListenInfo(), (k, v) =>
                {
                    v.RemoteServiceRequests = v.RemoteServiceRequests == null ? 1 : ++v.RemoteServiceRequests;
                    v.FinalRemoteInvokeTime = DateTime.Now;
                    ++v.ConcurrentRequests;
                    return v;
                });
                var message = await _remoteInvokeService.InvokeAsync(new RemoteInvokeContext
                {
                    Item = item,
                    InvokeMessage = invokeMessage
                }, requestTimeout);
                _serviceInvokeListenInfo.AddOrUpdate(serviceId, new ServiceInvokeListenInfo(), (k, v) =>
                {
                    v.SinceFaultRemoteServiceRequests = 0;
                    --v.ConcurrentRequests; return v;
                });
                return message;
            }
            catch (Exception ex)
            {
                _serviceInvokeListenInfo.AddOrUpdate(serviceId, new ServiceInvokeListenInfo(), (k, v) =>
                {
                    ++v.FaultRemoteServiceRequests;
                    ++v.SinceFaultRemoteServiceRequests;
                    --v.ConcurrentRequests;
                    return v;
                });
                await ExecuteExceptionFilter(ex, invokeMessage, token);
                return null;
            }
        }

        private async Task ExecuteExceptionFilter(Exception ex, RemoteInvokeMessage invokeMessage, CancellationToken token)
        {
            foreach (var filter in exceptionFilters)
            {
                await filter.ExecuteExceptionFilterAsync(new RpcActionExecutedContext
                {
                    Exception = ex,
                    InvokeMessage = invokeMessage
                }, token);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string GetHashItem(ServiceCommand command, IDictionary<string, object> parameters)
        {
            string result = "";
            if (command.ShuntStrategy == AddressSelectorMode.HashAlgorithm)
            {
                var parameter = parameters.Values.FirstOrDefault();
                result = parameter?.ToString();
            }
            return result;
        }
    }
}
