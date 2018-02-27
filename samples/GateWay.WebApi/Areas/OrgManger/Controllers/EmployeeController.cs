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
    public class EmployeeController : BaseApiController
    {
        private IOrgAppService _orgProxy;

        public EmployeeController()
        {
            _orgProxy = serviceProxyFactory.CreateProxy<IOrgAppService>();
        }


        #region 员工
        [HttpPost]
        public async Task<ServiceResult<object>> Post(EmployeeEditReq req)
        {
            //HttpContext.Request.Headers
            var result = await _orgProxy.CreateEmployee(req);
            return ServiceResult<object>.Create(result.OperateFlag, result.OperateResult);
        }

        [HttpGet("list")]
        public async Task<ServiceResult<BaseListResponseDto>> GetRoleTree(BasePagedRequestDto req)
        {
            var result = await _orgProxy.FindEmployeePageBy(req);
            return ServiceResult<BaseListResponseDto>.Create(true, result);
        }


        #endregion

         
    }
}