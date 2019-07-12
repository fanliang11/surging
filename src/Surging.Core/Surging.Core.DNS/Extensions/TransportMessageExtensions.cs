using Surging.Core.CPlatform.Messages;
using System.Runtime.CompilerServices;

namespace Surging.Core.DNS.Extensions
{
    /// <summary>
    /// Defines the <see cref="TransportMessageExtensions" />
    /// </summary>
    public static class TransportMessageExtensions
    {
        #region 方法

        /// <summary>
        /// The IsDnsResultMessage
        /// </summary>
        /// <param name="message">The message<see cref="TransportMessage"/></param>
        /// <returns>The <see cref="bool"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDnsResultMessage(this TransportMessage message)
        {
            return message.ContentType == typeof(DnsTransportMessage).FullName;
        }

        #endregion 方法
    }
}