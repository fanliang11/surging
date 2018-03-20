using DTO.Core;
using Surging.Core.CPlatform.Ioc;
using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using Surging.Core.CPlatform.Support;
using Surging.Core.CPlatform.Support.Attributes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interface.Org
{
    [ServiceBundle("api/{Service}")]
    public interface IOrgAppService : IServiceKey
    {
        #region CMD
        #region 公司管理

        /// <summary>
        /// 注册一个新公司
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Service(Date = "2018-2-12", Director = "刘旭东", Name = "注册一个新公司")]
        Task<OperateResultRsp> RegisterCorporation(CorpEditReq req);

        /// <summary>
        /// 激活一个新公司
        /// 手机短信或者邮箱确认
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Service(Date = "2018-2-12", Director = "刘旭东", Name = "激活一个新公司")]
        Task<OperateResultRsp> ActivateCorporation(CommonCMDReq req);

        /// <summary>
        /// 修改公司信息
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Service(Date = "2018-2-12", Director = "刘旭东", Name = "修改公司信息")]
        Task<OperateResultRsp> EditCorporation(CorpEditReq req);
        #endregion

        #region 权限管理

        #endregion

        #endregion


        #region Query

        #region 公司
        /// <summary>
        /// 所有公司列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Service(Date = "2018-1-30", Director = "刘旭东", Name = "所有公司列表")]
        Task<BaseTreeResponseDto> FindCorps(BaseTreeSearchReq req);
        #endregion

        #region 部门管理

        /// <summary>
        /// 当前用户所在公司全部部门列表
        /// 使用场景：部门树加载显示
        /// </summary>
        /// <param name="req"></param>
        /// <returns>全部部门列表</returns>
        //[Command(Strategy = StrategyType.Failover, RequestCacheEnabled = false)]
        [Service(Date = "2018-1-30", Director = "刘旭东", Name = "当前用户所在公司全部部门列表")]
        Task<BaseTreeResponseDto> FindDepartments(BaseTreeSearchReq req);

        /// <summary>
        /// 添加一个新部门
        /// </summary>
        /// <param name="req"></param>
        /// <returns>操作结果</returns>
        [Service(Date = "2018-2-1", Director = "刘旭东", Name = "添加一个新部门")]
        Task<OperateResultRsp> CreateDepartment(DeptEditReq req);

        /// <summary>
        /// 修改一个部门
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Service(Date = "2018-2-1", Director = "刘旭东", Name = "修改一个部门")]
        Task<OperateResultRsp> ModifyDepartment(DeptEditReq req);

        /// <summary>
        /// 删除一个部门
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Service(Date = "2018-2-1", Director = "刘旭东", Name = "删除一个部门")]
        Task<OperateResultRsp> RemoveDepartment(KeyIdReq req);

        #endregion 部门管理

        #region 权限管理
        /// <summary>
        /// 添加一个新角色
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Service(Date = "2018-2-1", Director = "刘旭东", Name = "添加一个新角色")]
        Task<OperateResultRsp> CreateRole(RoleEditReq req);

        /// <summary>
        /// 查询用户的所有权限
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Service(Date = "2018-2-14", Director = "刘旭东", Name = "查询用户的所有权限")]
        Task<BaseListResponseDto> QueryUserPermission(CommonCMDReq req);

        [Service(Date = "2018-2-14", Director = "刘旭东", Name = "查询公司的所有角色")]
        Task<BaseListResponseDto> FindCorpRoles(CommonCMDReq req);

        #endregion

        #region 员工管理
        /// <summary>
        /// 添加一个新员工
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Service(Date = "2018-2-1", Director = "刘旭东", Name = "添加一个新员工")]
        Task<OperateResultRsp> CreateEmployee(EmployeeEditReq req);

        /// <summary>
        /// 分页查询员工
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Service(Date = "2018-2-1", Director = "刘旭东", Name = "分页查询员工")]
        Task<BaseListResponseDto> FindEmployeePageBy(BasePagedRequestDto req);
        #endregion
        #endregion
    }
}
