using DotNetty.Codecs.Http;
using DotNetty.Common.Concurrency;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Routing.Template;
using Surging.Core.CPlatform.Serialization;
using Surging.Core.CPlatform.Transport;
using Surging.Core.CPlatform.Transport.Codec;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Surging.Core.Protocol.Http
{
    class DotNettyHttpServerMessageListener : IMessageListener, IDisposable
    {
        #region Field

        private readonly ILogger<DotNettyHttpServerMessageListener> _logger;
        private readonly ITransportMessageDecoder _transportMessageDecoder;
        private readonly ITransportMessageEncoder _transportMessageEncoder;
        private IChannel _channel;
        private readonly ISerializer<string> _serializer;
        private readonly IServiceRouteProvider _serviceRouteProvider;

        #endregion Field

        #region Constructor

        public DotNettyHttpServerMessageListener(ILogger<DotNettyHttpServerMessageListener> logger,
            ITransportMessageCodecFactory codecFactory, 
            ISerializer<string> serializer, 
            IServiceRouteProvider serviceRouteProvider)
        {
            _logger = logger;
            _transportMessageEncoder = codecFactory.GetEncoder();
            _transportMessageDecoder = codecFactory.GetDecoder();
            _serializer = serializer;
            _serviceRouteProvider = serviceRouteProvider;
        }

        #endregion Constructor

        #region Implementation of IMessageListener

        public event ReceivedDelegate Received;

        /// <summary>
        /// 触发接收到消息事件。
        /// </summary>
        /// <param name="sender">消息发送者。</param>
        /// <param name="message">接收到的消息。</param>
        /// <returns>一个任务。</returns>
        public async Task OnReceived(IMessageSender sender, TransportMessage message)
        {
            if (Received == null)
                return;
            await Received(sender, message);
        }

        #endregion Implementation of IMessageListener

        public async Task StartAsync(EndPoint endPoint)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"准备启动服务主机，监听地址：{endPoint}。");
            var serverCompletion = new TaskCompletionSource();
            var bossGroup = new MultithreadEventLoopGroup(1);
            var workerGroup = new MultithreadEventLoopGroup();//Default eventLoopCount is Environment.ProcessorCount * 2
            var bootstrap = new ServerBootstrap();
            bootstrap
            .Group(bossGroup, workerGroup)
            .Channel<TcpServerSocketChannel>()
            .Option(ChannelOption.SoReuseport, true)
            .ChildOption(ChannelOption.SoReuseaddr, true)
            .Option(ChannelOption.SoBacklog, AppConfig.ServerOptions.SoBacklog)
            .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
            {
                IChannelPipeline pipeline = channel.Pipeline;
                pipeline.AddLast("encoder", new HttpResponseEncoder());
                pipeline.AddLast(new HttpRequestDecoder(int.MaxValue, 8192, 8192, true));
                pipeline.AddLast(new HttpObjectAggregator(int.MaxValue));
                pipeline.AddLast(new ServerHandler(async (contenxt, message) =>
                {
                    var sender = new DotNettyHttpServerMessageSender(_transportMessageEncoder, contenxt, _serializer);
                    await OnReceived(sender, message);
                }, _logger, _serializer, _serviceRouteProvider));
                serverCompletion.TryComplete();
            }));
            try
            {
                _channel = await bootstrap.BindAsync(endPoint);
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug($"Http服务主机启动成功，监听地址：{endPoint}。");
            }
            catch
            {
                _logger.LogError($"Http服务主机启动失败，监听地址：{endPoint}。 ");
            }

        }

        public void CloseAsync()
        {
            Task.Run(async () =>
            {
                await _channel.EventLoop.ShutdownGracefullyAsync();
                await _channel.CloseAsync();
            }).Wait();
        }

        #region Implementation of IDisposable

        
        public void Dispose()
        {
            Task.Run(async () =>
            {
                await _channel.DisconnectAsync();
            }).Wait();
        }

        #endregion Implementation of IDisposable

        #region Help Class
        private class ServerHandler : SimpleChannelInboundHandler<IFullHttpRequest>
        {
            readonly TaskCompletionSource completion = new TaskCompletionSource();

            private readonly Action<IChannelHandlerContext, TransportMessage> _readAction;
            private readonly ILogger _logger;
            private readonly ISerializer<string> _serializer;
            private readonly IServiceRouteProvider _serviceRouteProvider;

            public ServerHandler(Action<IChannelHandlerContext, TransportMessage> readAction, 
                ILogger logger, 
                ISerializer<string> serializer,
                IServiceRouteProvider serviceRouteProvider)
            {
                _readAction = readAction;
                _logger = logger;
                _serializer = serializer;
                _serviceRouteProvider = serviceRouteProvider;
            }

            public bool WaitForCompletion()
            {
                this.completion.Task.Wait(TimeSpan.FromSeconds(5));
                return this.completion.Task.Status == TaskStatus.RanToCompletion;
            }

            protected override void ChannelRead0(IChannelHandlerContext ctx, IFullHttpRequest msg)
            {
                var data = new byte[msg.Content.ReadableBytes];
                msg.Content.ReadBytes(data);

                Task.Run(async () =>
                {
                    var parameters = GetParameters(HttpUtility.UrlDecode(msg.Uri), out string path);
                    var serviceRoute = await _serviceRouteProvider.GetRouteByPathRegex(path);
                    parameters.Remove("servicekey", out object serviceKey);
                    if (data.Length > 0)
                        parameters = _serializer.Deserialize<string, IDictionary<string, object>>(System.Text.Encoding.ASCII.GetString(data)) ?? new Dictionary<string, object>();
                    if (String.Compare(serviceRoute.ServiceDescriptor.RoutePath, path, true) != 0)
                    {
                        var @params = RouteTemplateSegmenter.Segment(serviceRoute.ServiceDescriptor.RoutePath, path);
                        foreach (var param in @params)
                        {
                            parameters.Add(param.Key,param.Value);
                        }
                    }
                    if (msg.Method.Name == "POST")
                    {
                        _readAction(ctx, new TransportMessage(new HttpMessage
                        {
                            Parameters = parameters,
                            RoutePath = serviceRoute.ServiceDescriptor.RoutePath,
                            ServiceKey = serviceKey?.ToString()
                        }));
                    }
                    else
                    {
                        _readAction(ctx, new TransportMessage(new HttpMessage
                        {
                            Parameters = parameters,
                            RoutePath = serviceRoute.ServiceDescriptor.RoutePath,
                            ServiceKey = serviceKey?.ToString()
                        }));
                    }
                });
            }

            public IDictionary<string, object> GetParameters(string msg, out string routePath)
            {
                var urlSpan = msg.AsSpan();
                var len = urlSpan.IndexOf("?");
                if (len == -1)
                {
                    routePath = urlSpan.TrimStart("/").ToString().ToLower();
                    return new  Dictionary<string, object>();
                }
                routePath = urlSpan.Slice(0, len).TrimStart("/").ToString().ToLower();
                var paramStr = urlSpan.Slice(len + 1).ToString();
                var parameters = paramStr.Split('&');
                return parameters.ToList().Select(p => p.Split("=")).ToDictionary(p => p[0].ToLower(), p => (object)p[1]);
            }

            public override void ExceptionCaught(IChannelHandlerContext context, Exception exception) => this.completion.TrySetException(exception);
        }

        #endregion Help Class
    }
}
