using Grpc.Core;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Network;
using Surging.Core.CPlatform.Transport;
using Surging.Core.Grpc.Runtime;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Grpc
{
   public  class GrpcServerMessageListener: IMessageListener, INetwork, IDisposable
    { 
        private Server _server;
        private readonly ILogger<GrpcServerMessageListener> _logger;
        private readonly IGrpcServiceEntryProvider _grpcServiceEntryProvider;
        private readonly NetworkProperties _networkProperties;

        public GrpcServerMessageListener(ILogger<GrpcServerMessageListener> logger,
            IGrpcServiceEntryProvider grpcServiceEntryProvider):this(logger, grpcServiceEntryProvider, new NetworkProperties())
        {
        }

        public GrpcServerMessageListener(ILogger<GrpcServerMessageListener> logger,
        IGrpcServiceEntryProvider grpcServiceEntryProvider, NetworkProperties networkProperties)
        {
            _logger = logger;
            _grpcServiceEntryProvider = grpcServiceEntryProvider;
            _networkProperties = networkProperties;
        }
        public  Task StartAsync(EndPoint endPoint)
        {
            var ipEndPoint = endPoint as IPEndPoint;
            _server = new Server() { Ports = { new ServerPort(ipEndPoint.Address.ToString(), ipEndPoint.Port, ServerCredentials.Insecure) } };
 
            try
            {
                var entries = _grpcServiceEntryProvider.GetEntries();

                var serverServiceDefinitions = new List<ServerServiceDefinition>();
                foreach (var entry in entries)
                {

                    var baseType = entry.Type.BaseType.BaseType;
                    var definitionType = baseType?.DeclaringType;

                    var methodInfo = definitionType?.GetMethod("BindService", new Type[] { baseType });
                    if (methodInfo != null)
                    {
                        var serviceDescriptor = methodInfo.Invoke(null, new object[] { entry.Behavior }) as ServerServiceDefinition;
                        if (serviceDescriptor != null)
                        {
                            _server.Services.Add(serviceDescriptor);
                            continue;
                        }
                    }
                } 
                _server.Start();
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug($"Grpc服务主机启动成功，监听地址：{endPoint}。");
            }
            catch
            {
                _logger.LogError($"Grpc服务主机启动失败，监听地址：{endPoint}。 ");
            }
            return Task.CompletedTask;
        }

        public Server  Server
        {
            get
            {
                return _server;
            }
        }

        public string Id { get; set; }

        public event ReceivedDelegate Received;

        public Task OnReceived(IMessageSender sender, TransportMessage message)
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _server.ShutdownAsync();
        }

        public async Task StartAsync()
        {
           await StartAsync(_networkProperties.CreateSocketAddress());
        }

        NetworkType INetwork.GetType()
        {
            return NetworkType.Grpc;
        }

        public void Shutdown()
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"Grpc服务主机已停止。");
            _server.ShutdownAsync();
        }

        public bool IsAlive()
        {
            return true;
        }

        public bool IsAutoReload()
        {
            return false;
        }
    }
}
