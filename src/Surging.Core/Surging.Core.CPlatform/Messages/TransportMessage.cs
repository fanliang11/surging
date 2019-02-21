using System;
using System.Runtime.CompilerServices;

namespace Surging.Core.CPlatform.Messages
{
    /// <summary>
    /// 传输消息模型。
    /// </summary>
    public class TransportMessage
    {

        public TransportMessage()
        {
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TransportMessage(object content)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            Content = content;
            ContentType = content.GetType().FullName;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TransportMessage(object content, string fullName)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            Content = content;
            ContentType = fullName;
        }

        /// <summary>
        /// 消息Id。
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 消息内容。
        /// </summary>
        public object Content { get; set; }

        /// <summary>
        /// 内容类型。
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// 是否调用消息。
        /// </summary>
        /// <returns>如果是则返回true，否则返回false。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsInvokeMessage()
        {
            return ContentType == MessagePackTransportMessageType.remoteInvokeMessageTypeName;
        }

        /// <summary>
        /// 是否是调用结果消息。
        /// </summary>
        /// <returns>如果是则返回true，否则返回false。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsInvokeResultMessage()
        {
            return ContentType == MessagePackTransportMessageType.remoteInvokeResultMessageTypeName;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHttpMessage()
        {
            return ContentType == MessagePackTransportMessageType.httpMessageTypeName;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHttpResultMessage()
        {
            return ContentType == MessagePackTransportMessageType.httpResultMessageTypeName;
        }

        /// <summary>
        /// 获取内容。
        /// </summary>
        /// <typeparam name="T">内容类型。</typeparam>
        /// <returns>内容实例。</returns> 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetContent<T>()
        {
            return (T)Content;
        }

        /// <summary>
        /// 创建一个调用传输消息。
        /// </summary>
        /// <param name="invokeMessage">调用实例。</param>
        /// <returns>调用传输消息。</returns>  
        public static TransportMessage CreateInvokeMessage(RemoteInvokeMessage invokeMessage)
        {
            return new TransportMessage(invokeMessage, MessagePackTransportMessageType.remoteInvokeMessageTypeName)
            {
                Id = Guid.NewGuid().ToString("N")
            };
        }

        /// <summary>
        /// 创建一个调用结果传输消息。
        /// </summary>
        /// <param name="id">消息Id。</param>
        /// <param name="invokeResultMessage">调用结果实例。</param>
        /// <returns>调用结果传输消息。</returns>  
        public static TransportMessage CreateInvokeResultMessage(string id, RemoteInvokeResultMessage invokeResultMessage)
        {
            return new TransportMessage(invokeResultMessage, MessagePackTransportMessageType.remoteInvokeResultMessageTypeName)
            {
                Id = id
            };
        }
    }
}