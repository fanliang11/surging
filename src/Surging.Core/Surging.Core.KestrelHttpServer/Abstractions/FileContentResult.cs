using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic; 
using System.Text;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;
using System.IO;
using Surging.Core.KestrelHttpServer.Internal;

namespace Surging.Core.KestrelHttpServer
{
   public class FileContentResult : FileResult
    {
        private byte[] _fileContents;
        protected const int BufferSize = 64 * 1024;

        public FileContentResult(byte[] fileContents, string contentType)
            : this(fileContents, MediaTypeHeaderValue.Parse(contentType))
        {
            if (fileContents == null)
            {
                throw new ArgumentNullException(nameof(fileContents));
            }
        }

        public FileContentResult(byte[] fileContents, string contentType,string fileDownloadName)
      : this(fileContents, MediaTypeHeaderValue.Parse(contentType))
        {
            if (fileContents == null)
            {
                throw new ArgumentNullException(nameof(fileContents));
            }
            if (fileDownloadName == null)
            {
                throw new ArgumentNullException(nameof(fileDownloadName));
            }
            this.FileDownloadName = fileDownloadName;
        }

        public FileContentResult(byte[] fileContents, MediaTypeHeaderValue contentType)
            : base(contentType?.ToString())
        {
            if (fileContents == null)
            {
                throw new ArgumentNullException(nameof(fileContents));
            }

            FileContents = fileContents;
        }
        
        public byte[] FileContents
        {
            get => _fileContents;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _fileContents = value;
            }
        }

        public override async Task ExecuteResultAsync(ActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            try
            {
                var contentDisposition = new ContentDispositionHeaderValue("attachment");
                contentDisposition.SetHttpFileName(FileDownloadName);
                var httpResponse = context.HttpContext.Response;

                httpResponse.Headers.Add("Content-Type", this.ContentType);
                httpResponse.Headers.Add("Content-Length", FileContents.Length.ToString());
                httpResponse.Headers[HeaderNames.ContentDisposition] = contentDisposition.ToString();
                using (var stream = new MemoryStream(FileContents))
                    await StreamCopyOperation.CopyToAsync(stream, httpResponse.Body, count: null, bufferSize: BufferSize, cancel: context.HttpContext.RequestAborted);
            }
            catch (OperationCanceledException)
            {
                context.HttpContext.Abort();
            }
        }
    }
}
