using Surging.Core.CPlatform.Utilities;
using Surging.Core.DeviceGateway.Runtime.Device;
using Surging.Core.DeviceGateway.Runtime.Device.Message.Function;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Transport;
using Surging.Core.CPlatform.Exceptions;
using Surging.Core.CPlatform.Runtime.Server;

namespace Surging.Core.DeviceGateway.Runtime.Core.Implementation
{
    public class DeviceInvokeProvider : IDeviceInvokeProvider
    {
        private readonly IDeviceMessageSender _messageSender;
        private readonly IDeviceMessageListener _messageListener;
        private readonly ILogger _logger;

        private readonly ConcurrentDictionary<string, ManualResetValueTaskSource<IDeviceMessage>> _resultDictionary =
            new ConcurrentDictionary<string, ManualResetValueTaskSource<IDeviceMessage>>();

        public DeviceInvokeProvider(IDeviceMessageSender messageSender, IDeviceMessageListener messageListener, ILogger logger)
        {
            _messageSender = messageSender;
            _messageListener = messageListener;
            _logger = logger;
            messageListener.Received += MessageListener_Received;
        }

        public async Task<IDeviceMessage> Invoke(string  messageId, object deviceMessage, CancellationToken cancellationToken)
        {
            try
            {
                var callbackTask = RegisterResultCallbackAsync(messageId, cancellationToken);
                await InvokeAsync(deviceMessage);

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

        public async Task InvokeAsync(object deviceMessage)
        {
            try
            {
                if (_messageSender != null)
                    //发送
                    await _messageSender.SendAndFlushAsync(deviceMessage);
            }
            catch (Exception exception)
            {
                throw new CommunicationException("与服务端通讯时发生了异常。", exception);
            }

            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("消息发送成功。");
             
        } 

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task<IDeviceMessage> RegisterResultCallbackAsync(string id, CancellationToken cancellationToken)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"准备获取Id为：{id}的响应内容。");

            var task = new ManualResetValueTaskSource<IDeviceMessage>();
            _resultDictionary.TryAdd(id, task);
            try
            {
                var result = await task.AwaitValue(cancellationToken);
                return result;
            }
            finally
            {
                //删除回调任务
                ManualResetValueTaskSource<IDeviceMessage> value;
                _resultDictionary.TryRemove(id, out value);
                value.SetCanceled();
            }
        }

        private async Task MessageListener_Received(IDeviceMessageSender sender, IDeviceMessage message)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
                _logger.LogTrace("设备回复接收到消息。");

            ManualResetValueTaskSource<IDeviceMessage> task;
            if (!_resultDictionary.TryGetValue(message.MessageId, out task))
                return;

            task.SetResult(message);
        }
    }
}
