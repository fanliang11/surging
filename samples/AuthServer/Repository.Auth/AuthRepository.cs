using DDD.Core;
using Domain.Auth.Aggregate;
using Microsoft.EntityFrameworkCore;
using Repository.EF.Core;
using Surging.Core.CPlatform.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Repository.Auth
{



    public class UserRepository : BaseImpRepository<User>
    {
        public override User FindBy(Guid key)
        {
            return _set //.Include(a => a.Departments).ThenInclude(a => a.Employees)
                .SingleOrDefault(a => a.KeyId == key);
        }
    }

    public class SubDomainRepository : BaseImpRepository<SubDomain>
    {
        public override SubDomain FindBy(Guid key)
        {
            return _set //.Include(a => a.Departments).ThenInclude(a => a.Employees)
                .SingleOrDefault(a => a.KeyId == key);
        }
    }

    public class UserQueryRepository : BaseImpQueryOnlyRepository<User>
    {
    }

    public class SubDomainQueryRepository : BaseImpQueryOnlyRepository<SubDomain>
    {

        public override IQueryable<SubDomain> Get(Expression<Func<SubDomain, bool>> where = null)
        {
            return _set.Include(a => a.SubDomainPermissions).Where(where);
        }
    }

}
