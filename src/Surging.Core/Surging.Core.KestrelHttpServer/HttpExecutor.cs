using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Convertibles;
using Surging.Core.CPlatform.Filters;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Transport;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.ProxyGenerator;
using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Surging.Core.CPlatform.Utilities.FastInvoke;
using System.Diagnostics;
using Surging.Core.CPlatform.Diagnostics;
using Surging.Core.CPlatform.Transport.Implementation;

namespace Surging.Core.KestrelHttpServer
{
    public  class HttpExecutor : IServiceExecutor
    {
        #region Field
        private readonly IServiceEntryLocate _serviceEntryLocate;
        private readonly ILogger<HttpExecutor> _logger;
        private readonly IServiceRouteProvider _serviceRouteProvider;
        private readonly IAuthorizationFilter _authorizationFilter;
        private readonly CPlatformContainer _serviceProvider;
        private readonly ITypeConvertibleService _typeConvertibleService;
        private readonly IServiceProxyProvider _serviceProxyProvider;
        private readonly ConcurrentDictionary<string, ValueTuple<FastInvokeHandler, object, MethodInfo>> _concurrent =
        new ConcurrentDictionary<string, ValueTuple<FastInvokeHandler, object, MethodInfo>>();
        #endregion Field

        #region Constructor

        public HttpExecutor(IServiceEntryLocate serviceEntryLocate, IServiceRouteProvider serviceRouteProvider,
            IAuthorizationFilter authorizationFilter,
            ILogger<HttpExecutor> logger, CPlatformContainer serviceProvider, IServiceProxyProvider serviceProxyProvider, ITypeConvertibleService typeConvertibleService)
        {
            _serviceEntryLocate = serviceEntryLocate;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _typeConvertibleService = typeConvertibleService;
            _serviceRouteProvider = serviceRouteProvider;
            _authorizationFilter = authorizationFilter;
            _serviceProxyProvider = serviceProxyProvider;
        }
        #endregion Constructor

        #region Implementation of IExecutor

        public async Task ExecuteAsync(IMessageSender sender, TransportMessage message)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
                _logger.LogTrace("服务提供者接收到消息。");

            if (!message.IsHttpMessage())
                return;
            HttpMessage httpMessage;
            try
            {
                httpMessage = message.GetContent<HttpMessage>();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "将接收到的消息反序列化成 TransportMessage<httpMessage> 时发送了错误。");
                return;
            }
            if (httpMessage.Attachments != null)
            {
                foreach (var attachment in httpMessage.Attachments)
                    RpcContext.GetContext().SetAttachment(attachment.Key, attachment.Value);
            }
            WirteDiagnosticBefore(message);
            var entry = _serviceEntryLocate.Locate(httpMessage);

            HttpResultMessage<object> httpResultMessage = new HttpResultMessage<object>() { };

            if (entry!=null && _serviceProvider.IsRegisteredWithKey(httpMessage.ServiceKey, entry.Type))
            {
                //执行本地代码。
                httpResultMessage = await LocalExecuteAsync(entry, httpMessage);
            }
            else
            {
                httpResultMessage = await RemoteExecuteAsync(httpMessage);
            }
            await SendRemoteInvokeResult(sender,message.Id, httpResultMessage);
        }
        

        #endregion Implementation of IServiceExecutor

        #region Private Method

        private async Task<HttpResultMessage<object>> RemoteExecuteAsync(HttpMessage httpMessage)
        {
            HttpResultMessage<object> resultMessage = new HttpResultMessage<object>();
            try {
                resultMessage.Entity=await _serviceProxyProvider.Invoke<object>(httpMessage.Parameters, httpMessage.RoutePath, httpMessage.ServiceKey);
                resultMessage.IsSucceed = resultMessage.Entity != default;
                resultMessage.StatusCode = resultMessage.IsSucceed ? (int)StatusCode.Success : (int)StatusCode.RequestError;
            }
            catch (Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError(ex, "执行远程调用逻辑时候发生了错误。");
                resultMessage = new HttpResultMessage<object> { Entity = null, Message = "执行发生了错误。", StatusCode = (int)StatusCode.RequestError };
            }
            return resultMessage;
        }

        private async Task<HttpResultMessage<object>> LocalExecuteAsync(ServiceEntry entry, HttpMessage httpMessage)
        {
            HttpResultMessage<object> resultMessage = new HttpResultMessage<object>();
            try
            {
                var result = await entry.Func(httpMessage.ServiceKey, httpMessage.Parameters);
                var task = result as Task;

                if (task == null)
                {
                    resultMessage.Entity = result;
                }
                else
                {
                    task.Wait();
                    var taskType = task.GetType().GetTypeInfo();
                    if (taskType.IsGenericType)
                        resultMessage.Entity = taskType.GetProperty("Result").GetValue(task);
                }
                resultMessage.IsSucceed = resultMessage.Entity != null;
                resultMessage.StatusCode = resultMessage.IsSucceed ? (int)StatusCode.Success : (int)StatusCode.RequestError;
            }
            catch (Exception exception)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError(exception, "执行本地逻辑时候发生了错误。");
                resultMessage.Message = "执行发生了错误。";
                resultMessage.StatusCode = exception.HResult;
            }
            return resultMessage;
        }

        private async Task SendRemoteInvokeResult(IMessageSender sender,string messageId, HttpResultMessage resultMessage)
        {
            try
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug("准备发送响应消息。");

                await sender.SendAndFlushAsync(new TransportMessage(messageId,resultMessage));
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug("响应消息发送成功。");
            }
            catch (Exception exception)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError(exception, "发送响应消息时候发生了异常。");
            }
        }

        private static string GetExceptionMessage(Exception exception)
        {
            if (exception == null)
                return string.Empty;

            var message = exception.Message;
            if (exception.InnerException != null)
            {
                message += "|InnerException:" + GetExceptionMessage(exception.InnerException);
            }
            return message;
        }

        private void WirteDiagnosticBefore(TransportMessage message)
        {
            if (!AppConfig.ServerOptions.DisableDiagnostic)
            {
                RpcContext.GetContext().SetAttachment("TraceId", message.Id);
                var diagnosticListener = new DiagnosticListener(DiagnosticListenerExtensions.DiagnosticListenerName);
                var remoteInvokeMessage = message.GetContent<HttpMessage>();
                diagnosticListener.WriteTransportBefore(TransportType.Rest, new TransportEventData(new DiagnosticMessage
                {
                    Content = message.Content,
                    ContentType = message.ContentType,
                    Id = message.Id,
                    MessageName = remoteInvokeMessage.RoutePath
                }, TransportType.Rest.ToString(),
               message.Id,
                RpcContext.GetContext().GetAttachment("RemoteIpAddress")?.ToString()));
            }
            else
            {
                var parameters = RpcContext.GetContext().GetContextParameters();
                parameters.TryRemove("RemoteIpAddress", out object value);
                RpcContext.GetContext().SetContextParameters(parameters);
            }

        }

        #endregion Private Method

    }
}

