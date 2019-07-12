using System;

namespace Surging.Core.CPlatform.Runtime.Server.Implementation
{
    /// <summary>
    /// Defines the <see cref="ServiceTokenGenerator" />
    /// </summary>
    public class ServiceTokenGenerator : IServiceTokenGenerator
    {
        #region 字段

        /// <summary>
        /// Defines the _serviceToken
        /// </summary>
        public string _serviceToken;

        #endregion 字段

        #region 方法

        /// <summary>
        /// The GeneratorToken
        /// </summary>
        /// <param name="code">The code<see cref="string"/></param>
        /// <returns>The <see cref="string"/></returns>
        public string GeneratorToken(string code)
        {
            bool enableToken;
            if (!bool.TryParse(code, out enableToken))
            {
                _serviceToken = code;
            }
            else
            {
                if (enableToken) _serviceToken = Guid.NewGuid().ToString("N");
                else _serviceToken = null;
            }
            return _serviceToken;
        }

        /// <summary>
        /// The GetToken
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public string GetToken()
        {
            return _serviceToken;
        }

        #endregion 方法
    }
}