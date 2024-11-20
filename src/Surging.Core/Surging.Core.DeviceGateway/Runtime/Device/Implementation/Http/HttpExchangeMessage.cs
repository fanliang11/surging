using DotNetty.Buffers;
using DotNetty.Common.Utilities;
using Surging.Core.DeviceGateway.Runtime.Core.Implementation;
using Surging.Core.DeviceGateway.Runtime.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Device.Implementation.Http
{
    public delegate Task ResponseEventHandler(HttpResponseMessage message);
    public class HttpExchangeMessage: HttpRequestMessage
    {
        public event ResponseEventHandler ResponseEvent=default ; 

        public HttpExchangeMessage(HttpRequestMessage request)
        {
            this.Request = request;
        }

        public HttpRequestMessage Request { get; internal set; }

        public string Path { get { return Request.Path; } }

        public string Url { get { return Request.Url; } }


        public HttpMethod Method { get { return Request.Method; } }

        public new IByteBuffer Payload { get { return Request.Payload; } }

        public string ContentType { get { return Request.ContentType; } }


        public List<Header> Headers { get { return Request.Headers; } }

        public async Task OnResponseEvent(HttpResponseMessage message)
        { 
            if (ResponseEvent == null)
                return;
            await ResponseEvent(message);
        }
        public Dictionary<string, string> QueryParameters { get { return Request.QueryParameters; } }
        public Task Success([NotNullWhen(true)] string message)
        {
            return OnResponseEvent(new DefaultHttpResponseMessage().ContentType(MediaType.ApplicationJson)
                                                     .Status(HttpStatus.Success)
                                                     .Body(message));

        }

        public Task Error(int status, [NotNullWhen(true)] string message)
        {
            return OnResponseEvent(new DefaultHttpResponseMessage().ContentType(MediaType.ApplicationJson)
                                                     .Status(status)
                                                     .Body(message));
        }

        public Task Response(DefaultHttpResponseMessage message)
        {
            return OnResponseEvent(message);
        }

    }
}
