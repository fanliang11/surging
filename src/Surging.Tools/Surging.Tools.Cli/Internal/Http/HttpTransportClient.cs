using McMaster.Extensions.CommandLineUtils;
using Surging.Core.CPlatform.Messages;
using Surging.Tools.Cli.Commands;
using Surging.Tools.Cli.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Tools.Cli.Internal.Http
{
    public class HttpTransportClient : ITransportClient
    { 
        private readonly CommandLineApplication<CurlCommand> _app;
        private readonly IHttpClientProvider _httpClientProvider;
        public HttpTransportClient(CommandLineApplication app, IHttpClientProvider httpClientProvider)
        {
            _app = app as CommandLineApplication<CurlCommand>;
            _httpClientProvider = httpClientProvider; 
        }

        public async Task<RemoteInvokeResultMessage> SendAsync(CancellationToken cancellationToken)
        {
            var command = _app.Model;
            var httpMessage = new HttpResultMessage<Object>();
            switch (command.Method.ToLower())
            {
                case "post":
                    {
                        var formData = command.FormData.ToDictionary();
                        if (formData.ContainsKey("type") && formData["type"] == "application/octet-stream")
                        {
                            httpMessage = await _httpClientProvider.UploadFileAsync<HttpResultMessage<Object>>(command.Address, command.FormData.ToDictionary(), command.Header.ToDictionary());
                        }
                        else
                            httpMessage = await _httpClientProvider.PostJsonMessageAsync<HttpResultMessage<Object>>(command.Address, command.Data, command.Header.ToDictionary());
                        break;
                    }
                case "get":
                    {
                        httpMessage = await _httpClientProvider.GetJsonMessageAsync<HttpResultMessage<Object>>(command.Address, command.Header.ToDictionary());
                        break;
                    }
                case "put":
                    {
                        httpMessage = await _httpClientProvider.PutJsonMessageAsync<HttpResultMessage<Object>>(command.Address, command.Data, command.Header.ToDictionary());
                        break;
                    }
                case "delete":
                    {
                        httpMessage = await _httpClientProvider.DeleteJsonMessageAsync<HttpResultMessage<Object>>(command.Address, command.Data, command.Header.ToDictionary());
                        break;
                    }

            }
            return new RemoteInvokeResultMessage
            {
                ExceptionMessage = httpMessage.Message,
                Result = httpMessage.Entity,
                StatusCode = httpMessage.StatusCode
            };
        }
    }
}
