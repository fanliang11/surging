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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static Surging.Core.CPlatform.Utilities.FastInvoke;

namespace Surging.Core.Protocol.Http
{
    public class HttpServiceExecutor : IServiceExecutor
    {
        #region Field

        private readonly IServiceEntryLocate _serviceEntryLocate;
        private readonly ILogger<HttpServiceExecutor> _logger;
        private readonly IServiceRouteProvider _serviceRouteProvider;
        private readonly IAuthorizationFilter _authorizationFilter;
        private readonly CPlatformContainer _serviceProvider;
        private readonly ITypeConvertibleService _typeConvertibleService;
        private readonly ConcurrentDictionary<string,ValueTuple< FastInvokeHandler,object, MethodInfo>> _concurrent =
 new ConcurrentDictionary<string, ValueTuple<FastInvokeHandler, object, MethodInfo>>();
        #endregion Field

        #region Constructor

        public HttpServiceExecutor(IServiceEntryLocate serviceEntryLocate, IServiceRouteProvider serviceRouteProvider,
            IAuthorizationFilter authorizationFilter,
            ILogger<HttpServiceExecutor> logger, CPlatformContainer serviceProvider, ITypeConvertibleService typeConvertibleService)
        {
            _serviceEntryLocate = serviceEntryLocate;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _typeConvertibleService = typeConvertibleService;
            _serviceRouteProvider = serviceRouteProvider;
            _authorizationFilter = authorizationFilter;
        }

        #endregion Constructor

        #region Implementation of IServiceExecutor

        /// <summary>
        /// 执行。
        /// </summary>
        /// <param name="sender">消息发送者。</param>
        /// <param name="message">调用消息。</param>
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
            if (_serviceProvider.IsRegisteredWithKey(httpMessage.ServiceKey, entry.Type))
            {
                HttpResultMessage<object> httpResultMessage = new HttpResultMessage<object>();
                //执行本地代码。
                await LocalExecuteAsync(entry, httpMessage, httpResultMessage);
                //向客户端发送调用结果。
                await SendRemoteInvokeResult(sender, httpResultMessage);
            }
            else
            {
                HttpResultMessage<object> httpResultMessage = new HttpResultMessage<object>();
                await RemoteExecuteAsync(entry, httpMessage, httpResultMessage);
                await SendRemoteInvokeResult(sender, httpResultMessage);
            }
        }

        #endregion Implementation of IServiceExecutor

        #region Private Method

        private async Task RemoteExecuteAsync(ServiceEntry entry, HttpMessage httpMessage, HttpResultMessage<object> resultMessage)
        {
            var provider = _concurrent.GetValueOrDefault(httpMessage.RoutePath);
            var list = new List<object>();
            if (provider.Item1 == null)
            {
                provider.Item2 = ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy(httpMessage.ServiceKey, entry.Type);
                provider.Item3 = provider.Item2.GetType().GetTypeInfo().DeclaredMethods.Where(p => p.Name == entry.MethodName).FirstOrDefault(); ;
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
            var methodResult = provider.Item1(provider.Item2, list.ToArray());

            var task = methodResult as Task;
            if (task == null)
            {
                resultMessage.Entity = methodResult;
                resultMessage.IsSucceed = resultMessage.Entity != null;
            }
            else
            {
                await task;
                var taskType = task.GetType().GetTypeInfo();
                if (taskType.IsGenericType)
                    resultMessage.Entity = taskType.GetProperty("Result").GetValue(task);
                resultMessage.IsSucceed = resultMessage.Entity != null;
            }
        }

        private async Task LocalExecuteAsync(ServiceEntry entry, HttpMessage httpMessage, HttpResultMessage<object> resultMessage)
        {
            try
            {
                var result = await entry.Func(httpMessage.ServiceKey, httpMessage.Parameters);
                var task = result as Task;

                if (task == null)
                {
                    resultMessage.Entity = result;
                    resultMessage.IsSucceed = resultMessage.Entity != null;
                }
                else
                {
                    task.Wait();
                    var taskType = task.GetType().GetTypeInfo();
                    if (taskType.IsGenericType)
                        resultMessage.Entity = taskType.GetProperty("Result").GetValue(task);
                    resultMessage.IsSucceed = resultMessage.Entity != null;
                }

            }
            catch (Exception exception)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError(exception, "执行本地逻辑时候发生了错误。");
                resultMessage.StatusCode = exception.HResult;
            }
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
