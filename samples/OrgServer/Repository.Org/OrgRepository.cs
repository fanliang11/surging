using Microsoft.EntityFrameworkCore;
using Repository.EF.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Domain.Org.Entity;
using Domain.Org.ValueObject;
using System.Data.SqlClient;
using System.Data;
using Domain.Org.Aggregate;

namespace Repository.Org
{
    public class OrgRepository : BaseImpRepository<Corporation>
    {
        public override Corporation FindBy(Guid key)
        {
            return _set.Include(a => a.Departments).ThenInclude(a => a.Employees)
                .Include(a=>a.CorpRoles)
                .SingleOrDefault(a => a.CorporationKeyId == key);
        }
    }

    public class CorpQueryRepository : BaseImpQueryOnlyRepository<Corporation>
    {

    }

    public class OrgQueryRepository : BaseImpQueryOnlyRepository<Department>
    {
    }

    public class EmployeeQueryRepository : BaseImpQueryOnlyRepository<Employee>
    {

        public List<RolePermission> FindUserRolePermission(Guid empId)
        {
            return _dbContext.Set<Employee>().AsNoTracking().Where(a=>a.KeyId==empId).Join<Employee,RolePermission,Guid, RolePermission>(_dbContext.Set<RolePermission>().AsNoTracking(), a => a.RoleKeyId, b => b.CorpRoleKeyId,(a,b)=>b).ToList();
        }
    }

    public class RolePermissionQueryRepository : BaseImpQueryOnlyRepository<RolePermission>
    {
    }
}
