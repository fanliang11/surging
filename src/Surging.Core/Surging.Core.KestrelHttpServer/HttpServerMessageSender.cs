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
    /// <summary>
    /// Defines the <see cref="HttpServerMessageSender" />
    /// </summary>
    public class HttpServerMessageSender : IMessageSender
    {
        #region 字段

        /// <summary>
        /// Defines the _context
        /// </summary>
        private readonly HttpContext _context;

        /// <summary>
        /// Defines the _serializer
        /// </summary>
        private readonly ISerializer<string> _serializer;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpServerMessageSender"/> class.
        /// </summary>
        /// <param name="serializer">The serializer<see cref="ISerializer{string}"/></param>
        /// <param name="httpContext">The httpContext<see cref="HttpContext"/></param>
        public HttpServerMessageSender(ISerializer<string> serializer, HttpContext httpContext)
        {
            _serializer = serializer;
            _context = httpContext;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The SendAndFlushAsync
        /// </summary>
        /// <param name="message">The message<see cref="TransportMessage"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task SendAndFlushAsync(TransportMessage message)
        {
            var httpMessage = message.GetContent<HttpResultMessage<Object>>();
            var actionResult = httpMessage.Entity as IActionResult;
            if (actionResult == null)
            {
                var text = _serializer.Serialize(message.Content);
                var data = Encoding.UTF8.GetBytes(text);
                var contentLength = data.Length;
                _context.Response.Headers.Add("Content-Type", "application/json");
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

        /// <summary>
        /// The SendAsync
        /// </summary>
        /// <param name="message">The message<see cref="TransportMessage"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task SendAsync(TransportMessage message)
        {
            var actionResult = message.GetContent<IActionResult>();
            if (actionResult == null)
            {
                var text = _serializer.Serialize(message);
                var data = Encoding.UTF8.GetBytes(_serializer.Serialize(text));
                var contentLength = data.Length;
                _context.Response.Headers.Add("Content-type", "application/json");
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

        #endregion 方法
    }
}