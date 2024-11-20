using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Transport;
using Surging.Core.Protocol.Tcp.Extensions;
using Surging.Core.Protocol.Tcp.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Tcp
{
    public class TcpServiceExecutor : IServiceExecutor
    {
        #region Field

        private readonly ITcpServiceEntryProvider _tcpServiceEntryProvider;
        private readonly ILogger<TcpServiceExecutor> _logger;

        #endregion Field

        #region Constructor

        public TcpServiceExecutor(ITcpServiceEntryProvider dnsServiceEntryProvider,
            ILogger<TcpServiceExecutor> logger)
        {
            _tcpServiceEntryProvider = dnsServiceEntryProvider;
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


            byte[] tcpMessage = null;
            try
            {
                if (message.IsTcpDispatchMessage())
                    tcpMessage = message.GetContent<byte[]>();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "将接收到的消息反序列化成 TransportMessage<byte[]> 时发送了错误。");
                return;
            }
            var entry = _tcpServiceEntryProvider.GetEntry();
            if (entry == null)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError($"未实现tcpBehavior实例。");
                return;
            }
            if (tcpMessage != null)
                await LocalExecuteAsync(entry, tcpMessage);

            await SendRemoteInvokeResult(sender, tcpMessage);
        }

        #endregion Implementation of IServiceExecutor

        #region Private Method


        private async Task LocalExecuteAsync(TcpServiceEntry entry, byte[] bytes)
        {
            HttpResultMessage<object> resultMessage = new HttpResultMessage<object>();
            try
            {
               // await entry.Behavior.Dispatch(bytes);
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
