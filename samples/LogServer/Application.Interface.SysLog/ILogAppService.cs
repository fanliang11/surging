using Surging.Core.CPlatform.Ioc;
using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interface.SysLog
{
    public class LogListViewDto
    {
        /// <summary>
        /// 日志类型
        /// </summary>
        public string LogType { get; set; }

        /// <summary>
        /// 日志类型
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 操作部门
        /// </summary>
        public string OpDept { get; set; }

        /// <summary>
        /// 操作人
        /// </summary>
        public string OpUser { get; set; }
    }
    [ServiceBundle("api/{Service}")]
    public interface ILogAppService : IServiceKey
    {
        /// <summary>
        /// 列表分页查询
        /// </summary>
        /// <returns></returns>
        [Service(Date = "2018-1-20", Director = "刘旭东", Name = "查询日志")]
        //[Authorization(AuthType = AuthorizationType.JWT)]
      //  [BindEvent("修改系统参数的事件|订单支付的事件")]
        Task<List<LogListViewDto>> PagedQuery(int pageIndex,int pageSize);


    }
}
