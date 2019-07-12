using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Text;

namespace Surging.Core.KestrelHttpServer.Internal
{
    /// <summary>
    /// Defines the <see cref="FileStreamResult" />
    /// </summary>
    public class FileStreamResult : FileResult
    {
        #region 字段

        /// <summary>
        /// Defines the _fileStream
        /// </summary>
        private Stream _fileStream;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="FileStreamResult"/> class.
        /// </summary>
        /// <param name="fileStream">The fileStream<see cref="Stream"/></param>
        /// <param name="contentType">The contentType<see cref="MediaTypeHeaderValue"/></param>
        public FileStreamResult(Stream fileStream, MediaTypeHeaderValue contentType)
            : base(contentType?.ToString())
        {
            if (fileStream == null)
            {
                throw new ArgumentNullException(nameof(fileStream));
            }

            FileStream = fileStream;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileStreamResult"/> class.
        /// </summary>
        /// <param name="fileStream">The fileStream<see cref="Stream"/></param>
        /// <param name="contentType">The contentType<see cref="string"/></param>
        public FileStreamResult(Stream fileStream, string contentType)
            : this(fileStream, MediaTypeHeaderValue.Parse(contentType))
        {
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the FileStream
        /// </summary>
        public Stream FileStream
        {
            get => _fileStream;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _fileStream = value;
            }
        }

        #endregion 属性
    }
}