using Microsoft.AspNetCore.Http;
using Surging.Core.CPlatform.Diagnostics;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Serialization;
using Surging.Core.CPlatform.Transport;
using Surging.Core.CPlatform.Transport.Implementation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            var httpMessage = message.GetContent<HttpResultMessage<Object>>();
            var actionResult= httpMessage.Entity as IActionResult;
            WirteDiagnostic(message);
            if (actionResult == null)
            {
                var text = _serializer.Serialize(message.Content);
                var data = Encoding.UTF8.GetBytes(text);
                var contentLength = data.Length;
                _context.Response.Headers.Add("Content-Type", "application/json;charset=utf-8");
                _context.Response.Headers.Add("Content-Length", contentLength.ToString());
                await _context.Response.WriteAsync(text);
            }
            else
            {
                await actionResult.ExecuteResultAsync(new ActionContext
                {
                    HttpContext = _context,
                    Message = message
                });
            }
        }

        public async Task SendAsync(TransportMessage message)
        {
           await this.SendAndFlushAsync(message);
        }

        private void WirteDiagnostic(TransportMessage message)
        {
            if (!CPlatform.AppConfig.ServerOptions.DisableDiagnostic)
            {
                var diagnosticListener = new DiagnosticListener(DiagnosticListenerExtensions.DiagnosticListenerName);
                var remoteInvokeResultMessage = message.GetContent<HttpResultMessage>();
                if (remoteInvokeResultMessage.IsSucceed)
                {
                    diagnosticListener.WriteTransportAfter(TransportType.Rest, new ReceiveEventData(new DiagnosticMessage
                    {
                        Content = message.Content,
                        ContentType = message.ContentType,
                        Id = message.Id
                    }));
                }
                else
                {
                    diagnosticListener.WriteTransportError(TransportType.Rest, new TransportErrorEventData(new DiagnosticMessage
                    {
                        Content = message.Content,
                        ContentType = message.ContentType,
                        Id = message.Id
                    }, new Exception(remoteInvokeResultMessage.Message)));
                }
            }
        }
          
    }
}
