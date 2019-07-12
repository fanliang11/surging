using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using Surging.IModuleServices.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.IModuleServices.Common
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IRoteMangeService" />
    /// </summary>
    [ServiceBundle("Api/{Service}")]
    public interface IRoteMangeService
    {
        #region 方法

        /// <summary>
        /// The GetServiceById
        /// </summary>
        /// <param name="serviceId">The serviceId<see cref="string"/></param>
        /// <returns>The <see cref="Task{UserModel}"/></returns>
        Task<UserModel> GetServiceById(string serviceId);

        /// <summary>
        /// The SetRote
        /// </summary>
        /// <param name="model">The model<see cref="RoteModel"/></param>
        /// <returns>The <see cref="Task{bool}"/></returns>
        Task<bool> SetRote(RoteModel model);

        #endregion 方法
    }

    #endregion 接口
}