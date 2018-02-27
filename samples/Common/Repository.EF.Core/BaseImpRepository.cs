using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DDD.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Surging.Core.CPlatform.Ioc;

namespace Repository.EF.Core
{
    public   class BaseImpRepository<T> : BaseRepository,IRepository<T, Guid> where T : IAggregate
    {
        private readonly string _modelAssemblyName;
        private readonly DefaultDbContext _dbContext;
        protected readonly DbSet<T> _set;
        public BaseImpRepository()//DefaultDbContext dbContext)
        {
            //if (dbContext == null) throw new ArgumentNullException(nameof(dbContext));

            _dbContext = new DefaultDbContext();
            //_dbContext.Database.EnsureCreated();
            _set = _dbContext.Set<T>();
        }

        

        public virtual void Delete(Guid key)
        {
        }

        

        public virtual void Dispose()
        {
            if (_dbContext != null)
            {
                _dbContext.Dispose();
            }
        }

        public virtual T FindBy(Guid key) //where T : IAggregate
        {
            //TODO:想要动态地加载聚合的所有数据 
            return null;// _dbContext.Set<T>().First(a => a.KeyId == key);
        }
     
        public void Add(T aggregate)// where T : IAggregate
        {
          //  using (var _dbContext=new DefaultDbContext(_modelAssemblyName))
           // {
                _dbContext.Set<T>().Add(aggregate);
              //  _dbContext.SaveChanges();
          //  }
        }
        public void Update(T aggregate)// where T : IAggregate
        {
            _dbContext.Set<T>().Update(aggregate);
        }

        public int Commit()
        {
         return   _dbContext.SaveChanges();
        }
    }
}
