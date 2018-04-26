using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Interface.Org;
using DTO.Core;
using GateWay.WebApi.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Surging.Core.ApiGateWay;

namespace GateWay.WebApi.Areas.OrgManger.Controllers
{
    //[Produces("application/json")]
    [Route("api/[controller]")]
    public class RoleController : BaseApiController
    {
        private IOrgAppService _orgProxy;

        public RoleController()
        {
            _orgProxy = serviceProxyFactory.CreateProxy<IOrgAppService>();
        }


        #region 角色
        [HttpPost]
        public async Task<ServiceResult<object>> Post(RoleEditReq req)
        {
            var result = await _orgProxy.CreateRole(req);
            return ServiceResult<object>.Create(result.OperateFlag, result.OperateResult);
        }

        [HttpGet("list")]
        public async Task<ServiceResult<BaseListResponseDto>> GetRoleTree(CommonCMDReq req)
        {
            var result = await _orgProxy.FindCorpRoles(req);
            return ServiceResult<BaseListResponseDto>.Create(true, result);
        }


        #endregion

        #region 角色包含的权限

        #endregion
    }
}