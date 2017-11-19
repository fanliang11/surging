using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Surging.Core.Consul.Utilitys
{
    public class HttpUtils
    {
        public static readonly string DefaultHost = "localhost";
        public static readonly int DefaultPort = 8500;
        public static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(6);

        public static HttpClient CreateClient(string agentHost = null, int? agentPort = null)
        {
            var host = agentHost ?? DefaultHost;
            var port = agentPort ?? DefaultPort;

            var uri = new UriBuilder("http", host, port);
            return new HttpClient(new HttpClientHandler() { MaxConnectionsPerServer = 50 })
            {
                BaseAddress = uri.Uri,
                Timeout = DefaultTimeout
            };
        }
    }
}
