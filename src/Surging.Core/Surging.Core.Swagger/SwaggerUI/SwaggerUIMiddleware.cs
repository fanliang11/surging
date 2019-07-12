using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Surging.Core.Swagger.SwaggerUI
{
    /// <summary>
    /// Defines the <see cref="SwaggerUIMiddleware" />
    /// </summary>
    public class SwaggerUIMiddleware
    {
        #region 常量

        /// <summary>
        /// Defines the EmbeddedFileNamespace
        /// </summary>
        private const string EmbeddedFileNamespace = "Surging.Core.Swagger.SwaggerUI.node_modules.swagger_ui_dist";

        #endregion 常量

        #region 字段

        /// <summary>
        /// Defines the _options
        /// </summary>
        private readonly SwaggerUIOptions _options;

        /// <summary>
        /// Defines the _staticFileMiddleware
        /// </summary>
        private readonly StaticFileMiddleware _staticFileMiddleware;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="SwaggerUIMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next<see cref="RequestDelegate"/></param>
        /// <param name="hostingEnv">The hostingEnv<see cref="IHostingEnvironment"/></param>
        /// <param name="loggerFactory">The loggerFactory<see cref="ILoggerFactory"/></param>
        /// <param name="optionsAccessor">The optionsAccessor<see cref="IOptions{SwaggerUIOptions}"/></param>
        public SwaggerUIMiddleware(
            RequestDelegate next,
            IHostingEnvironment hostingEnv,
            ILoggerFactory loggerFactory,
            IOptions<SwaggerUIOptions> optionsAccessor)
            : this(next, hostingEnv, loggerFactory, optionsAccessor.Value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SwaggerUIMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next<see cref="RequestDelegate"/></param>
        /// <param name="hostingEnv">The hostingEnv<see cref="IHostingEnvironment"/></param>
        /// <param name="loggerFactory">The loggerFactory<see cref="ILoggerFactory"/></param>
        /// <param name="options">The options<see cref="SwaggerUIOptions"/></param>
        public SwaggerUIMiddleware(
            RequestDelegate next,
            IHostingEnvironment hostingEnv,
            ILoggerFactory loggerFactory,
            SwaggerUIOptions options)
        {
            _options = options ?? new SwaggerUIOptions();
            _staticFileMiddleware = CreateStaticFileMiddleware(next, hostingEnv, loggerFactory, options);
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The Invoke
        /// </summary>
        /// <param name="httpContext">The httpContext<see cref="HttpContext"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task Invoke(HttpContext httpContext)
        {
            var httpMethod = httpContext.Request.Method;
            var path = httpContext.Request.Path.Value;

            // If the RoutePrefix is requested (with or without trailing slash), redirect to index URL
            if (httpMethod == "GET" && Regex.IsMatch(path, $"^/{_options.RoutePrefix}/?$"))
            {
                // Use relative redirect to support proxy environments
                var relativeRedirectPath = path.EndsWith("/")
                    ? "index.html"
                    : $"{path.Split('/').Last()}/index.html";

                RespondWithRedirect(httpContext.Response, relativeRedirectPath);
                return;
            }

            if (httpMethod == "GET" && Regex.IsMatch(path, $"/{_options.RoutePrefix}/?index.html"))
            {
                await RespondWithIndexHtml(httpContext.Response);
                return;
            }

            await _staticFileMiddleware.Invoke(httpContext);
        }

        /// <summary>
        /// The CreateStaticFileMiddleware
        /// </summary>
        /// <param name="next">The next<see cref="RequestDelegate"/></param>
        /// <param name="hostingEnv">The hostingEnv<see cref="IHostingEnvironment"/></param>
        /// <param name="loggerFactory">The loggerFactory<see cref="ILoggerFactory"/></param>
        /// <param name="options">The options<see cref="SwaggerUIOptions"/></param>
        /// <returns>The <see cref="StaticFileMiddleware"/></returns>
        private StaticFileMiddleware CreateStaticFileMiddleware(
            RequestDelegate next,
            IHostingEnvironment hostingEnv,
            ILoggerFactory loggerFactory,
            SwaggerUIOptions options)
        {
            var staticFileOptions = new StaticFileOptions
            {
                RequestPath = string.IsNullOrEmpty(options.RoutePrefix) ? string.Empty : $"/{options.RoutePrefix}",
                FileProvider = new EmbeddedFileProvider(typeof(SwaggerUIMiddleware).GetTypeInfo().Assembly, EmbeddedFileNamespace),
            };

            return new StaticFileMiddleware(next, hostingEnv, Options.Create(staticFileOptions), loggerFactory);
        }

        /// <summary>
        /// The GetIndexArguments
        /// </summary>
        /// <returns>The <see cref="IDictionary{string, string}"/></returns>
        private IDictionary<string, string> GetIndexArguments()
        {
            return new Dictionary<string, string>()
            {
                { "%(DocumentTitle)", _options.DocumentTitle },
                { "%(HeadContent)", _options.HeadContent },
                { "%(ConfigObject)", SerializeToJson(_options.ConfigObject) },
                { "%(OAuthConfigObject)", SerializeToJson(_options.OAuthConfigObject) }
            };
        }

        /// <summary>
        /// The RespondWithIndexHtml
        /// </summary>
        /// <param name="response">The response<see cref="HttpResponse"/></param>
        /// <returns>The <see cref="Task"/></returns>
        private async Task RespondWithIndexHtml(HttpResponse response)
        {
            response.StatusCode = 200;
            response.ContentType = "text/html";

            using (var stream = _options.IndexStream())
            {
                // Inject arguments before writing to response
                var htmlBuilder = new StringBuilder(new StreamReader(stream).ReadToEnd());
                foreach (var entry in GetIndexArguments())
                {
                    htmlBuilder.Replace(entry.Key, entry.Value);
                }

                await response.WriteAsync(htmlBuilder.ToString(), Encoding.UTF8);
            }
        }

        /// <summary>
        /// The RespondWithRedirect
        /// </summary>
        /// <param name="response">The response<see cref="HttpResponse"/></param>
        /// <param name="location">The location<see cref="string"/></param>
        private void RespondWithRedirect(HttpResponse response, string location)
        {
            response.StatusCode = 301;
            response.Headers["Location"] = location;
        }

        /// <summary>
        /// The SerializeToJson
        /// </summary>
        /// <param name="obj">The obj<see cref="object"/></param>
        /// <returns>The <see cref="string"/></returns>
        private string SerializeToJson(object obj)
        {
            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new[] { new StringEnumConverter(true) },
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.None,
                StringEscapeHandling = StringEscapeHandling.EscapeHtml
            });
        }

        #endregion 方法
    }
}