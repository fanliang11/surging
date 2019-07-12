using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Exceptions;
using Surging.Core.CPlatform.HashAlgorithms;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Runtime.Client.Address.Resolvers;
using Surging.Core.CPlatform.Runtime.Client.HealthChecks;
using Surging.Core.CPlatform.Transport;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Runtime.Client.Implementation
{
    /// <summary>
    /// 远程调用服务
    /// </summary>
    public class RemoteInvokeService : IRemoteInvokeService
    {
        #region 字段

        /// <summary>
        /// Defines the _addressResolver
        /// </summary>
        private readonly IAddressResolver _addressResolver;

        /// <summary>
        /// Defines the _healthCheckService
        /// </summary>
        private readonly IHealthCheckService _healthCheckService;

        /// <summary>
        /// Defines the _logger
        /// </summary>
        private readonly ILogger<RemoteInvokeService> _logger;

        /// <summary>
        /// Defines the _transportClientFactory
        /// </summary>
        private readonly ITransportClientFactory _transportClientFactory;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteInvokeService"/> class.
        /// </summary>
        /// <param name="hashAlgorithm">The hashAlgorithm<see cref="IHashAlgorithm"/></param>
        /// <param name="addressResolver">The addressResolver<see cref="IAddressResolver"/></param>
        /// <param name="transportClientFactory">The transportClientFactory<see cref="ITransportClientFactory"/></param>
        /// <param name="logger">The logger<see cref="ILogger{RemoteInvokeService}"/></param>
        /// <param name="healthCheckService">The healthCheckService<see cref="IHealthCheckService"/></param>
        public RemoteInvokeService(IHashAlgorithm hashAlgorithm, IAddressResolver addressResolver, ITransportClientFactory transportClientFactory, ILogger<RemoteInvokeService> logger, IHealthCheckService healthCheckService)
        {
            _addressResolver = addressResolver;
            _transportClientFactory = transportClientFactory;
            _logger = logger;
            _healthCheckService = healthCheckService;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The InvokeAsync
        /// </summary>
        /// <param name="context">The context<see cref="RemoteInvokeContext"/></param>
        /// <returns>The <see cref="Task{RemoteInvokeResultMessage}"/></returns>
        public async Task<RemoteInvokeResultMessage> InvokeAsync(RemoteInvokeContext context)
        {
            return await InvokeAsync(context, Task.Factory.CancellationToken);
        }

        /// <summary>
        /// The InvokeAsync
        /// </summary>
        /// <param name="context">The context<see cref="RemoteInvokeContext"/></param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task{RemoteInvokeResultMessage}"/></returns>
        public async Task<RemoteInvokeResultMessage> InvokeAsync(RemoteInvokeContext context, CancellationToken cancellationToken)
        {
            var invokeMessage = context.InvokeMessage;
            AddressModel address = null;
            var vt = ResolverAddress(context, context.Item);
            address = vt.IsCompletedSuccessfully ? vt.Result : await vt;
            try
            {
                var endPoint = address.CreateEndPoint();
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug($"使用地址：'{endPoint}'进行调用。");
                var client = await _transportClientFactory.CreateClientAsync(endPoint);
                return await client.SendAsync(invokeMessage, cancellationToken).WithCancellation(cancellationToken);
            }
            catch (CommunicationException)
            {
                await _healthCheckService.MarkFailure(address);
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"发起请求中发生了错误，服务Id：{invokeMessage.ServiceId}。");
                throw;
            }
        }

        /// <summary>
        /// The InvokeAsync
        /// </summary>
        /// <param name="context">The context<see cref="RemoteInvokeContext"/></param>
        /// <param name="requestTimeout">The requestTimeout<see cref="int"/></param>
        /// <returns>The <see cref="Task{RemoteInvokeResultMessage}"/></returns>
        public async Task<RemoteInvokeResultMessage> InvokeAsync(RemoteInvokeContext context, int requestTimeout)
        {
            var invokeMessage = context.InvokeMessage;
            AddressModel address = null;
            var vt = ResolverAddress(context, context.Item);
            address = vt.IsCompletedSuccessfully ? vt.Result : await vt;
            try
            {
                var endPoint = address.CreateEndPoint();
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug($"使用地址：'{endPoint}'进行调用。");
                var client = await _transportClientFactory.CreateClientAsync(endPoint);
                using (var cts = new CancellationTokenSource())
                {
                    return await client.SendAsync(invokeMessage, cts.Token).WithCancellation(cts, requestTimeout);
                }
            }
            catch (CommunicationException)
            {
                await _healthCheckService.MarkFailure(address);
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"发起请求中发生了错误，服务Id：{invokeMessage.ServiceId}。错误信息：{exception.Message}");
                throw;
            }
        }

        /// <summary>
        /// The ResolverAddress
        /// </summary>
        /// <param name="context">The context<see cref="RemoteInvokeContext"/></param>
        /// <param name="item">The item<see cref="string"/></param>
        /// <returns>The <see cref="ValueTask{AddressModel}"/></returns>
        private async ValueTask<AddressModel> ResolverAddress(RemoteInvokeContext context, string item)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (context.InvokeMessage == null)
                throw new ArgumentNullException(nameof(context.InvokeMessage));

            if (string.IsNullOrEmpty(context.InvokeMessage.ServiceId))
                throw new ArgumentException("服务Id不能为空。", nameof(context.InvokeMessage.ServiceId));
            //远程调用信息
            var invokeMessage = context.InvokeMessage;
            //解析服务地址
            var vt = _addressResolver.Resolver(invokeMessage.ServiceId, item);
            var address = vt.IsCompletedSuccessfully ? vt.Result : await vt;
            if (address == null)
                throw new CPlatformException($"无法解析服务Id：{invokeMessage.ServiceId}的地址信息。");
            return address;
        }

        #endregion 方法
    }
}