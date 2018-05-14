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

namespace Surging.Core.CPlatform.Support.Implementation
{
    public class BreakeRemoteInvokeService : IBreakeRemoteInvokeService
    {
        private readonly IServiceCommandProvider _commandProvider;
        private readonly IRemoteInvokeService _remoteInvokeService;
        private readonly ILogger<BreakeRemoteInvokeService> _logger;
        private readonly ConcurrentDictionary<string, ServiceInvokeListenInfo> _serviceInvokeListenInfo = new ConcurrentDictionary<string, ServiceInvokeListenInfo>();
        private readonly IHashAlgorithm _hashAlgorithm;

        public BreakeRemoteInvokeService(IHashAlgorithm hashAlgorithm, IServiceCommandProvider commandProvider, ILogger<BreakeRemoteInvokeService> logger, IRemoteInvokeService remoteInvokeService)
        {
            _commandProvider = commandProvider;
            _remoteInvokeService = remoteInvokeService;
            _logger = logger;
            _hashAlgorithm = hashAlgorithm;
        }

        public async Task<RemoteInvokeResultMessage> InvokeAsync(IDictionary<string, object> parameters, string serviceId, string serviceKey, bool decodeJOject)
        {
            var serviceInvokeInfos = _serviceInvokeListenInfo.GetOrAdd(serviceId,
                new ServiceInvokeListenInfo() { FirstInvokeTime=DateTime.Now,
                FinalRemoteInvokeTime =DateTime.Now });
            var command = await _commandProvider.GetCommand(serviceId);
            var intervalSeconds = (DateTime.Now - serviceInvokeInfos.FinalRemoteInvokeTime).TotalSeconds;
            bool reachConcurrentRequest() => serviceInvokeInfos.ConcurrentRequests > command.MaxConcurrentRequests;
            bool reachRequestVolumeThreshold() => intervalSeconds <= 10
                && serviceInvokeInfos.SinceFaultRemoteServiceRequests > command.BreakerRequestVolumeThreshold;
            bool reachErrorThresholdPercentage() =>
                serviceInvokeInfos.FaultRemoteServiceRequests / (serviceInvokeInfos.RemoteServiceRequests ?? 1) * 100 > command.BreakeErrorThresholdPercentage;
            var hashCode = GetHashCode(command,parameters);
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
                        return await MonitorRemoteInvokeAsync(parameters, serviceId, serviceKey, decodeJOject, command.ExecutionTimeoutInMilliseconds, hashCode);
                    }
                    else
                    {
                        _serviceInvokeListenInfo.AddOrUpdate(serviceId, new ServiceInvokeListenInfo(), (k, v) => { v.LocalServiceRequests++; return v; });
                        return null;
                    }
                }
                else
                {
                    return await MonitorRemoteInvokeAsync(parameters, serviceId, serviceKey, decodeJOject, command.ExecutionTimeoutInMilliseconds, hashCode);
                }
            }
        }

        private async Task<RemoteInvokeResultMessage> MonitorRemoteInvokeAsync(IDictionary<string, object> parameters, string serviceId, string serviceKey, bool decodeJOject, int requestTimeout,int hashCode)
        {
            var serviceInvokeInfo = _serviceInvokeListenInfo.GetOrAdd(serviceId, new ServiceInvokeListenInfo());
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
                    HashCode=hashCode ,
                    InvokeMessage = new RemoteInvokeMessage
                    {
                        Parameters = parameters,
                        ServiceId = serviceId,
                        ServiceKey = serviceKey,
                        DecodeJOject = decodeJOject,
                        Attachments = RpcContext.GetContext().GetContextParameters()
                    }
                }, requestTimeout);
                _serviceInvokeListenInfo.AddOrUpdate(serviceId, new ServiceInvokeListenInfo(), (k, v) =>
                {
                    v.SinceFaultRemoteServiceRequests = 0;
                    --v.ConcurrentRequests; return v;
                });
                return message;
            }
            catch
            {
                _serviceInvokeListenInfo.AddOrUpdate(serviceId, new ServiceInvokeListenInfo(), (k, v) =>
                {
                    ++v.FaultRemoteServiceRequests;
                    ++v.SinceFaultRemoteServiceRequests;
                    --v.ConcurrentRequests;
                    return v;
                });
                return null;
            }
        }

        private int GetHashCode(ServiceCommand command, IDictionary<string, object> parameters)
        {
            var result = 0;
            if(command.ShuntStrategy==AddressSelectorMode.HashAlgorithm)
            {
                var parameter = parameters.Values.FirstOrDefault();
                result= _hashAlgorithm.Hash(parameter?.ToString());
            }
            return result;
        }
    }
}
