using Microsoft.AspNetCore.Http;
using Surging.Core.CPlatform.Serialization;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protokollwandler.Internal.Implementation
{
    public class MessageSender : IMessageSender
    {
        private readonly ISerializer<string> _serializer;
        private readonly HttpContext _context; 
        public MessageSender(ISerializer<string> serializer, HttpContext httpContext)
        {
            _serializer = serializer;
            _context = httpContext;
        }

        public async Task SendAndFlushAsync(string message, string contentType)
        {
            _context.Response.Clear();
            var data = Encoding.UTF8.GetBytes(message);
            var contentLength = data.Length;
            _context.Response.Headers.Add("Content-Type", contentType);
            _context.Response.Headers.Add("Content-Length", contentLength.ToString());
            await _context.Response.WriteAsync(message);
        } 
    }
}
