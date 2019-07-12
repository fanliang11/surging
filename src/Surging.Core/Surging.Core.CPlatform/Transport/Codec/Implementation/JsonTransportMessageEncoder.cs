using Newtonsoft.Json;
using Surging.Core.CPlatform.Messages;
using System.Text;

namespace Surging.Core.CPlatform.Transport.Codec.Implementation
{
    /// <summary>
    /// Defines the <see cref="JsonTransportMessageEncoder" />
    /// </summary>
    public sealed class JsonTransportMessageEncoder : ITransportMessageEncoder
    {
        #region 方法

        /// <summary>
        /// The Encode
        /// </summary>
        /// <param name="message">The message<see cref="TransportMessage"/></param>
        /// <returns>The <see cref="byte[]"/></returns>
        public byte[] Encode(TransportMessage message)
        {
            var content = JsonConvert.SerializeObject(message);
            return Encoding.UTF8.GetBytes(content);
        }

        #endregion 方法
    }
}