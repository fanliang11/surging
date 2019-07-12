using Surging.Core.ProxyGenerator;
using Surging.Core.System.Ioc;
using Surging.IModuleServices.Common;
using Surging.IModuleServices.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Modules.Common.Domain
{
    /// <summary>
    /// Defines the <see cref="RoteMangeService" />
    /// </summary>
    public class RoteMangeService : ProxyServiceBase, IRoteMangeService
    {
        #region 方法

        /// <summary>
        /// The GetServiceById
        /// </summary>
        /// <param name="serviceId">The serviceId<see cref="string"/></param>
        /// <returns>The <see cref="Task{UserModel}"/></returns>
        public Task<UserModel> GetServiceById(string serviceId)
        {
            return Task.FromResult(new UserModel());
        }

        /// <summary>
        /// The SetRote
        /// </summary>
        /// <param name="model">The model<see cref="RoteModel"/></param>
        /// <returns>The <see cref="Task{bool}"/></returns>
        public Task<bool> SetRote(RoteModel model)
        {
            return Task.FromResult(true);
        }

        #endregion 方法
    }
}