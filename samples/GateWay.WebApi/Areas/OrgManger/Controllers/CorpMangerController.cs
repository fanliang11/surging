using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Interface.Org;
using DTO.Core;
using GateWay.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Surging.Core.ApiGateWay;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.ProxyGenerator;

namespace GateWay.WebApi
{
    [Route("api/corp"), CustomAuthorizeFilter]
    public class CorpController : BaseApiController
    {
        private IOrgAppService _orgProxy;

        public CorpController()
        {
            _orgProxy = serviceProxyFactory.CreateProxy<IOrgAppService>();
        }


        #region 公司
        [HttpPost]
        public async Task<ServiceResult<object>> RegisterCorporation(CorpEditReq req)
        {
            var result = await _orgProxy.RegisterCorporation(req);
            return ServiceResult<object>.Create(result.OperateFlag, result.OperateResult);
        }

        [HttpPut("activate")]
        public async Task<ServiceResult<object>> ActivateCorporation(CommonCMDReq req)
        {
            var result = await _orgProxy.ActivateCorporation(req);
            return ServiceResult<object>.Create(result.OperateFlag, result.OperateResult);
        }
        
        [HttpGet("tree")]
        public async Task<ServiceResult<BaseTreeResponseDto>> GetCorpTree(BaseTreeSearchReq req)
        {

            var result = await _orgProxy.FindCorps(req);
            return ServiceResult<BaseTreeResponseDto>.Create(true, result);
        }

        #endregion
        #region 部门管理
        [HttpGet("org/tree")]
        public async Task<ServiceResult<BaseTreeResponseDto>> GetOrgTree(BaseTreeSearchReq req)
        {
           
            var result = await _orgProxy.FindDepartments(req);
            return ServiceResult<BaseTreeResponseDto>.Create(true, result);
        }

        [HttpGet("org/info")]
        public ServiceResult<object> GetInfo(KeyIdReq req)
        {
            return ServiceResult<object>.Create(true, "");
        }

        [HttpPost("org")]
        public async Task<ServiceResult<object>> Post(DeptEditReq req)
        {

            var result = await _orgProxy.CreateDepartment(req);
            return ServiceResult<object>.Create(result.OperateFlag, result.OperateResult);
        }

        [HttpPut("org")]
        public async Task<ServiceResult<object>> Put(DeptEditReq req)
        {
         
            var result = await _orgProxy.ModifyDepartment(req);
            return ServiceResult<object>.Create(result.OperateFlag, result.OperateResult);
        }

        [HttpDelete("org")]
        public async Task<ServiceResult<object>> Delete(KeyIdReq req)
        {
            var result = await _orgProxy.RemoveDepartment(req);
            return ServiceResult<object>.Create(result.OperateFlag, result.OperateResult);
        }

        #endregion

    }
}
