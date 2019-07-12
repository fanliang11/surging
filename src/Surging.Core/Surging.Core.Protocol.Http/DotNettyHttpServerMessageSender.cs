using DotNetty.Buffers;
using DotNetty.Codecs.Http;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Serialization;
using Surging.Core.CPlatform.Transport;
using Surging.Core.CPlatform.Transport.Codec;
using Surging.Core.DotNetty;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Http
{
    /// <summary>
    /// Defines the <see cref="DotNettyHttpServerMessageSender" />
    /// </summary>
    public class DotNettyHttpServerMessageSender : DotNettyMessageSender, IMessageSender
    {
        #region 字段

        /// <summary>
        /// Defines the _context
        /// </summary>
        private readonly IChannelHandlerContext _context;

        /// <summary>
        /// Defines the _serializer
        /// </summary>
        private readonly ISerializer<string> _serializer;

        /// <summary>
        /// Defines the ContentLengthEntity
        /// </summary>
        private readonly AsciiString ContentLengthEntity = HttpHeaderNames.ContentLength;

        /// <summary>
        /// Defines the ContentTypeEntity
        /// </summary>
        private readonly AsciiString ContentTypeEntity = HttpHeaderNames.ContentType;

        /// <summary>
        /// Defines the ServerEntity
        /// </summary>
        private readonly AsciiString ServerEntity = HttpHeaderNames.Server;

        /// <summary>
        /// Defines the TypeJson
        /// </summary>
        private readonly AsciiString TypeJson = AsciiString.Cached("application/json");

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="DotNettyHttpServerMessageSender"/> class.
        /// </summary>
        /// <param name="transportMessageEncoder">The transportMessageEncoder<see cref="ITransportMessageEncoder"/></param>
        /// <param name="context">The context<see cref="IChannelHandlerContext"/></param>
        /// <param name="serializer">The serializer<see cref="ISerializer{string}"/></param>
        public DotNettyHttpServerMessageSender(ITransportMessageEncoder transportMessageEncoder, IChannelHandlerContext context, ISerializer<string> serializer) : base(transportMessageEncoder)
        {
            _context = context;
            _serializer = serializer;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// 发送消息并清空缓冲区。
        /// </summary>
        /// <param name="message">消息内容。</param>
        /// <returns>一个任务。</returns>
        public async Task SendAndFlushAsync(TransportMessage message)
        {
            var buffer = GetByteBuffer(message, out int contentLength);
            var response = WriteResponse(_context, buffer, TypeJson, AsciiString.Cached($"{ contentLength}"));

            await _context.WriteAndFlushAsync(response);
            await _context.CloseAsync();
        }

        /// <summary>
        /// 发送消息。
        /// </summary>
        /// <param name="message">消息内容。</param>
        /// <returns>一个任务。</returns>
        public async Task SendAsync(TransportMessage message)
        {
            var buffer = GetByteBuffer(message, out int contentLength);
            var response = WriteResponse(_context, buffer, TypeJson, AsciiString.Cached($"{contentLength}"));
            await _context.WriteAsync(response);
        }

        /// <summary>
        /// The GetByteBuffer
        /// </summary>
        /// <param name="message">The message<see cref="TransportMessage"/></param>
        /// <param name="contentLength">The contentLength<see cref="int"/></param>
        /// <returns>The <see cref="IByteBuffer"/></returns>
        private IByteBuffer GetByteBuffer(TransportMessage message, out int contentLength)
        {
            contentLength = 0;
            if (!message.IsHttpResultMessage())
                return null;

            var httpResultMessage = message.GetContent<HttpResultMessage>();
            var data = Encoding.UTF8.GetBytes(_serializer.Serialize(httpResultMessage));
            contentLength = data.Length;
            return Unpooled.WrappedBuffer(data);
        }

        /// <summary>
        /// The WriteResponse
        /// </summary>
        /// <param name="ctx">The ctx<see cref="IChannelHandlerContext"/></param>
        /// <param name="buf">The buf<see cref="IByteBuffer"/></param>
        /// <param name="contentType">The contentType<see cref="ICharSequence"/></param>
        /// <param name="contentLength">The contentLength<see cref="ICharSequence"/></param>
        /// <returns>The <see cref="DefaultFullHttpResponse"/></returns>
        private DefaultFullHttpResponse WriteResponse(IChannelHandlerContext ctx, IByteBuffer buf, ICharSequence contentType, ICharSequence contentLength)
        {
            var response = new DefaultFullHttpResponse(HttpVersion.Http11, HttpResponseStatus.OK, buf, false);
            HttpHeaders headers = response.Headers;
            headers.Set(ContentTypeEntity, contentType);
            headers.Set(ContentLengthEntity, contentLength);
            return response;
        }

        #endregion 方法
    }
}