using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Swagger
{
    /// <summary>
    /// Defines the <see cref="SwaggerMiddleware" />
    /// </summary>
    public class SwaggerMiddleware
    {
        #region 字段

        /// <summary>
        /// Defines the _next
        /// </summary>
        private readonly RequestDelegate _next;

        /// <summary>
        /// Defines the _options
        /// </summary>
        private readonly SwaggerOptions _options;

        /// <summary>
        /// Defines the _requestMatcher
        /// </summary>
        private readonly TemplateMatcher _requestMatcher;

        /// <summary>
        /// Defines the _swaggerSerializer
        /// </summary>
        private readonly JsonSerializer _swaggerSerializer;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="SwaggerMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next<see cref="RequestDelegate"/></param>
        /// <param name="mvcJsonOptions">The mvcJsonOptions<see cref="IOptions{MvcJsonOptions}"/></param>
        /// <param name="options">The options<see cref="SwaggerOptions"/></param>
        public SwaggerMiddleware(
            RequestDelegate next,
            IOptions<MvcJsonOptions> mvcJsonOptions,
            SwaggerOptions options)
        {
            _next = next;
            _swaggerSerializer = SwaggerSerializerFactory.Create(mvcJsonOptions);
            _options = options ?? new SwaggerOptions();
            _requestMatcher = new TemplateMatcher(TemplateParser.Parse(options.RouteTemplate), new RouteValueDictionary());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SwaggerMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next<see cref="RequestDelegate"/></param>
        /// <param name="mvcJsonOptionsAccessor">The mvcJsonOptionsAccessor<see cref="IOptions{MvcJsonOptions}"/></param>
        /// <param name="optionsAccessor">The optionsAccessor<see cref="IOptions{SwaggerOptions}"/></param>
        public SwaggerMiddleware(
            RequestDelegate next,
            IOptions<MvcJsonOptions> mvcJsonOptionsAccessor,
            IOptions<SwaggerOptions> optionsAccessor)
            : this(next, mvcJsonOptionsAccessor, optionsAccessor.Value)
        {
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The Invoke
        /// </summary>
        /// <param name="httpContext">The httpContext<see cref="HttpContext"/></param>
        /// <param name="swaggerProvider">The swaggerProvider<see cref="ISwaggerProvider"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task Invoke(HttpContext httpContext, ISwaggerProvider swaggerProvider)
        {
            if (!RequestingSwaggerDocument(httpContext.Request, out string documentName))
            {
                await _next(httpContext);
                return;
            }

            var basePath = string.IsNullOrEmpty(httpContext.Request.PathBase)
                ? null
                : httpContext.Request.PathBase.ToString();

            try
            {
                var swagger = swaggerProvider.GetSwagger(documentName, null, basePath);

                // One last opportunity to modify the Swagger Document - this time with request context
                foreach (var filter in _options.PreSerializeFilters)
                {
                    filter(swagger, httpContext.Request);
                }

                await RespondWithSwaggerJson(httpContext.Response, swagger);
            }
            catch (UnknownSwaggerDocument)
            {
                RespondWithNotFound(httpContext.Response);
            }
        }

        /// <summary>
        /// The RequestingSwaggerDocument
        /// </summary>
        /// <param name="request">The request<see cref="HttpRequest"/></param>
        /// <param name="documentName">The documentName<see cref="string"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private bool RequestingSwaggerDocument(HttpRequest request, out string documentName)
        {
            documentName = null;
            if (request.Method != "GET") return false;

            var routeValues = new RouteValueDictionary();
            if (!_requestMatcher.TryMatch(request.Path, routeValues) || !routeValues.ContainsKey("documentName")) return false;

            documentName = routeValues["documentName"].ToString();
            return true;
        }

        /// <summary>
        /// The RespondWithNotFound
        /// </summary>
        /// <param name="response">The response<see cref="HttpResponse"/></param>
        private void RespondWithNotFound(HttpResponse response)
        {
            response.StatusCode = 404;
        }

        /// <summary>
        /// The RespondWithSwaggerJson
        /// </summary>
        /// <param name="response">The response<see cref="HttpResponse"/></param>
        /// <param name="swagger">The swagger<see cref="SwaggerDocument"/></param>
        /// <returns>The <see cref="Task"/></returns>
        private async Task RespondWithSwaggerJson(HttpResponse response, SwaggerDocument swagger)
        {
            response.StatusCode = 200;
            response.ContentType = "application/json;charset=utf-8";

            var jsonBuilder = new StringBuilder();
            using (var writer = new StringWriter(jsonBuilder))
            {
                _swaggerSerializer.Serialize(writer, swagger);
                await response.WriteAsync(jsonBuilder.ToString(), new UTF8Encoding(false));
            }
        }

        #endregion 方法
    }
}