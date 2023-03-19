using Grpc.Core;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Transport;
using Surging.Core.Grpc.Runtime;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Grpc
{
   public  class GrpcServerMessageListener: IMessageListener, IDisposable
    { 
        private Server _server;
        private readonly ILogger<GrpcServerMessageListener> _logger;
        private readonly IGrpcServiceEntryProvider _grpcServiceEntryProvider;

        public GrpcServerMessageListener(ILogger<GrpcServerMessageListener> logger,
            IGrpcServiceEntryProvider grpcServiceEntryProvider)
        {
            _logger = logger;
            _grpcServiceEntryProvider = grpcServiceEntryProvider; 
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

        public event ReceivedDelegate Received;

        public Task OnReceived(IMessageSender sender, TransportMessage message)
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _server.ShutdownAsync();
        }
    }
}
