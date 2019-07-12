using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Exceptions;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Transport.Implementation
{
    /// <summary>
    /// 一个默认的传输客户端实现。
    /// </summary>
    public class TransportClient : ITransportClient, IDisposable
    {
        #region 字段

        /// <summary>
        /// Defines the _logger
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Defines the _messageListener
        /// </summary>
        private readonly IMessageListener _messageListener;

        /// <summary>
        /// Defines the _messageSender
        /// </summary>
        private readonly IMessageSender _messageSender;

        /// <summary>
        /// Defines the _resultDictionary
        /// </summary>
        private readonly ConcurrentDictionary<string, ManualResetValueTaskSource<TransportMessage>> _resultDictionary =
            new ConcurrentDictionary<string, ManualResetValueTaskSource<TransportMessage>>();

        /// <summary>
        /// Defines the _serviceExecutor
        /// </summary>
        private readonly IServiceExecutor _serviceExecutor;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="TransportClient"/> class.
        /// </summary>
        /// <param name="messageSender">The messageSender<see cref="IMessageSender"/></param>
        /// <param name="messageListener">The messageListener<see cref="IMessageListener"/></param>
        /// <param name="logger">The logger<see cref="ILogger"/></param>
        /// <param name="serviceExecutor">The serviceExecutor<see cref="IServiceExecutor"/></param>
        public TransportClient(IMessageSender messageSender, IMessageListener messageListener, ILogger logger,
            IServiceExecutor serviceExecutor)
        {
            _messageSender = messageSender;
            _messageListener = messageListener;
            _logger = logger;
            _serviceExecutor = serviceExecutor;
            messageListener.Received += MessageListener_Received;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The Dispose
        /// </summary>
        public void Dispose()
        {
            (_messageSender as IDisposable)?.Dispose();
            (_messageListener as IDisposable)?.Dispose();
            foreach (var taskCompletionSource in _resultDictionary.Values)
            {
                taskCompletionSource.SetCanceled();
            }
        }

        /// <summary>
        /// 发送消息。
        /// </summary>
        /// <param name="message">远程调用消息模型。</param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>远程调用消息的传输消息。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<RemoteInvokeResultMessage> SendAsync(RemoteInvokeMessage message, CancellationToken cancellationToken)
        {
            try
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug("准备发送消息。");

                var transportMessage = TransportMessage.CreateInvokeMessage(message);

                //注册结果回调
                var callbackTask = RegisterResultCallbackAsync(transportMessage.Id, cancellationToken);

                try
                {
                    //发送
                    await _messageSender.SendAndFlushAsync(transportMessage);
                }
                catch (Exception exception)
                {
                    throw new CommunicationException("与服务端通讯时发生了异常。", exception);
                }

                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug("消息发送成功。");

                return await callbackTask;
            }
            catch (Exception exception)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError(exception, "消息发送失败。");
                throw;
            }
        }

        /// <summary>
        /// The MessageListener_Received
        /// </summary>
        /// <param name="sender">The sender<see cref="IMessageSender"/></param>
        /// <param name="message">The message<see cref="TransportMessage"/></param>
        /// <returns>The <see cref="Task"/></returns>
        private async Task MessageListener_Received(IMessageSender sender, TransportMessage message)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
                _logger.LogTrace("服务消费者接收到消息。");

            ManualResetValueTaskSource<TransportMessage> task;
            if (!_resultDictionary.TryGetValue(message.Id, out task))
                return;

            if (message.IsInvokeResultMessage())
            {
                var content = message.GetContent<RemoteInvokeResultMessage>();
                if (!string.IsNullOrEmpty(content.ExceptionMessage))
                {
                    task.SetException(new CPlatformCommunicationException(content.ExceptionMessage, content.StatusCode));
                }
                else
                {
                    task.SetResult(message);
                }
            }
            if (_serviceExecutor != null && message.IsInvokeMessage())
                await _serviceExecutor.ExecuteAsync(sender, message);
        }

        /// <summary>
        /// 注册指定消息的回调任务。
        /// </summary>
        /// <param name="id">消息Id。</param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>远程调用结果消息模型。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task<RemoteInvokeResultMessage> RegisterResultCallbackAsync(string id, CancellationToken cancellationToken)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"准备获取Id为：{id}的响应内容。");

            var task = new ManualResetValueTaskSource<TransportMessage>();
            _resultDictionary.TryAdd(id, task);
            try
            {
                var result = await task.AwaitValue(cancellationToken);
                return result.GetContent<RemoteInvokeResultMessage>();
            }
            finally
            {
                //删除回调任务
                ManualResetValueTaskSource<TransportMessage> value;
                _resultDictionary.TryRemove(id, out value);
                value.SetCanceled();
            }
        }

        #endregion 方法
    }
}