using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Swagger.Internal
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IServiceSchemaProvider" />
    /// </summary>
    public interface IServiceSchemaProvider
    {
        #region 方法

        /// <summary>
        /// The GetSchemaFilesPath
        /// </summary>
        /// <returns>The <see cref="IEnumerable{string}"/></returns>
        IEnumerable<string> GetSchemaFilesPath();

        #endregion 方法
    }

    #endregion 接口
}