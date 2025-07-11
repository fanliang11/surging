using DotNetty.Buffers;
using DotNetty.Common.Utilities;
using Surging.Core.CPlatform.Codecs.Core;
using Surging.Core.DeviceGateway.Runtime.Core;
using Surging.Core.DeviceGateway.Runtime.Core.Implementation;
using Surging.Core.DeviceGateway.Runtime.Device.Implementation.Http;
using Surging.Core.DeviceGateway.Runtime.Device.MessageCodec;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading.Tasks;
using HttpRequestMessage = Surging.Core.DeviceGateway.Runtime.Device.Implementation.Http.HttpRequestMessage;

namespace Surging.Core.DeviceGateway.Runtime.Device.Implementation
{
    public class HttpDeviceMessageCodec : DeviceMessageCodec
    {
        private readonly MessageTransport _transport;

        public HttpDeviceMessageCodec() : this(MessageTransport.Http)
        {
        }

        private static DefaultHttpResponseMessage Unauthorized(String msg)
        {
            return new DefaultHttpResponseMessage()
                    .ContentType(MediaType.ApplicationJson)
                    .Body("{\"success\":false,\"code\":\"unauthorized\",\"message\":\"" + msg + "\"}")
                    .Status(HttpStatus.AuthorizationFailed);
        }

        private static DefaultHttpResponseMessage BadRequest()
        {
            return new DefaultHttpResponseMessage()
                    .ContentType(MediaType.ApplicationJson)
                    .Body("{\"success\":false,\"code\":\"bad_request\"}")
                    .Status(HttpStatus.RequestError);
        }

        public HttpDeviceMessageCodec(MessageTransport transport)
        {
            _transport = transport;
        }
        public override IObservable<IDeviceMessage> Decode(MessageDecodeContext context)
        {
            if (context.GetMessage() is HttpRequestMessage)
            {
                return DecodeHttpRequestMessage(context);
            }
            return Observable.Return<IDeviceMessage>(default);
        }


        public override  IObservable<IEncodedMessage> Encode(MessageEncodeContext context)
        {
            return Observable.Return<IEncodedMessage>(default);
        }



        private IObservable<IDeviceMessage> DecodeHttpRequestMessage(MessageDecodeContext context)
        {
            var result = Observable.Return<IDeviceMessage>(default);
            var message = (HttpExchangeMessage)context.GetMessage();

            Header? header = message.Request.GetHeader("Authorization");
            if (header == null || header.Value == null || header.Value.Length == 0)
            {
                message
                     .Response(Unauthorized("Authorization header is required")).ToObservable()
                     .Subscribe(p => result = result.Publish(default));

                return result;
            }
            var httpToken = header.Value[0];

            var paths = message.Path.Split("/");
            if (paths.Length == 0)
            {
                message.Response(BadRequest()).ToObservable()
                   .Subscribe(p => result = result.Publish(default));
                return result;
            }
            String deviceId = paths[1];
            context.GetDevice(deviceId).Subscribe(async deviceOperator =>
            {
                var config = deviceOperator==null?null: await deviceOperator.GetConfig("token");
                var token = config?.Convert<string>();
                if (token == null || !httpToken.Equals(token))
                {
                    await message
                         .Response(Unauthorized("Device not registered or authentication failed"));
                }
                else
                {
                    var deviceMessage = await DecodeBody(message, deviceId);
                    if (deviceMessage != null)
                    {
                        await message.Success("{\"success\":true,\"code\":\"success\"}");
                        result = result.Publish(deviceMessage);
                    }
                    else
                    {
                        await message.Response(BadRequest());
                    }
                }
            });
            return result;
        }

        private async Task<IDeviceMessage> DecodeBody(HttpExchangeMessage message,string deviceId)
        {

            byte[] body = new byte[message.Payload.ReadableBytes];
            message.Payload.ReadBytes(body);
            var deviceMessage = await TopicMessageCodec.Dodecode(message.Path, body);
            deviceMessage.DeviceId = deviceId;
            return deviceMessage;
        }
    }
}
