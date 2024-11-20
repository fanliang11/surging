using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Text.Json;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Surging.Tools.Cli.Utilities;
using System.IO;

namespace Surging.Tools.Cli.Internal.Http
{
    public class HttpClientProvider : IHttpClientProvider
    {
        private readonly IHttpClientFactory _clientFactory;
        public HttpClientProvider(IHttpClientFactory httpClientFactory)
        {
            _clientFactory = httpClientFactory;
        }

        public async Task<T> DeleteJsonMessageAsync<T>(string requestUri, string message, IDictionary<string, string> headers)
        {
            var json = JsonSerializer.SerializeToUtf8Bytes(message);
            var content = new ByteArrayContent(json);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var request = new HttpRequestMessage(HttpMethod.Delete, $"{requestUri}")
            {
                Content = content,

            };
            foreach (var header in headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }
            var httpClient = _clientFactory.CreateClient();
            var response = await httpClient.SendAsync(request);
            json = await response.Content.ReadAsByteArrayAsync(Encoding.UTF8);
            return JsonSerializer.Deserialize<T>(json);
        }

        public async Task<T> GetJsonMessageAsync<T>(string requestUri,  IDictionary<string, string> headers)
        {  
            var request = new HttpRequestMessage(HttpMethod.Get, $"{requestUri}");
            foreach (var header in headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }
            var httpClient = _clientFactory.CreateClient();
            var response = await httpClient.SendAsync(request);
            var json = await response.Content.ReadAsByteArrayAsync(Encoding.UTF8);
            return JsonSerializer.Deserialize<T>(json);
        }

        public async Task<T> PostJsonMessageAsync<T>(string requestUri, string message, IDictionary<string,string> headers)
        {
            var json = JsonSerializer.SerializeToUtf8Bytes(message);
            var content = new ByteArrayContent(json);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, $"{requestUri}")
            {
                Content = content, 
               
            };
            foreach(var header in headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }
            var httpClient = _clientFactory.CreateClient();
            var response = await httpClient.SendAsync(request); 
            json = await response.Content.ReadAsByteArrayAsync(Encoding.UTF8); ;
            return JsonSerializer.Deserialize<T>(json);
        }

        public async Task<T> PutJsonMessageAsync<T>(string requestUri, string message, IDictionary<string, string> headers)
        {
            var json = JsonSerializer.SerializeToUtf8Bytes(message);
            var content = new ByteArrayContent(json);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var request = new HttpRequestMessage(HttpMethod.Put, $"{requestUri}")
            {
                Content = content,

            };
            foreach (var header in headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }
            var httpClient = _clientFactory.CreateClient();
            var response = await httpClient.SendAsync(request);
            json = await response.Content.ReadAsByteArrayAsync(Encoding.UTF8);
            return JsonSerializer.Deserialize<T>(json);
        }

        public async Task<T> UploadFileAsync<T>(string requestUri,Dictionary<string,string> formData, IDictionary<string, string> headers)
        {
            if(!formData.ContainsKey("filename"))
                throw new ArgumentNullException("filename");
            var fileName = formData["filename"];
            if (File.Exists(fileName))
            {
                using (var fileStream = File.OpenRead(fileName))
                {
                    var content = new MultipartFormDataContent();
                    content.Add(new StreamContent(fileStream, (int)fileStream.Length), "file", Path.GetFileName(fileName));
                    foreach (var form in formData)
                    {
                        if (form.Key != "type" && form.Key != "filename")
                            content.Add(new StringContent(form.Value), form.Key);
                    }
                    var request = new HttpRequestMessage(HttpMethod.Post, $"{requestUri}")
                    {
                        Content = content,
                    };
                    foreach (var header in headers)
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }
                    var httpClient = _clientFactory.CreateClient();
                    var response = await httpClient.SendAsync(request);
                    var json = await response.Content.ReadAsByteArrayAsync(Encoding.UTF8);
                    return JsonSerializer.Deserialize<T>(json);
                }
            }
            else
                throw new FileNotFoundException($"{fileName} not Found");
        }
    }
}
