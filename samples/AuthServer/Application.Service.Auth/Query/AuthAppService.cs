using DTO.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Service.Auth
{
   public partial class AuthAppService
    {
     public   Task<BaseListResponseDto> FindDomainPermissions(CommonCMDReq req)
        {
            BaseListResponseDto rsp = new BaseListResponseDto();
           var rows= _querySubDomainRepository.Get(a => !a.IsDelete,b=>b.SubDomainPermissions).ToList();
            rsp.Result = rows.Select(a => new {
                keyId = a.KeyId,
                domainNmae = a.Name,
                action = a.SubDomainPermissions.Where(b=>b.PermissionType== Domain.Auth.Entity.PermissionCategory.Action)?.GroupBy(b => b.GroupName, c => new {
                    name=c.Name,
                    no=c.No
                }).ToDictionary(b=>b.Key,c=>c),
                page = a.SubDomainPermissions.Where(b => b.PermissionType == Domain.Auth.Entity.PermissionCategory.Page)?.Select(c => new {
                    name = c.Name,
                    no = c.No
                }).ToList()
            }).ToList();
            return Task.FromResult(rsp);
        }

    }
}
