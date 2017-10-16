using System;
using Surging.Core.Caching;
using Surging.Core.CPlatform.EventBus.Events;
using Surging.Core.CPlatform.Filters.Implementation;
using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using Surging.Core.CPlatform.Support;
using Surging.Core.CPlatform.Support.Attributes;
using Surging.Core.ProxyGenerator.Implementation;
using Surging.Core.System.Intercept;
using System.Threading.Tasks;
using System.Collections.Generic;
using Liuxd.IModuleServices.System.DTO;

namespace Liuxd.IModuleServices.System
{
    /// <summary>
    /// 系统参数设置（系统和业务）
    /// </summary>
    [ServiceBundle]
    public interface ISystemService
    {
        [Service(Date = "2017-10-11", Director = "刘旭东", Name = "设置单个系统配置")]
        Task<bool> SetSysConfig(SysParameter sysParameter);

        [Service(Date = "2017-10-11", Director = "刘旭东", Name = "设置多个系统配置")]
        Task<bool> SetSysConfigs(ICollection<SysParameter> sysParameters);

        [Service(Date = "2017-10-11", Director = "刘旭东", Name = "查询类别下的所有参数")]
        Task<List<SysParameter>> GetSysConfigs(string category);
    }
}
