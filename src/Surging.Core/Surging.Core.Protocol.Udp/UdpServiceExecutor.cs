using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Transport;
using Surging.Core.Protocol.Udp.Extensions;
using Surging.Core.Protocol.Udp.Runtime;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Udp
{
   public class UdpServiceExecutor : IServiceExecutor
    {
        #region Field

        private readonly IUdpServiceEntryProvider _udpServiceEntryProvider;
        private readonly ILogger<UdpServiceExecutor> _logger;

        #endregion Field

        #region Constructor

        public UdpServiceExecutor(IUdpServiceEntryProvider dnsServiceEntryProvider,
            ILogger<UdpServiceExecutor> logger)
        {
            _udpServiceEntryProvider = dnsServiceEntryProvider;
            _logger = logger;
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


            byte[] udpMessage = null;
            try
            {
                if (message.IsUdpDispatchMessage())
                    udpMessage = message.GetContent<byte[]>();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "将接收到的消息反序列化成 TransportMessage<byte[]> 时发送了错误。");
                return;
            }
            var entry = _udpServiceEntryProvider.GetEntry();
            if (entry == null)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError($"未实现UdpBehavior实例。");
                return;
            }
            if (udpMessage != null)
                await LocalExecuteAsync(entry, udpMessage);
           
            await SendRemoteInvokeResult(sender, udpMessage);
        }

        #endregion Implementation of IServiceExecutor

        #region Private Method


        private async Task LocalExecuteAsync(UdpServiceEntry entry, byte [] bytes)
        {
            HttpResultMessage<object> resultMessage = new HttpResultMessage<object>();
            try
            { 
                 await entry.Behavior.Dispatch(bytes);
            }
            catch (Exception exception)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError(exception, "执行本地逻辑时候发生了错误。");
            } 
        }

        private async Task SendRemoteInvokeResult(IMessageSender sender, byte[] resultMessage)
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

        #endregion Private Method
    }
}
