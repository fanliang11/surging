using Application.Interface.Org;
using DTO.Core;
using Surging.Core.Caching;
using Surging.Core.CPlatform.Ioc;
using Surging.Core.ProxyGenerator;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Domain.Org.Entity;

namespace Application.Service.Org
{
    public partial class OrgAppService
    {
        public Task<BaseTreeResponseDto> FindCorps(BaseTreeSearchReq req)
        {
            BaseTreeResponseDto rsp = new BaseTreeResponseDto();
            rsp.Tree = _queryCorpRepository.Get(a => !a.IsDelete).Select(a => new BaseTreeDto
            {
                Id = a.CorporationKeyId.ToString(),
                Name = a.Name,
                PId = Guid.Empty.ToString()
            }).ToList();

            return Task.FromResult(rsp);
        }

        public Task<BaseTreeResponseDto> FindDepartments(BaseTreeSearchReq req)
        {
            BaseTreeResponseDto rsp = new BaseTreeResponseDto();
            var rows = _queryOrgRepository.Get(a => !a.IsDelete && a.CorporationKeyId == req.Identify.CorporationKeyId).Select(a => new BaseTreeDto
            {
                Id = a.CorporationKeyId.ToString(),
                Name = a.Name,
                PId = a.ParentKeyId.ToString()
            }).ToList();
            rsp.Tree = rows;
            return Task.FromResult(rsp);
        }

        public Task<BaseListResponseDto> QueryUserPermission(CommonCMDReq req)
        {
            var rsp = new BaseListResponseDto();
            var rows = _queryEmployeeRepository.FindUserRolePermission(Guid.Parse(req.Identify.Token));
            rsp.Result = new
            {
                pages = rows.Where(a => a.PermissionType == 1)?.Select(a => new
                {
                    name = a.Name,
                    no = a.No,
                }),
                actions = rows.Where(a => a.PermissionType == 2)?.Select(a => new
                {
                    name = a.Name,
                    no = a.No,
                })
            };
            rsp.OperateFlag = true;
            return Task.FromResult(rsp);

        }


        public Task<BaseListResponseDto> FindCorpRoles(CommonCMDReq req)
        {
            var rsp = new BaseListResponseDto();
            var rows = _queryCorpRepository.Get(a=>a.CorporationKeyId== req.Identify.CorporationKeyId,a=>a.CorpRoles).FirstOrDefault()?.CorpRoles.ToList();
            rsp.Result = rows?.Select(a=>new { roleName=a.Name,roleId=a.KeyId }).ToList();
            rsp.OperateFlag = true;
            return Task.FromResult(rsp);

        }

        public Task<BaseListResponseDto> FindEmployeePageBy(BasePagedRequestDto req)
        {
            var rsp = new BaseListResponseDto();
            var rows = _queryEmployeeRepository.GetByPagination(a => !a.IsDelete&&a.CorporationKeyId==req.Identify.CorporationKeyId, req.PageSize, req.PageIndex,null).ToList();
            rsp.Result = rows;
            rsp.OperateFlag = true;
            return Task.FromResult(rsp);
        }
    }
}
