using Microsoft.Extensions.Configuration;
using Surging.Core.Caching.Utilities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Surging.Core.Caching.Configurations.Remote
{
    /// <summary>
    /// Defines the <see cref="RemoteConfigurationProvider" />
    /// </summary>
    internal class RemoteConfigurationProvider : ConfigurationProvider
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteConfigurationProvider"/> class.
        /// </summary>
        /// <param name="source">The source<see cref="RemoteConfigurationSource"/></param>
        public RemoteConfigurationProvider(RemoteConfigurationSource source)
        {
            Check.NotNull(source, "source");
            if (!string.IsNullOrEmpty(source.ConfigurationKeyPrefix))
            {
                Check.CheckCondition(() => source.ConfigurationKeyPrefix.Trim().StartsWith(":"), CachingResources.InvalidStartCharacter, "source.ConfigurationKeyPrefix", ":");
                Check.CheckCondition(() => source.ConfigurationKeyPrefix.Trim().EndsWith(":"), CachingResources.InvalidEndCharacter, "source.ConfigurationKeyPrefix", ":");
            }
            Source = source;
            Backchannel = new HttpClient(source.BackchannelHttpHandler ?? new HttpClientHandler());
            Backchannel.DefaultRequestHeaders.UserAgent.ParseAdd("获取CacheConfiugration信息");
            Backchannel.Timeout = source.BackchannelTimeout;
            Backchannel.MaxResponseContentBufferSize = 1024 * 1024 * 10;
            Parser = source.Parser ?? new JsonConfigurationParser();
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the Backchannel
        /// </summary>
        public HttpClient Backchannel { get; }

        /// <summary>
        /// Gets the Parser
        /// </summary>
        public IConfigurationParser Parser { get; }

        /// <summary>
        /// Gets the Source
        /// </summary>
        public RemoteConfigurationSource Source { get; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The Load
        /// </summary>
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
                    throw new Exception(string.Format(CachingResources.HttpException, response.StatusCode, response.ReasonPhrase));
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

        #endregion 方法
    }
}