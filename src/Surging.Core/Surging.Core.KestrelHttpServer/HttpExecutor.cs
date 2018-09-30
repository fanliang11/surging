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
        private readonly ConcurrentDictionary<string, ValueTuple<FastInvokeHandler, object, MethodInfo>> _concurrent =
        new ConcurrentDictionary<string, ValueTuple<FastInvokeHandler, object, MethodInfo>>();
        #endregion Field

        #region Constructor

        public HttpExecutor(IServiceEntryLocate serviceEntryLocate, IServiceRouteProvider serviceRouteProvider,
            IAuthorizationFilter authorizationFilter,
            ILogger<HttpExecutor> logger, CPlatformContainer serviceProvider, ITypeConvertibleService typeConvertibleService)
        {
            _serviceEntryLocate = serviceEntryLocate;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _typeConvertibleService = typeConvertibleService;
            _serviceRouteProvider = serviceRouteProvider;
            _authorizationFilter = authorizationFilter;
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
            var entry = _serviceEntryLocate.Locate(httpMessage);
            if (entry == null)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError($"根据服务routePath：{httpMessage.RoutePath}，找不到服务条目。");
                return;
            }
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("准备执行本地逻辑。");
            HttpResultMessage<object> httpResultMessage = new HttpResultMessage<object>() { };

            if (_serviceProvider.IsRegisteredWithKey(httpMessage.ServiceKey, entry.Type))
            {
                //执行本地代码。
                httpResultMessage = await LocalExecuteAsync(entry, httpMessage);
            }
            else
            {
                httpResultMessage = await RemoteExecuteAsync(entry, httpMessage);
            }
            await SendRemoteInvokeResult(sender, httpResultMessage);
        }
        

        #endregion Implementation of IServiceExecutor

        #region Private Method

        private async Task<HttpResultMessage<object>> RemoteExecuteAsync(ServiceEntry entry, HttpMessage httpMessage)
        {
            HttpResultMessage<object> resultMessage = new HttpResultMessage<object>();
            var provider = _concurrent.GetValueOrDefault(httpMessage.RoutePath);
            var list = new List<object>();
            if (provider.Item1 == null)
            {
                provider.Item2 = ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy(httpMessage.ServiceKey, entry.Type);
                provider.Item3 = provider.Item2.GetType().GetTypeInfo().DeclaredMethods.Where(p => p.Name == entry.MethodName).FirstOrDefault(); 
                provider.Item1 = FastInvoke.GetMethodInvoker(provider.Item3);
                _concurrent.GetOrAdd(httpMessage.RoutePath, ValueTuple.Create<FastInvokeHandler, object, MethodInfo>(provider.Item1, provider.Item2, provider.Item3));
            }
            foreach (var parameterInfo in provider.Item3.GetParameters())
            {
                var value = httpMessage.Parameters[parameterInfo.Name];
                var parameterType = parameterInfo.ParameterType;
                var parameter = _typeConvertibleService.Convert(value, parameterType);
                list.Add(parameter);
            }
            try
            {
                var methodResult = provider.Item1(provider.Item2, list.ToArray());

                var task = methodResult as Task;
                if (task == null)
                {
                    resultMessage.Entity = methodResult;
                }
                else
                {
                    await task;
                    var taskType = task.GetType().GetTypeInfo();
                    if (taskType.IsGenericType)
                        resultMessage.Entity = taskType.GetProperty("Result").GetValue(task);
                }
                resultMessage.IsSucceed = resultMessage.Entity != null;
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

        private async Task SendRemoteInvokeResult(IMessageSender sender, HttpResultMessage resultMessage)
        {
            try
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug("准备发送响应消息。");

                await sender.SendAndFlushAsync(new TransportMessage(resultMessage));
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

        #endregion Private Method

    }
}

