using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Microsoft.Extensions.Logging;
using RulesEngine.ExpressionBuilders;
using RulesEngine.Models;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Network;
using Surging.Core.CPlatform.Transport;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.Protocol.Tcp.Adapter;
using Surging.Core.Protocol.Tcp.Codecs;
using Surging.Core.Protocol.Tcp.RuleParser.Implementation;
using Surging.Core.Protocol.Tcp.Runtime;
using Surging.Core.Protocol.Tcp.Util;
using System.Net;
using System.Text;
using System.Linq.Expressions;
using DotNetty.Common.Utilities;

namespace Surging.Core.Protocol.Tcp
{
    public class DotNettyTcpServerMessageListener : IMessageListener, INetwork, IDisposable
    {

        #region Field
        public event ReceivedDelegate Received;
        private readonly TcpServerProperties _tcpServerProperties;
        private readonly ILogger _logger;
        private IChannel _channel;
        private readonly TcpRuleWorkflow _ruleWorkflow;
        private readonly RulesEngine.RulesEngine _engine;
        public string Id { get; set; }
        #endregion Field

        #region Constructor
        public DotNettyTcpServerMessageListener(ILogger logger, string id, TcpServerProperties properties)
        {
            _logger = logger; 
            Id = id;
            _tcpServerProperties= properties;
            _ruleWorkflow = GetTcpRuleWorkflow();
            _engine = GetRuleEngine(); 
        }
        #endregion

        public async Task OnReceived(IMessageSender sender, TransportMessage message)
        {
            if (Received == null)
                return;
            await Received(sender, message);
        }

        public void Dispose()
        {
            Task.Run(async () =>
            {
                await _channel.EventLoop.ShutdownGracefullyAsync();
                await _channel.CloseAsync();
            }).Wait();
        }



        public bool IsAlive()
        {
            return _channel.Active;
        }

        public bool IsAutoReload()
        {
            return false;
        }



        public void Shutdown()
        {
            Task.Run(async () =>
            {
                await _channel.EventLoop.ShutdownGracefullyAsync();
                await _channel.CloseAsync();
            }).Wait();
        }

        NetworkType INetwork.GetType()
        {
            return NetworkType.TcpServer;
        }

        public async Task StartAsync(EndPoint endPoint)
        {
            var ipEndPoint = (IPEndPoint)endPoint;
            _tcpServerProperties.Host = ipEndPoint.Address.ToString();
            _tcpServerProperties.Port= ipEndPoint.Port;
            await StartAsync();
        }

        public async Task StartAsync()
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"准备启动服务主机，监听地址：{_tcpServerProperties.Host}:{_tcpServerProperties.Port}。");

            IEventLoopGroup bossGroup = new MultithreadEventLoopGroup(1);
            IEventLoopGroup workerGroup = new MultithreadEventLoopGroup();//Default eventLoopCount is Environment.ProcessorCount * 2
            var tcpServiceEntryProvider = ServiceLocator.GetService<ITcpServiceEntryProvider>();
            var workerGroup1 = new SingleThreadEventLoop();

