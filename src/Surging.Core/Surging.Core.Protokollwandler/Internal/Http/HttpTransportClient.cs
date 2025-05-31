
using Microsoft.AspNetCore.Http;
using Surging.Core.CPlatform.Messages;
using Surging.Core.Protokollwandler.Internal;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.CPlatform.Serialization;
using Autofac;
using System.Web;

namespace Surging.Core.Protokollwandler.Internal.Http
{
    public class HttpTransportClient : ITransportClient
    { 
        private readonly IHttpClientProvider _httpClientProvider;
        private readonly ISerializer<string> _serializer;
        public HttpTransportClient(IHttpClientProvider httpClientProvider)
        {
            _serializer = ServiceLocator.Current.Resolve<ISerializer<string>>();
            _httpClientProvider = httpClientProvider; 
        }

        public async Task<string> SendAsync(string address, IDictionary<string, object> parameters, HttpContext httpContext)
        {
            var httpMessage = "";
            switch (httpContext.Request.Method)
            {
                case "POST":
                    {
                        
                        httpMessage = await _httpClientProvider.PostJsonMessageAsync<string>($"{address}{httpContext.Request.QueryString.Value}", _serializer.Serialize(parameters.Values.FirstOrDefault()), httpContext.Request.Headers.ToDictionary(p=>p.Key,p=>p.Value.ToString()));
                        break;
                    }
                case "GET":
                    {
                        httpMessage = await _httpClientProvider.GetJsonMessageAsync<string>($"{address}{httpContext.Request.QueryString.Value}", httpContext.Request.Headers.ToDictionary(p => p.Key, p => p.Value.ToString()));
                        break;
                    }
                case "PUT":
                    {
                        httpMessage = await _httpClientProvider.PutJsonMessageAsync<string>($"{address}{httpContext.Request.QueryString.Value}", _serializer.Serialize(parameters), httpContext.Request.Headers.ToDictionary(p => p.Key, p => p.Value.ToString()));
                        break;
                    }
                case "Delete":
                    {
                        httpMessage = await _httpClientProvider.DeleteJsonMessageAsync<string>($"{address}{httpContext.Request.QueryString.Value}", _serializer.Serialize(parameters), httpContext.Request.Headers.ToDictionary(p => p.Key, p => p.Value.ToString()));
                        break;
                    }

            }
            return httpMessage;
        }

    }
}
