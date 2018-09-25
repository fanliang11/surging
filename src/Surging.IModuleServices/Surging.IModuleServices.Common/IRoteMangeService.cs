using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using Surging.IModuleServices.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.IModuleServices.Common
{
    [ServiceBundle("Api/{Service}")]
    public interface IRoteMangeService
    {
        Task<UserModel> GetServiceById(string serviceId);

        Task<bool> SetRote(RoteModel model);
    }
}
