using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Surging.Core.Caching.Configurations.Remote
{
    public  class RemoteConfigurationEvents
    {
        public Action<HttpRequestMessage> OnSendingRequest { get; set; } = msg => { };

        public Func<IDictionary<string, string>, IDictionary<string, string>> OnDataParsed { get; set; } = data => data;
         
        public virtual void SendingRequest(HttpRequestMessage msg) => OnSendingRequest(msg);

        public virtual IDictionary<string, string> DataParsed(IDictionary<string, string> data) => OnDataParsed(data);
    }
}