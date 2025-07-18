using DotNetty.Buffers;
using Newtonsoft.Json.Linq;
using Surging.Core.DeviceGateway.Runtime.Core.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Device.Implementation.Http
{
    public class DefaultHttpResponseMessage : HttpResponseMessage
    {
        public DefaultHttpResponseMessage Body(string payload)
        {
            return Payload(Encoding.UTF8.GetBytes( payload));
        }

        public DefaultHttpResponseMessage Body(byte[] payload)
        {
            return Payload(Unpooled.WrappedBuffer(payload));
        }

        public DefaultHttpResponseMessage Payload(string payload)
        {
            return Payload(Encoding.UTF8.GetBytes(payload));
        }

        public DefaultHttpResponseMessage Payload(byte[] payload)
        {
            return Payload(Unpooled.WrappedBuffer(payload));
        }

        public DefaultHttpResponseMessage Payload(IByteBuffer buffer)
        {
            base.Payload=buffer;
            base.Headers.Add(new Http.Header() { Name = "Content-Length", Value = new string[] { base.Payload.ReadableBytes.ToString()} });

            return this ;
        }

        public DefaultHttpResponseMessage ContentType(int mediaType)
        { 
             base.ContentType=(MediaType.ToString(mediaType));
              base.Headers.Add(new Http.Header() { Name= "Content-Type" , Value=new string[] { base.ContentType, "charset=utf-8" } });
            return this ;
        }

        public DefaultHttpResponseMessage ContentType(int mediaType,string charset)
        {
            base.ContentType = (MediaType.ToString(mediaType));
            base.Headers.Add(new Http.Header() { Name = "Content-Type", Value = new string[] { base.ContentType, $"charset={charset}" } });
            return this;
        }

        public DefaultHttpResponseMessage Header(String key, params string[] values)
        {
            if (Headers == null)
            {
                base.Headers = new List<Header>();
            }
            base.Headers.Add(new Header() { Name = key, Value = values });
            return this;
        }

        public DefaultHttpResponseMessage Headers(Dictionary<string, object> headers)
        {
            foreach(var header in  headers)
            {
               Header(header.Key,header.Value?.ToString());
            }
            return this;
        }

        public DefaultHttpResponseMessage Status(int status)
        {
            base.Status = status;
            return this;
        }

        public DefaultHttpResponseMessage Status(HttpStatus status)
        {
            Status((int)status);
            return this;
        }

        public DefaultHttpResponseMessage headers(List<Header> headers)
        {
            base.Headers = headers;
            return this;
        }

    }
}
