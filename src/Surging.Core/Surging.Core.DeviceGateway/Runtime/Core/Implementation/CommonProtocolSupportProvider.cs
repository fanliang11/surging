using Surging.Core.CPlatform.Protocol;
using Surging.Core.DeviceGateway.Runtime.Core.Metadata.Type;
using Surging.Core.DeviceGateway.Runtime.Core.Metadata;
using Surging.Core.DeviceGateway.Runtime.Device.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Surging.Core.DeviceGateway.Runtime.Device;
using System.Runtime.InteropServices;

namespace Surging.Core.DeviceGateway.Runtime.Core.Implementation
{
    public class CommonProtocolSupportProvider : ProtocolSupportProvider
    {

        private readonly DefaultConfigMetadata _tcpConfig = new DefaultConfigMetadata(
        "TCP认证配置"
        , "key为tcp认证密钥")
        .Add("tcp_auth_key", "key", "TCP认证KEY", StringType.Instance);


        private readonly DefaultConfigMetadata _udpConfig = new DefaultConfigMetadata(
"udp认证配置"
, "key为udp认证密钥")
.Add("udp_auth_key", "key", "udp认证KEY", StringType.Instance);

        private readonly DefaultConfigMetadata _mqttConfig = new DefaultConfigMetadata(
        "Mqtt认证配置"
        , "secureId以及secureKey在创建设备产品或设备实例时进行配置.\r\n    timestamp为当前时间戳(毫秒), 与服务器时间不能相差5分钟.\r\n        md5为32位, 不区分大小写")
        .Add("secureId", "secureId", "用户唯一标识编号", StringType.Instance)
        .Add("secureKey", "secureKey", "密钥", StringType.Instance);
        public override IObservable<IProtocolSupport> Create(ProtocolContext context)
        {
            return Observable.Return<IProtocolSupport>(default);
        }

        public  IObservable<IProtocolSupport> Create(ProtocolSupportProperties  properties)
        {
            var support = new ComplexProtocolSupport();
            support.Id = properties.Id;
            support.Name = properties.Name;
            support.Description = properties.Description;
            switch(properties.Transport)
            {
                case MessageTransport.Tcp:
                    {
                        support.AddConfigMetadata(properties.Transport, _tcpConfig);
                        support.AddAuthenticator(properties.Transport, new TcpAuthenticator());
                        support.AddMessageCodecSupport(properties.Transport, () => Observable.Return(new ScriptDeviceMessageCodec(properties.Script)));
                    }
                    break;
                case MessageTransport.Mqtt:
                    {
                        support.AddConfigMetadata(properties.Transport, _mqttConfig);
                        support.AddAuthenticator(properties.Transport, new DefaultAuthenticator());
                        support.AddMessageCodecSupport(properties.Transport, () => Observable.Return(new ScriptDeviceMessageCodec(properties.Script)));
                    }
                    break;
                case MessageTransport.Udp:
                    {
                        support.AddConfigMetadata(properties.Transport, _udpConfig);
                        support.AddAuthenticator(properties.Transport, new TcpAuthenticator());
                        support.AddMessageCodecSupport(properties.Transport, () => Observable.Return(new ScriptDeviceMessageCodec(properties.Script)));
                    }
                    break;
                default:
                    {
                        support.AddConfigMetadata(properties.Transport, _tcpConfig);
                        support.AddAuthenticator(properties.Transport, new TcpAuthenticator());
                        support.AddMessageCodecSupport(properties.Transport, () => Observable.Return(new ScriptDeviceMessageCodec(properties.Script)));
                    }
                   break;
            }
            return Observable.Return<IProtocolSupport>(support);
        }

        public class TcpAuthenticator : IAuthenticator
        {
            public IObservable<AuthenticationResult> Authenticate(IAuthenticationRequest request, IDeviceOperator deviceOperator)
            {
                var result = Observable.Return(AuthenticationResult.Failure(StatusCode.CUSTOM_ERROR, "不支持请求参数类型"));
                var tcp = request as DefaultAuthRequest;
                if (tcp != null)
                {
                    deviceOperator.GetConfig("key").Subscribe(config =>
                    {
                        var password = config.Convert<string>();
                        if (tcp.Password.Equals(password))
                        {
                            result = result.Publish(AuthenticationResult.Success(tcp.DeviceId));
                        }
                        else
                        {
                            result = result.Publish(AuthenticationResult.Failure(StatusCode.CUSTOM_ERROR, "验证失败,密码错误"));
                        }
                    });
                }
                return result;
            }

            public IObservable<AuthenticationResult> Authenticate(IAuthenticationRequest request, IDeviceRegistry registry)
            {
                var result = Observable.Return(AuthenticationResult.Failure(StatusCode.CUSTOM_ERROR, "不支持请求参数类型")); ;
                var tcp = request as DefaultAuthRequest;
                if (tcp != null)
                {
                    registry
                      .GetDevice(tcp.DeviceId)
                      .Subscribe(async p =>
                      {

                          var config = await p.GetConfig("key");
                          var password = config.Convert<string>();
                          if (tcp.Password.Equals(password))
                          {
                              result = result.Publish(AuthenticationResult.Success(tcp.DeviceId));
                          }
                          else
                          {
                              result = result.Publish(AuthenticationResult.Failure(StatusCode.CUSTOM_ERROR, "验证失败,密码错误"));
                          }
                      });
                }
                return result;
            }
        }
    }
}
