using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Surging.Core.CPlatform.Runtime.Server;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Surging.Core.Protokollwandler.Internal.WebService
{
    public class SoapWebServiceTransportClient : ITransportClient
    {
        private readonly IWebServiceProvider _webServiceProvider; 
        public SoapWebServiceTransportClient(IWebServiceProvider webServiceProvider)
        {
            _webServiceProvider = webServiceProvider;
        }

        public Task<string> SendAsync(string address, IDictionary<string, object> parameters, HttpContext httpContext)
        { 
            var webServiceMessage = new XmlDocument();  
            webServiceMessage = _webServiceProvider.QuerySoapWebService(address.Substring(0, address.LastIndexOf("/")), address.Substring(address.LastIndexOf("/") + 1), parameters);
            return Task.FromResult<string>(webServiceMessage.OuterXml);
        }
    }
}
