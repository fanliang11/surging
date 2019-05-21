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
    public class RemoteInvokeService : IRemoteInvokeService
    {
        private readonly IAddressResolver _addressResolver;
        private readonly ITransportClientFactory _transportClientFactory;
        private readonly ILogger<RemoteInvokeService> _logger;
        private readonly IHealthCheckService _healthCheckService;

        public RemoteInvokeService(IHashAlgorithm hashAlgorithm,IAddressResolver addressResolver, ITransportClientFactory transportClientFactory, ILogger<RemoteInvokeService> logger, IHealthCheckService healthCheckService)
        {
            _addressResolver = addressResolver;
            _transportClientFactory = transportClientFactory;
            _logger = logger;
            _healthCheckService = healthCheckService;
        }

        #region Implementation of IRemoteInvokeService

        public async Task<RemoteInvokeResultMessage> InvokeAsync(RemoteInvokeContext context)
        {
            return await InvokeAsync(context, Task.Factory.CancellationToken);
        }

        public async Task<RemoteInvokeResultMessage> InvokeAsync(RemoteInvokeContext context, CancellationToken cancellationToken)
        {
            var invokeMessage = context.InvokeMessage;
            AddressModel address = null;
            var vt = ResolverAddress(context, context.Item);
            address = vt.IsCompletedSuccessfully? vt.Result: await vt; 
            try
            {
                var endPoint = address.CreateEndPoint();
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug($"使用地址：'{endPoint}'进行调用。");
                var client =await _transportClientFactory.CreateClientAsync(endPoint);
                return await client.SendAsync(invokeMessage,cancellationToken).WithCancellation(cancellationToken);
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
                var client =await _transportClientFactory.CreateClientAsync(endPoint);
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

        private async ValueTask<AddressModel> ResolverAddress(RemoteInvokeContext context,string item)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (context.InvokeMessage == null)
                throw new ArgumentNullException(nameof(context.InvokeMessage));

            if (string.IsNullOrEmpty(context.InvokeMessage.ServiceId))
                throw new ArgumentException("服务Id不能为空。", nameof(context.InvokeMessage.ServiceId));
            var invokeMessage = context.InvokeMessage; 
            var vt =  _addressResolver.Resolver(invokeMessage.ServiceId, item);
            var address = vt.IsCompletedSuccessfully ? vt.Result : await vt;
            if (address == null)
                throw new CPlatformException($"无法解析服务Id：{invokeMessage.ServiceId}的地址信息。");
            return address;
        }

        #endregion Implementation of IRemoteInvokeService
    }
}