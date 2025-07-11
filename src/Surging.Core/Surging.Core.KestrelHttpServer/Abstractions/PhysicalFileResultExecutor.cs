using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Surging.Core.KestrelHttpServer.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.KestrelHttpServer.Abstractions
{
   public class PhysicalFileResultExecutor : FileResultExecutorBase
    {
        protected const int BufferSize = 64 * 1024;

        public PhysicalFileResultExecutor(ILoggerFactory loggerFactory)
            : base(CreateLogger<PhysicalFileResultExecutor>(loggerFactory))
        {
        }

        public Task ExecuteAsync(ActionContext context, PhysicalFileResult result)
        {
            SetHeadersAndLog(context, result);
            return WriteFileAsync(context, result);
        }

        private async Task WriteFileAsync(ActionContext context, PhysicalFileResult result)
        {
            var response = context.HttpContext.Response;

            if (!Path.IsPathRooted(result.FileName))
            {
                throw new NotSupportedException($"{result.FileName}Format File Result Path Not Rooted");
            }

            using (var fileStream = GetFileStream(result.FileName))
            {

                await fileStream.CopyToAsync(response.Body, BufferSize);
            }
        }

        protected virtual Stream GetFileStream(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return new FileStream(
                    path,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite,
                    BufferSize,
                    FileOptions.Asynchronous | FileOptions.SequentialScan);
        }
    }
}