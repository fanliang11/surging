using Liuxd.IModuleServices.System;
using Surging.Core.System.Ioc;
using System;
using Liuxd.IModuleServices.System.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Liuxd.ModuleServices.System
{
    [ModuleName("System")]
    public class SystemService : ISystemService
    {
        #region Implementation of ISystemService

        public Task<List<SysParameter>> GetSysConfigs(string category)
        {
            return Task.FromResult(new List<SysParameter>()
            {
                new SysParameter
                {
                     Code="001",
                      Id="1",
                       ParentId="0",
                        value="1"
                },
                 new SysParameter
                {
                     Code="002",
                      Id="2",
                       ParentId="0",
                        value="2"
                }
            });

        }

        public Task<bool> SetSysConfig(SysParameter sysParameter)
        {
            return Task.FromResult(true);
        }

        public Task<bool> SetSysConfigs(ICollection<SysParameter> sysParameters)
        {
            return Task.FromResult(true);
        }
        #endregion
    }
}
