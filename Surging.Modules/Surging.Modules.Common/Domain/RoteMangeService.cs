using Surging.Core.System.Ioc;
using Surging.IModuleServices.Common;
using Surging.IModuleServices.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Modules.Common.Domain
{
    public class RoteMangeService : IRoteMangeService
    {
        public Task<UserModel> GetServiceById(string serviceId)
        {
            return Task.FromResult(new UserModel());
        }

        public Task<bool> SetRote(RoteModel model)
        {
            return Task.FromResult(true);
        }
    }
}
