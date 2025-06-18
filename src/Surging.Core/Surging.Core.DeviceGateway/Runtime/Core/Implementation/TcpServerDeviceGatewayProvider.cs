using DotNetty.Common.Utilities;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Network;
using Surging.Core.CPlatform.Protocol;
using Surging.Core.DeviceGateway.Runtime.Device;
using Surging.Core.DeviceGateway.Runtime.Session;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Surging.Core.DeviceGateway.Runtime.Core.Implementation
{
    public class TcpServerDeviceGatewayProvider : DeviceGatewayProvider
    {
        private const NetworkType NETWORK_TYPE = NetworkType.Tcp;


        public readonly INetworkManager _networkManager;

        public readonly IDeviceRegistry _registry;

        public readonly IDeviceSessionManager _sessionManager;

        public readonly IProtocolSupports _protocolSupports;

        public readonly ILogger<TcpServerDeviceGatewayProvider> _logger;


        public TcpServerDeviceGatewayProvider(INetworkManager networkManager,
                                      IDeviceRegistry registry,
                                      IDeviceSessionManager sessionManager, 
                                      IProtocolSupports protocolSupports)
        {
            _networkManager = networkManager;
            _registry = registry;
            _sessionManager = sessionManager; 
            _protocolSupports = protocolSupports;
        }

        public override IObservable<IDeviceGateway> CreateDeviceGateway(DeviceGatewayProperties properties)
        {
            var result = Observable.Empty<IDeviceGateway>();
            var tcpNetwork = _networkManager.GetNetwork(NETWORK_TYPE, properties.ChannelId).Subscribe(p =>
            {
                string protocol = properties.Protocol;
                if (string.IsNullOrEmpty(protocol))
                {
                    if (_logger.IsEnabled(LogLevel.Error))
                        _logger.LogError("消息协议不能为空");
                }
                result= result.Publish(new TcpDeviceGateway(properties.Id,
                    _protocolSupports.GetProtocol(protocol),
                     _registry,
                      _sessionManager,
                      p));
            });
            return result;
        }
         
        public override string GetId()
        {
            return "TcpGateway";
        }

        public override string GetName()
        {
            return "TCP 透传接入";
        }

        public override MessageTransport GetTransport()
        {
            return MessageTransport.Tcp;
        }

        public override ISubject<IDeviceGateway> ReloadDeviceGateway(IDeviceGateway gateway, DeviceGatewayProperties properties)
        {
            var deviceGateway = gateway as TcpDeviceGateway;
                           var result = new AsyncSubject<IDeviceGateway>();
            //网络组件发生变化
            if (string.Equals(NETWORK_TYPE.ToString(), properties.ChannelId,StringComparison.OrdinalIgnoreCase))
            {
                 gateway
                    .ShutdownAsync()
                    .Subscribe(p => this.CreateDeviceGateway(properties)
                    .Subscribe(deviceGateway =>
                    {
                        deviceGateway.Startup();
                        result.OnNext(gateway);
                        result.OnCompleted();
                    }));
                    
                return result;
            }  
            String protocol = properties.Protocol;
            _protocolSupports.GetProtocol(protocol).Subscribe(p =>
            {
                //更新协议 
                if (deviceGateway != null)
                {
                    deviceGateway.Protocol = Observable.Return(p);
                    result.OnNext(gateway);
                    result.OnCompleted();
                }
            });
            return result;
        }
    }
}
