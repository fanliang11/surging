using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Routing.Template;
using Surging.Core.CPlatform.Serialization;
using Surging.Core.CPlatform.Transport;
using Surging.Core.KestrelHttpServer.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Surging.Core.KestrelHttpServer
{
    public abstract class HttpMessageListener : IMessageListener
    {
        public  event ReceivedDelegate Received;
        private readonly ILogger<HttpMessageListener> _logger;
        private readonly ISerializer<string> _serializer;
        private  event RequestDelegate Requested;
        private readonly IServiceRouteProvider _serviceRouteProvider;

        public HttpMessageListener(ILogger<HttpMessageListener> logger, ISerializer<string> serializer, IServiceRouteProvider serviceRouteProvider)
        {
            _logger = logger;
            _serializer = serializer;
            _serviceRouteProvider = serviceRouteProvider;
        }

        public  async Task OnReceived(IMessageSender sender, TransportMessage message)
        {
            if (Received == null)
                return;
            await Received(sender, message);
        }

        public async Task OnReceived(IMessageSender sender, HttpContext context)
        {
            var path = HttpUtility.UrlDecode(GetRoutePath(context.Request.Path.ToString()));
            var serviceRoute =await _serviceRouteProvider.GetRouteByPathRegex(path);
            IDictionary<string, object> parameters = context.Request.Query.ToDictionary(p => p.Key,p => (object)p.Value.ToString());
            parameters.Remove("servicekey", out object serviceKey);
            StreamReader streamReader = new StreamReader(context.Request.Body);
            var data = await streamReader.ReadToEndAsync();
            if (data.Length > 0)
                parameters = _serializer.Deserialize<string, IDictionary<string, object>>(data) ?? new Dictionary<string, object>();
            if (String.Compare(serviceRoute.ServiceDescriptor.RoutePath, path, true) != 0)
            {
                var @params = RouteTemplateSegmenter.Segment(serviceRoute.ServiceDescriptor.RoutePath, path);
                foreach (var param in @params)
                {
                    parameters.Add(param.Key,param.Value);
                }
            }
            if (context.Request.HasFormContentType)
            {
                var collection =await GetFormCollection(context.Request);
                parameters.Add("form", collection);
                await Received(sender, new TransportMessage(new HttpMessage
                {
                    Parameters = parameters,
                    RoutePath = serviceRoute.ServiceDescriptor.RoutePath,
                    ServiceKey = serviceKey?.ToString()
                }));
            }
            else
            {
              
                if (context.Request.Method == "POST")
                {
                    await Received(sender, new TransportMessage(new HttpMessage
                    {
                        Parameters = parameters,
                        RoutePath = serviceRoute.ServiceDescriptor.RoutePath,
                        ServiceKey = serviceKey?.ToString()
                    }));
                }
                else
                {
                    await Received(sender, new TransportMessage(new HttpMessage
                    {
                        Parameters = parameters,
                        RoutePath = serviceRoute.ServiceDescriptor.RoutePath,
                        ServiceKey = serviceKey?.ToString()
                    }));
                }
            }
        }

        private async Task<HttpFormCollection> GetFormCollection(HttpRequest request)
        {
            var boundary = GetName("boundary=", request.ContentType); 
            var reader = new MultipartReader(boundary, request.Body);
            var collection = await GetMultipartForm(reader);
            var fileCollection = new HttpFormFileCollection();
            var fields = new Dictionary<string, StringValues>();
            foreach (var item in collection)
            {
                if (item.Value is HttpFormFileCollection)
                {
                    var itemCollection = item.Value as HttpFormFileCollection;
                    fileCollection.AddRange(itemCollection);
                }
                else
                {
                    var itemCollection = item.Value as Dictionary<string, StringValues>;
                    fields = fields.Concat(itemCollection).ToDictionary(k => k.Key, v => v.Value);

                }
            }
           return new HttpFormCollection(fields, fileCollection);
        }

        private async Task<IDictionary<string,object>> GetMultipartForm(MultipartReader reader)
        {
           var section = await reader.ReadNextSectionAsync();
            var collection = new Dictionary<string, object>();
            if (section != null)
            { 
                var name=GetName("name=",section.ContentDisposition);
                var fileName = GetName("filename=",section.ContentDisposition);
                var buffer = new MemoryStream();
                await section.Body.CopyToAsync(buffer);
                if(string.IsNullOrEmpty(fileName))
                {
                    var fields = new Dictionary<string, StringValues>();
                    StreamReader streamReader = new StreamReader(buffer);
                    fields.Add(name, new StringValues(UTF8Encoding.Default.GetString(buffer.GetBuffer(),0,(int)buffer.Length)));
                    collection.Add(name, fields);
                }
                else
                {
                    var fileCollection = new HttpFormFileCollection();
                    StreamReader streamReader = new StreamReader(buffer);
                    fileCollection.Add(new HttpFormFile(buffer.Length,name,fileName,buffer.GetBuffer()));
                    collection.Add(name, fileCollection);
                }
                var formCollection= await GetMultipartForm(reader);
                foreach(var item in formCollection)
                {
                    if (!collection.ContainsKey(item.Key))
                        collection.Add(item.Key,item.Value);
                }
            }
            return collection;
        }

        private string  GetName(string type,string content)
        {
            var elements = content.Split(';');
            var element = elements.Where(entry => entry.Trim().StartsWith(type)).FirstOrDefault()?.Trim();
            var name = element?.Substring(type.Length);
            if (!string.IsNullOrEmpty(name) && name.Length >= 2 && name[0] == '"' && name[name.Length - 1] == '"')
            {
                name = name.Substring(1, name.Length - 2);
            }
            return name;
        }

        private string GetRoutePath(string path)
        {
            string routePath = "";
            var urlSpan = path.AsSpan();
            var len = urlSpan.IndexOf("?");
            if (len == -1)
                routePath = urlSpan.TrimStart("/").ToString().ToLower();
            else
                routePath = urlSpan.Slice(0, len).TrimStart("/").ToString().ToLower();
            return routePath;
        }
    }
}
