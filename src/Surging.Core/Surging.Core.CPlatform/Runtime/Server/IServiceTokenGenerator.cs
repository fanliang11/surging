using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Runtime.Server
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IServiceTokenGenerator" />
    /// </summary>
    public interface IServiceTokenGenerator
    {
        #region 方法

        /// <summary>
        /// The GeneratorToken
        /// </summary>
        /// <param name="code">The code<see cref="string"/></param>
        /// <returns>The <see cref="string"/></returns>
        string GeneratorToken(string code);

        /// <summary>
        /// The GetToken
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        string GetToken();

        #endregion 方法
    }

    #endregion 接口
}