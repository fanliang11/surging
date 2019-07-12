using Autofac;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Filters;
using Surging.Core.CPlatform.Filters.Implementation;
using Surging.Core.CPlatform.HashAlgorithms;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Runtime.Client;
using Surging.Core.CPlatform.Runtime.Client.Address.Resolvers.Implementation.Selectors.Implementation;
using Surging.Core.CPlatform.Transport.Implementation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Support.Implementation
{
    /// <summary>
    /// Defines the <see cref="BreakeRemoteInvokeService" />
    /// </summary>
    public class BreakeRemoteInvokeService : IBreakeRemoteInvokeService
    {
        #region 字段

        /// <summary>
        /// Defines the _commandProvider
        /// </summary>
        private readonly IServiceCommandProvider _commandProvider;

        /// <summary>
        /// Defines the _hashAlgorithm
        /// </summary>
        private readonly IHashAlgorithm _hashAlgorithm;

        /// <summary>
        /// Defines the _logger
        /// </summary>
        private readonly ILogger<BreakeRemoteInvokeService> _logger;

        /// <summary>
        /// Defines the _remoteInvokeService
        /// </summary>
        private readonly IRemoteInvokeService _remoteInvokeService;

        /// <summary>
        /// Defines the _serviceInvokeListenInfo
        /// </summary>
        private readonly ConcurrentDictionary<string, ServiceInvokeListenInfo> _serviceInvokeListenInfo = new ConcurrentDictionary<string, ServiceInvokeListenInfo>();

        /// <summary>
        /// Defines the exceptionFilters
        /// </summary>
        private readonly IEnumerable<IExceptionFilter> exceptionFilters = new List<IExceptionFilter>();

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="BreakeRemoteInvokeService"/> class.
        /// </summary>
        /// <param name="hashAlgorithm">The hashAlgorithm<see cref="IHashAlgorithm"/></param>
        /// <param name="commandProvider">The commandProvider<see cref="IServiceCommandProvider"/></param>
        /// <param name="logger">The logger<see cref="ILogger{BreakeRemoteInvokeService}"/></param>
        /// <param name="remoteInvokeService">The remoteInvokeService<see cref="IRemoteInvokeService"/></param>
        /// <param name="serviceProvider">The serviceProvider<see cref="CPlatformContainer"/></param>
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

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The InvokeAsync
        /// </summary>
        /// <param name="parameters">The parameters<see cref="IDictionary{string, object}"/></param>
        /// <param name="serviceId">The serviceId<see cref="string"/></param>
        /// <param name="serviceKey">The serviceKey<see cref="string"/></param>
        /// <param name="decodeJOject">The decodeJOject<see cref="bool"/></param>
        /// <returns>The <see cref="Task{RemoteInvokeResultMessage}"/></returns>
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

        /// <summary>
        /// The ExecuteExceptionFilter
        /// </summary>
        /// <param name="ex">The ex<see cref="Exception"/></param>
        /// <param name="invokeMessage">The invokeMessage<see cref="RemoteInvokeMessage"/></param>
        /// <param name="token">The token<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task"/></returns>
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

        /// <summary>
        /// The GetHashItem
        /// </summary>
        /// <param name="command">The command<see cref="ServiceCommand"/></param>
        /// <param name="parameters">The parameters<see cref="IDictionary{string, object}"/></param>
        /// <returns>The <see cref="string"/></returns>
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

        /// <summary>
        /// The MonitorRemoteInvokeAsync
        /// </summary>
        /// <param name="parameters">The parameters<see cref="IDictionary{string, object}"/></param>
        /// <param name="serviceId">The serviceId<see cref="string"/></param>
        /// <param name="serviceKey">The serviceKey<see cref="string"/></param>
        /// <param name="decodeJOject">The decodeJOject<see cref="bool"/></param>
        /// <param name="requestTimeout">The requestTimeout<see cref="int"/></param>
        /// <param name="item">The item<see cref="string"/></param>
        /// <returns>The <see cref="Task{RemoteInvokeResultMessage}"/></returns>
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

        #endregion 方法
    }
}