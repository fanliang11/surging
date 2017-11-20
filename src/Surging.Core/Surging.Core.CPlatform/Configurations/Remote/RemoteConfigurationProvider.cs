using Microsoft.Extensions.Configuration;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Surging.Core.CPlatform.Configurations.Remote
{
    class RemoteConfigurationProvider : ConfigurationProvider
    {
        public RemoteConfigurationProvider(RemoteConfigurationSource source)
        {
            Check.NotNull(source, "source");
            if (!string.IsNullOrEmpty(source.ConfigurationKeyPrefix))
            {
                Check.CheckCondition(() => source.ConfigurationKeyPrefix.Trim().StartsWith(":"), CPlatformResource.InvalidStartCharacter, "source.ConfigurationKeyPrefix", ":");
                Check.CheckCondition(() => source.ConfigurationKeyPrefix.Trim().EndsWith(":"), CPlatformResource.InvalidEndCharacter, "source.ConfigurationKeyPrefix",":");
            }
            Source = source;
            Backchannel = new HttpClient(source.BackchannelHttpHandler ?? new HttpClientHandler());
            Backchannel.DefaultRequestHeaders.UserAgent.ParseAdd("获取CacheConfiugration信息");
            Backchannel.Timeout = source.BackchannelTimeout;
            Backchannel.MaxResponseContentBufferSize = 1024 * 1024 * 10; 
            Parser = source.Parser ?? new JsonConfigurationParser();
        }

        public RemoteConfigurationSource Source { get; }

        public IConfigurationParser Parser { get; }

        public HttpClient Backchannel { get; }
        
        public override void Load()
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, Source.ConfigurationUri);
            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Source.MediaType));
            Source.Events.SendingRequest(requestMessage);
            try
            {
                var response = Backchannel.SendAsync(requestMessage)
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult();

                if (response.IsSuccessStatusCode)
                {
                    using (var stream = response.Content.ReadAsStreamAsync()
                        .ConfigureAwait(false)
                        .GetAwaiter()
                        .GetResult())
                    {
                        var data = Parser.Parse(stream, Source.ConfigurationKeyPrefix?.Trim());
                        Data = Source.Events.DataParsed(data);
                    }
                }
                else if (!Source.Optional)
                {
                    throw new Exception(string.Format(CPlatformResource.HttpException, response.StatusCode, response.ReasonPhrase));
                }
            }
            catch (Exception)
            {
                if (!Source.Optional)
                {
                    throw;
                }
            }
        }
    }
}
