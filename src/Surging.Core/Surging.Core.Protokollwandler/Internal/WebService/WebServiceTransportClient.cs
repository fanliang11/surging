using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Surging.Core.Protokollwandler.Internal.WebService
{
    class WebServiceTransportClient : ITransportClient
    {
        private readonly IWebServiceProvider _webServiceProvider;
        public WebServiceTransportClient(IWebServiceProvider webServiceProvider)
        {
            _webServiceProvider = webServiceProvider;
        }

        public   Task<string> SendAsync(string address, IDictionary<string, object> parameters, HttpContext httpContext)
        {
            var webServiceMessage = new XmlDocument();
            switch (httpContext.Request.Method)
            {
                case "POST":
                    {

                        webServiceMessage =   _webServiceProvider.QueryPostWebService(address.Substring(0,address.LastIndexOf("/")), address.Substring(address.LastIndexOf("/")+1), parameters);
                        break;
                    }
                case "GET":
                    {
                        webServiceMessage = _webServiceProvider.QueryGetWebService(address.Substring(0, address.LastIndexOf("/")), address.Substring(address.LastIndexOf("/")+1), parameters);
                        break;
                    }
         

            }
            return Task.FromResult<string>(webServiceMessage.OuterXml);
        }
    }
}
