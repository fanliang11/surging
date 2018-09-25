using DDD.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Repository.Dapper.Core
{
    public class BaseImpRepository<T> : IRepository<T, Guid> where T : IAggregate
    {
        public void Add(T aggregate)
        {
            throw new NotImplementedException();
        }

        public int Commit()
        {
            throw new NotImplementedException();
        }

        public void Delete(Guid key)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public T FindBy(Guid key)
        {
            throw new NotImplementedException();
        }

        public void Update(T aggregate)
        {
            throw new NotImplementedException();
        }
    }
}
