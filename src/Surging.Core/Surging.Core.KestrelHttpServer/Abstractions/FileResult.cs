using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.KestrelHttpServer
{
    /// <summary>
    /// Defines the <see cref="FileResult" />
    /// </summary>
    public abstract class FileResult : ActionResult
    {
        #region 字段

        /// <summary>
        /// Defines the _fileDownloadName
        /// </summary>
        private string _fileDownloadName;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="FileResult"/> class.
        /// </summary>
        /// <param name="contentType">The contentType<see cref="string"/></param>
        protected FileResult(string contentType)
        {
            if (contentType == null)
            {
                throw new ArgumentNullException(nameof(contentType));
            }

            ContentType = contentType;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the ContentType
        /// </summary>
        public string ContentType { get; }

        /// <summary>
        /// Gets or sets the FileDownloadName
        /// </summary>
        public string FileDownloadName
        {
            get { return _fileDownloadName ?? string.Empty; }
            set { _fileDownloadName = value; }
        }

        #endregion 属性
    }
}