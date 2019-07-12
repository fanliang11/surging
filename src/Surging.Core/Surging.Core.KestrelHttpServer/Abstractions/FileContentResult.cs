using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Surging.Core.KestrelHttpServer.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.KestrelHttpServer
{
    /// <summary>
    /// Defines the <see cref="FileContentResult" />
    /// </summary>
    public class FileContentResult : FileResult
    {
        #region 常量

        /// <summary>
        /// Defines the BufferSize
        /// </summary>
        protected const int BufferSize = 64 * 1024;

        #endregion 常量

        #region 字段

        /// <summary>
        /// Defines the _fileContents
        /// </summary>
        private byte[] _fileContents;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="FileContentResult"/> class.
        /// </summary>
        /// <param name="fileContents">The fileContents<see cref="byte[]"/></param>
        /// <param name="contentType">The contentType<see cref="MediaTypeHeaderValue"/></param>
        public FileContentResult(byte[] fileContents, MediaTypeHeaderValue contentType)
            : base(contentType?.ToString())
        {
            if (fileContents == null)
            {
                throw new ArgumentNullException(nameof(fileContents));
            }

            FileContents = fileContents;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileContentResult"/> class.
        /// </summary>
        /// <param name="fileContents">The fileContents<see cref="byte[]"/></param>
        /// <param name="contentType">The contentType<see cref="string"/></param>
        public FileContentResult(byte[] fileContents, string contentType)
            : this(fileContents, MediaTypeHeaderValue.Parse(contentType))
        {
            if (fileContents == null)
            {
                throw new ArgumentNullException(nameof(fileContents));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileContentResult"/> class.
        /// </summary>
        /// <param name="fileContents">The fileContents<see cref="byte[]"/></param>
        /// <param name="contentType">The contentType<see cref="string"/></param>
        /// <param name="fileDownloadName">The fileDownloadName<see cref="string"/></param>
        public FileContentResult(byte[] fileContents, string contentType, string fileDownloadName)
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

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the FileContents
        /// </summary>
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

        #endregion 属性

        #region 方法

        /// <summary>
        /// The ExecuteResultAsync
        /// </summary>
        /// <param name="context">The context<see cref="ActionContext"/></param>
        /// <returns>The <see cref="Task"/></returns>
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

        #endregion 方法
    }
}