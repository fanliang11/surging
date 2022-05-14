using DotNetty.Buffers;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Network;
using Surging.Core.Protocol.Tcp.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Tcp.Adapter
{
    public class ConnectionChannelHandlerAdapter : ChannelHandlerAdapter
	{
		private readonly ILogger _logger;
		private readonly IDeviceProvider _deviceProvider;
		private readonly ITcpServiceEntryProvider _tcpServiceEntryProvider;
		private readonly TcpServerProperties _tcpServerProperties;
		public ConnectionChannelHandlerAdapter(ILogger logger, IDeviceProvider deviceProvider, ITcpServiceEntryProvider tcpServiceEntryProvider, TcpServerProperties tcpServerProperties)
		{
			_logger = logger;
			_deviceProvider = deviceProvider;
			_tcpServiceEntryProvider = tcpServiceEntryProvider;
			_tcpServerProperties= tcpServerProperties;

		}

     
        public override void ChannelActive(IChannelHandlerContext ctx)
		{
			_deviceProvider.Register(ctx);
			var tcpEntry=_tcpServiceEntryProvider.GetEntry();
			tcpEntry.Behavior.DeviceStatusProcess(DeviceStatus.Connected, ctx.Channel.Id.AsLongText(), _tcpServerProperties);
			if (_logger.IsEnabled(LogLevel.Information))
				_logger.LogInformation("channel active:" + ctx.Channel.RemoteAddress);

		}

		public override void ChannelInactive(IChannelHandlerContext ctx)
		{
			_deviceProvider.Unregister(ctx);
			var tcpEntry = _tcpServiceEntryProvider.GetEntry();
			tcpEntry.Behavior.DeviceStatusProcess(DeviceStatus.Closed, ctx.Channel.Id.AsLongText(), _tcpServerProperties);
			if (_logger.IsEnabled(LogLevel.Information))
				_logger.LogInformation("channel inactive:" + ctx.Channel.RemoteAddress);
		
		}


		public override void ExceptionCaught(IChannelHandlerContext ctx, Exception exception)
		{
			_deviceProvider.Unregister(ctx);
			var tcpEntry = _tcpServiceEntryProvider.GetEntry();
			tcpEntry.Behavior.DeviceStatusProcess(DeviceStatus.Abnormal, ctx.Channel.Id.AsLongText(), _tcpServerProperties);
			if (_logger.IsEnabled(LogLevel.Error))
				_logger.LogError("channel exceptionCaught:" + ctx.Channel.RemoteAddress, exception);

		}
	}
}
 