using System;

namespace Surging.Core.Swagger
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="ISwaggerProvider" />
    /// </summary>
    public interface ISwaggerProvider
    {
        #region 方法

        /// <summary>
        /// The GetSwagger
        /// </summary>
        /// <param name="documentName">The documentName<see cref="string"/></param>
        /// <param name="host">The host<see cref="string"/></param>
        /// <param name="basePath">The basePath<see cref="string"/></param>
        /// <param name="schemes">The schemes<see cref="string[]"/></param>
        /// <returns>The <see cref="SwaggerDocument"/></returns>
        SwaggerDocument GetSwagger(
            string documentName,
            string host = null,
            string basePath = null,
            string[] schemes = null);

        #endregion 方法
    }

    #endregion 接口

    /// <summary>
    /// Defines the <see cref="UnknownSwaggerDocument" />
    /// </summary>
    public class UnknownSwaggerDocument : Exception
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownSwaggerDocument"/> class.
        /// </summary>
        /// <param name="documentName">The documentName<see cref="string"/></param>
        public UnknownSwaggerDocument(string documentName)
            : base(string.Format("Unknown Swagger document - {0}", documentName))
        {
        }

        #endregion 构造函数
    }
}