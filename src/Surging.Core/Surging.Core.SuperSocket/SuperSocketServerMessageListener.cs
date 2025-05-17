using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SuperSocket;
using SuperSocket.Server;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Host;
using Surging.Core.CPlatform.Engines;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Transport;
using Surging.Core.CPlatform.Transport.Codec;
using Surging.Core.SuperSocket.Adapter;
using System.Net;

namespace Surging.Core.SuperSocket
{
    public class SuperSocketServerMessageListener : IMessageListener, IDisposable
    {
        public event ReceivedDelegate Received;

        private readonly ILogger<SuperSocketServerMessageListener> _logger;

        private readonly ITransportMessageDecoder _transportMessageDecoder;
        private readonly ITransportMessageEncoder _transportMessageEncoder;
        private readonly IServiceEngineLifetime _serviceEngineLifetime;

        public SuperSocketServerMessageListener(ILogger<SuperSocketServerMessageListener> logger, ITransportMessageCodecFactory codecFactory, IServiceEngineLifetime serviceEngineLifetime)
        {
            _logger = logger;
            _transportMessageEncoder = codecFactory.GetEncoder();
            _transportMessageDecoder = codecFactory.GetDecoder();
            _serviceEngineLifetime = serviceEngineLifetime;
        }
        public async Task StartAsync(EndPoint endPoint)
        {

            _serviceEngineLifetime.ServiceEngineStarted.Register(async () =>
            {
                try
                {
                    var ipEndPoint = endPoint as IPEndPoint;
                    var host = SuperSocketHostBuilder.Create<TransportMessage, TransportMessagePipelineFilter>()
                
                    .UsePackageHandler( (s, p) =>
                    {
                        Task.Run(async () =>
                        {
                            var sender = new SuperSocketServerMessageSender(_transportMessageEncoder, s);
                            await OnReceived(sender, p);
                        });
                        return ValueTask.CompletedTask;
                    })
                    .ConfigureSuperSocket(options =>
                    {
                        options.Name = "Echo Server";
                        options.Logger = _logger; 
                        options.AddListener(new ListenOptions
                        {
                            Ip = ipEndPoint.Address.ToString(),
                            Port = ipEndPoint.Port,
                          
                        }
                        );
                    })
                    .ConfigureLogging((hostCtx, loggingBuilder) =>
                    {
                        loggingBuilder.AddConsole();
                    })
                    .Build();
                    await host.RunAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"SuperSocket服务主机启动失败，监听地址：{endPoint}。 ");
                }
            });

        }
         
        public async Task OnReceived(IMessageSender sender, TransportMessage message)
        {
            if (Received == null)
                return;
            await Received(sender, message);
        }

        public void Dispose()
        { 
        }
    }
}
