using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.ApiGateWay.OAuth
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IAuthorizationServerProvider" />
    /// </summary>
    public interface IAuthorizationServerProvider
    {
        #region 方法

        /// <summary>
        /// The GenerateTokenCredential
        /// </summary>
        /// <param name="parameters">The parameters<see cref="Dictionary{string, object}"/></param>
        /// <returns>The <see cref="Task{string}"/></returns>
        Task<string> GenerateTokenCredential(Dictionary<string, object> parameters);

        /// <summary>
        /// The GetPayloadString
        /// </summary>
        /// <param name="token">The token<see cref="string"/></param>
        /// <returns>The <see cref="string"/></returns>
        string GetPayloadString(string token);

        /// <summary>
        /// The ValidateClientAuthentication
        /// </summary>
        /// <param name="token">The token<see cref="string"/></param>
        /// <returns>The <see cref="Task{bool}"/></returns>
        Task<bool> ValidateClientAuthentication(string token);

        #endregion 方法
    }

    #endregion 接口
}