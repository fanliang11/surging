using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Exceptions;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Runtime.Client;
using Surging.Core.CPlatform.Runtime.Client.Address.Resolvers;
using Surging.Core.CPlatform.Runtime.Client.HealthChecks;
using Surging.Core.CPlatform.Transport;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Mqtt.Internal.Runtime.Implementation
{
    /// <summary>
    /// Defines the <see cref="MqttRemoteInvokeService" />
    /// </summary>
    public class MqttRemoteInvokeService : IMqttRemoteInvokeService
    {
        #region 字段

        /// <summary>
        /// Defines the _healthCheckService
        /// </summary>
        private readonly IHealthCheckService _healthCheckService;

        /// <summary>
        /// Defines the _logger
        /// </summary>
        private readonly ILogger<MqttRemoteInvokeService> _logger;

        /// <summary>
        /// Defines the _mqttBrokerEntryManger
        /// </summary>
        private readonly IMqttBrokerEntryManger _mqttBrokerEntryManger;

        /// <summary>
        /// Defines the _transportClientFactory
        /// </summary>
        private readonly ITransportClientFactory _transportClientFactory;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="MqttRemoteInvokeService"/> class.
        /// </summary>
        /// <param name="transportClientFactory">The transportClientFactory<see cref="ITransportClientFactory"/></param>
        /// <param name="logger">The logger<see cref="ILogger{MqttRemoteInvokeService}"/></param>
        /// <param name="healthCheckService">The healthCheckService<see cref="IHealthCheckService"/></param>
        /// <param name="mqttBrokerEntryManger">The mqttBrokerEntryManger<see cref="IMqttBrokerEntryManger"/></param>
        public MqttRemoteInvokeService(ITransportClientFactory transportClientFactory,
            ILogger<MqttRemoteInvokeService> logger,
            IHealthCheckService healthCheckService,
            IMqttBrokerEntryManger mqttBrokerEntryManger)
        {
            _transportClientFactory = transportClientFactory;
            _logger = logger;
            _healthCheckService = healthCheckService;
            _mqttBrokerEntryManger = mqttBrokerEntryManger;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The InvokeAsync
        /// </summary>
        /// <param name="context">The context<see cref="RemoteInvokeContext"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task InvokeAsync(RemoteInvokeContext context)
        {
            await InvokeAsync(context, Task.Factory.CancellationToken);
        }

        /// <summary>
        /// The InvokeAsync
        /// </summary>
        /// <param name="context">The context<see cref="RemoteInvokeContext"/></param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task InvokeAsync(RemoteInvokeContext context, CancellationToken cancellationToken)
        {
            var mqttContext = context as MqttRemoteInvokeContext;
            if (mqttContext != null)
            {
                var invokeMessage = context.InvokeMessage;
                var host = NetUtils.GetHostAddress();
                var addresses = await _mqttBrokerEntryManger.GetMqttBrokerAddress(mqttContext.topic);
                addresses = addresses.Except(new AddressModel[] { host });
                foreach (var address in addresses)
                {
                    try
                    {
                        var endPoint = address.CreateEndPoint();
                        if (_logger.IsEnabled(LogLevel.Debug))
                            _logger.LogDebug($"使用地址：'{endPoint}'进行调用。");
                        var client = await _transportClientFactory.CreateClientAsync(endPoint);
                        await client.SendAsync(invokeMessage, cancellationToken).WithCancellation(cancellationToken);
                    }
                    catch (CommunicationException)
                    {
                        await _healthCheckService.MarkFailure(address);
                    }
                    catch (Exception exception)
                    {
                        _logger.LogError(exception, $"发起请求中发生了错误，服务Id：{invokeMessage.ServiceId}。");
                    }
                }
            }
        }

        /// <summary>
        /// The InvokeAsync
        /// </summary>
        /// <param name="context">The context<see cref="RemoteInvokeContext"/></param>
        /// <param name="requestTimeout">The requestTimeout<see cref="int"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task InvokeAsync(RemoteInvokeContext context, int requestTimeout)
        {
            var mqttContext = context as MqttRemoteInvokeContext;
            if (mqttContext != null)
            {
                var invokeMessage = context.InvokeMessage;
                var host = NetUtils.GetHostAddress();
                var addresses = await _mqttBrokerEntryManger.GetMqttBrokerAddress(mqttContext.topic);
                if (addresses != null)
                {
                    addresses = addresses.Except(new AddressModel[] { host });
                    foreach (var address in addresses)
                    {
                        try
                        {
                            var endPoint = address.CreateEndPoint();
                            if (_logger.IsEnabled(LogLevel.Debug))
                                _logger.LogDebug($"使用地址：'{endPoint}'进行调用。");
                            var client = await _transportClientFactory.CreateClientAsync(endPoint);
                            using (var cts = new CancellationTokenSource())
                            {
                                await client.SendAsync(invokeMessage, cts.Token).WithCancellation(cts, requestTimeout);
                            }
                        }
                        catch (CommunicationException)
                        {
                            await _healthCheckService.MarkFailure(address);
                        }
                        catch (Exception exception)
                        {
                            _logger.LogError(exception, $"发起mqtt请求中发生了错误，服务Id：{invokeMessage.ServiceId}。");
                        }
                    }
                }
            }
        }

        #endregion 方法
    }
}