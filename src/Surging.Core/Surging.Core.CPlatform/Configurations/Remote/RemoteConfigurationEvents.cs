using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Surging.Core.CPlatform.Configurations.Remote
{
    /// <summary>
    /// Defines the <see cref="RemoteConfigurationEvents" />
    /// </summary>
    public class RemoteConfigurationEvents
    {
        #region 属性

        /// <summary>
        /// Gets or sets the OnDataParsed
        /// </summary>
        public Func<IDictionary<string, string>, IDictionary<string, string>> OnDataParsed { get; set; } = data => data;

        /// <summary>
        /// Gets or sets the OnSendingRequest
        /// </summary>
        public Action<HttpRequestMessage> OnSendingRequest { get; set; } = msg => { };

        #endregion 属性

        #region 方法

        /// <summary>
        /// The DataParsed
        /// </summary>
        /// <param name="data">The data<see cref="IDictionary{string, string}"/></param>
        /// <returns>The <see cref="IDictionary{string, string}"/></returns>
        public virtual IDictionary<string, string> DataParsed(IDictionary<string, string> data) => OnDataParsed(data);

        /// <summary>
        /// The SendingRequest
        /// </summary>
        /// <param name="msg">The msg<see cref="HttpRequestMessage"/></param>
        public virtual void SendingRequest(HttpRequestMessage msg) => OnSendingRequest(msg);

        #endregion 方法
    }
}