using Microsoft.AspNetCore.Http;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Serialization;
using Surging.Core.CPlatform.Transport;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.KestrelHttpServer
{
    public class HttpServerMessageSender : IMessageSender
    {
        private readonly ISerializer<string> _serializer;
        private readonly HttpContext _context;
       public  HttpServerMessageSender(ISerializer<string> serializer,HttpContext httpContext)
        {
            _serializer = serializer;
            _context = httpContext;
        }

        public async Task SendAndFlushAsync(TransportMessage message)
        {
            var text = _serializer.Serialize(message.Content);
            var data = Encoding.UTF8.GetBytes(text);
            var contentLength = data.Length;
            _context.Response.Headers.Add("Content-Type", "application/json");
            _context.Response.Headers.Add("Content-Length", contentLength.ToString());
            await _context.Response.WriteAsync(text);
        }

        public async Task SendAsync(TransportMessage message)
        {
            var text = _serializer.Serialize(message);
            var data = Encoding.UTF8.GetBytes(_serializer.Serialize(text));
            var  contentLength = data.Length;
            _context.Response.Headers.Add("Content-type", "application/json");
            _context.Response.Headers.Add("Content-Length", contentLength.ToString());
           await  _context.Response.WriteAsync(text);
           
           
        }

        private async Task WriteResponse(HttpContext context, TransportMessage message)
        {
            await context.Response.WriteAsync("hello, world");
            await context.Response.Body.FlushAsync();
        }
    }
}
