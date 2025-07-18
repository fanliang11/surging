using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace Surging.Core.KestrelHttpServer.Abstractions
{
   public  class FileResultExecutorBase
    {
        public FileResultExecutorBase(ILogger logger)
        {
            Logger = logger;
        }

        private readonly ILogger<FileResultExecutorBase> _logger;

        protected ILogger Logger { get; }

        protected    DispositionHeader ContentDispositionHeader { get; set; }


        protected void SetHeadersAndLog(ActionContext context, FileResult result)
        {
            SetContentType(context, result);
            SetContentDispositionHeader(context, result);
            if (Logger.IsEnabled(LogLevel.Information))
                Logger.LogInformation($"Executing FileResult, sending file as {result.FileDownloadName}");
        }

        private void SetContentDispositionHeader(ActionContext context, FileResult result)
        {
            if (!string.IsNullOrEmpty(result.FileDownloadName))
            {
                var contentDisposition = new Microsoft.Net.Http.Headers.ContentDispositionHeaderValue(ContentDispositionHeader.ToString());
                contentDisposition.SetHttpFileName(result.FileDownloadName);
                context.HttpContext.Response.Headers[HeaderNames.ContentDisposition] = contentDisposition.ToString();
            }
        }

        private void SetContentType(ActionContext context, FileResult result)
        {
            var response = context.HttpContext.Response;
            response.ContentType = result.ContentType;
        }

        protected static ILogger CreateLogger<T>(ILoggerFactory factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            return factory.CreateLogger<T>();
        }
    }
}