            var bootstrap = new ServerBootstrap();
            bootstrap
            .Channel<TcpServerSocketChannel>()
            .ChildOption(ChannelOption.SoKeepalive, true)
            .Option(ChannelOption.SoBacklog, AppConfig.ServerOptions.SoBacklog)
            .ChildOption(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
            .Group(bossGroup, workerGroup)
        .ChildHandler(new ActionChannelInitializer<IChannel>( channel =>
        {
            var pipeline = channel.Pipeline;
           
            pipeline.AddLast(new ConnectionChannelHandlerAdapter(_logger, ServiceLocator.GetService<IDeviceProvider>(), tcpServiceEntryProvider, _tcpServerProperties));
             pipeline.AddLast(workerGroup1, "ServerHandler", new ServerHandler(_tcpServerProperties,_engine,_ruleWorkflow, _logger)
            );  
            switch (_tcpServerProperties.ParserType)
            {
                case PayloadParserType.Direct:
                    {
                        pipeline.AddLast(new LengthFieldPrepender(4));
                        pipeline.AddLast(new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 4, 0, 4));
                    }
                    break;
                case PayloadParserType.FixedLength:
                    {
                        if (_tcpServerProperties.ParserConfiguration != null && _tcpServerProperties.ParserConfiguration.ContainsKey("size"))
                        {
                            var configValue = _tcpServerProperties.ParserConfiguration["size"];
                            pipeline.AddLast(new FixedLengthFrameDecoder(int.Parse(configValue.ToString() ?? "0")));
                        }
                    }
                    break;
                case PayloadParserType.Delimited:
                    {
                        if (_tcpServerProperties.ParserConfiguration != null && _tcpServerProperties.ParserConfiguration.ContainsKey("delimited"))
                        {
                            var configValue = _tcpServerProperties.ParserConfiguration["delimited"];
                            var delimiter = Unpooled.CopiedBuffer(Encoding.Default.GetBytes(configValue.ToString() ?? ""));
                            pipeline.AddLast(new DelimiterBasedFrameDecoder(int.MaxValue, true, delimiter));
                        }
                    }
                    break;
            }
        
        }))
             .Option(ChannelOption.SoBroadcast, true);
            try
            {
                _channel = await bootstrap.BindAsync(_tcpServerProperties.CreateSocketAddress());
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug($"Tcp服务主机启动成功，监听地址：{_tcpServerProperties.Host}:{_tcpServerProperties.Port}。");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Tcp服务主机启动失败，监听地址：{_tcpServerProperties.Host}:{_tcpServerProperties.Port}。 ");
            }
        }

        private RulesEngine.RulesEngine GetRuleEngine()
        {
            var reSettingsWithCustomTypes = new ReSettings { CustomTypes = new Type[] { typeof(RulePipePayloadParser) } };
            var result = new RulesEngine.RulesEngine(new Workflow[] { _ruleWorkflow.GetWorkflow() }, null, reSettingsWithCustomTypes);
            return result;
        }

        private TcpRuleWorkflow GetTcpRuleWorkflow()
        {
            var result = new TcpRuleWorkflow("1==1");
            if (_tcpServerProperties.ParserConfiguration!=null && _tcpServerProperties.ParserConfiguration.ContainsKey("script"))
            {
                var configValue = _tcpServerProperties.ParserConfiguration["script"];
                result = new TcpRuleWorkflow(configValue.ToString() ?? "");
            }
            return result;
        }

        private class ServerHandler : ChannelHandlerAdapter
        {

            private readonly TcpServerProperties _tcpServerProperties; 
            private readonly ILogger _logger;
            private readonly RulesEngine.RulesEngine _engine;
            private readonly TcpRuleWorkflow _ruleWorkflow;


            public ServerHandler(TcpServerProperties tcpServerProperties, RulesEngine.RulesEngine engine, TcpRuleWorkflow ruleWorkflow,ILogger logger)
            {
                _tcpServerProperties = tcpServerProperties;
                _engine = engine;
                _ruleWorkflow = ruleWorkflow;
                _logger = logger;
            }

            public override async void ChannelRead(IChannelHandlerContext ctx, object message)
            {
                var buffer = (IByteBuffer)message;    
                var parser = await GetParser();
                var tcpServiceEntryProvider = ServiceLocator.GetService<ITcpServiceEntryProvider>();
                var entry = tcpServiceEntryProvider.GetEntry();
                if (entry != null) { entry.Behavior.Parser = parser;
                    entry.Behavior.Sender = new TcpServerMessageSender(ctx);
                    entry.Behavior.Load(ctx.Channel.Id.AsLongText() ,_tcpServerProperties); }
                if (_tcpServerProperties.ParserType == PayloadParserType.Script)
                {
                    if (_tcpServerProperties.ParserConfiguration != null && _tcpServerProperties.ParserConfiguration.ContainsKey("script"))
                    {
                        parser.Build(buffer);
                    }
                }
                else
                {
                    parser.Direct(buffer => buffer).Fixed(buffer.ReadableBytes).Handle(buffer);
                }
                ReferenceCountUtil.Release(buffer);
               
            }

            public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
            {
                context.CloseAsync();
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError(exception, $"与服务器：{context.Channel.RemoteAddress}通信时发送了错误。");
            }
  

            private async Task<RulePipePayloadParser> GetParser()
            {
                var payloadParser = new RulePipePayloadParser();
                var ruleResult = await _engine.ExecuteActionWorkflowAsync(_ruleWorkflow.WorkflowName, _ruleWorkflow.RuleName, new RuleParameter[] { new RuleParameter("parser", payloadParser) });
                return payloadParser;
            }
        }
    }
}
