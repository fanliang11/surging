using Newtonsoft.Json;
using Surging.Core.CPlatform.Messages;
using System.Text;

namespace Surging.Core.CPlatform.Transport.Codec.Implementation
{
    /// <summary>
    /// Defines the <see cref="JsonTransportMessageDecoder" />
    /// </summary>
    public sealed class JsonTransportMessageDecoder : ITransportMessageDecoder
    {
        #region 方法

        /// <summary>
        /// The Decode
        /// </summary>
        /// <param name="data">The data<see cref="byte[]"/></param>
        /// <returns>The <see cref="TransportMessage"/></returns>
        public TransportMessage Decode(byte[] data)
        {
            var content = Encoding.UTF8.GetString(data);
            var message = JsonConvert.DeserializeObject<TransportMessage>(content);
            if (message.IsInvokeMessage())
            {
                message.Content = JsonConvert.DeserializeObject<RemoteInvokeMessage>(message.Content.ToString());
            }
            if (message.IsInvokeResultMessage())
            {
                message.Content = JsonConvert.DeserializeObject<RemoteInvokeResultMessage>(message.Content.ToString());
            }
            return message;
        }

        #endregion 方法
    }
}