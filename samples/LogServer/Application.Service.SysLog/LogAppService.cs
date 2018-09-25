using Application.Interface.SysLog; 
using Surging.Core.ProxyGenerator;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Service.SysLog
{
    public class LogAppService : ProxyServiceBase, ILogAppService
    {
        public Task<List<LogListViewDto>> PagedQuery(int pageIndex, int pageSize)
        {
            throw new NotImplementedException();
        }
    }
}
