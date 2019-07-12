using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.KestrelHttpServer.Internal
{
    /// <summary>
    /// Defines the <see cref="HttpFormFile" />
    /// </summary>
    public class HttpFormFile
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpFormFile"/> class.
        /// </summary>
        /// <param name="length">The length<see cref="long"/></param>
        /// <param name="name">The name<see cref="string"/></param>
        /// <param name="fileName">The fileName<see cref="string"/></param>
        /// <param name="file">The file<see cref="byte[]"/></param>
        public HttpFormFile(long length, string name, string fileName, byte[] file)
        {
            Length = length;
            Name = name;
            FileName = fileName;
            File = file;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the File
        /// </summary>
        public byte[] File { get; }

        /// <summary>
        /// Gets the FileName
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Gets the Length
        /// </summary>
        public long Length { get; }

        /// <summary>
        /// Gets the Name
        /// </summary>
        public string Name { get; }

        #endregion 属性
    }
}