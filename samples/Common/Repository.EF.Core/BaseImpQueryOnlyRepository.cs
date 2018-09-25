using DDD.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Surging.Core.CPlatform.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Repository.EF.Core
{
   public class BaseImpQueryOnlyRepository<T>: BaseRepository,IQueryOnlyRepository<T,Guid> where T : class, IDBModel<Guid>
    {
        protected readonly DefaultDbContext _dbContext;
        protected readonly DbSet<T> _set;
        public BaseImpQueryOnlyRepository()//DefaultDbContext dbContext)
        {
            //if (dbContext == null) throw new ArgumentNullException(nameof(dbContext));

            _dbContext = new DefaultDbContext();
            _set = _dbContext.Set<T>();
        }

      

        #region 所有跟添加/修改/删除无关的界面查询，也不包含报表查询

        public virtual int Count(Expression<Func<T, bool>> @where = null)
        {
            return where == null ? _set.Count() : _set.Count(@where);
        }
         
        public virtual bool Exist(Expression<Func<T, bool>> @where = null)
        {
            return Get(where).Any();
        }

        public virtual bool Exist(Expression<Func<T, bool>> @where = null, params Expression<Func<T, object>>[] includes)
        {
            return Get(where, includes).Any();
        }

        public virtual int ExecuteSqlWithNonQuery(string sql, params object[] parameters)
        {
            return _dbContext.ExecuteSqlWithNonQuery(sql, parameters);
        }

        public virtual T GetSingle(Guid key)
        {
            return _set.Find(key);
        }

        public T GetSingle(Guid key, params Expression<Func<T, object>>[] includes)
        {
            if (includes == null) return GetSingle(key);
            var query = _set.AsQueryable();
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            Expression<Func<T, bool>> filter = m => m.KeyId.Equals(key);
            return query.SingleOrDefault(filter.Compile());
        }

        public T GetSingle(Expression<Func<T, bool>> @where = null)
        {
            if (where == null) return _set.SingleOrDefault();
            return _set.SingleOrDefault(@where);
        }

        public T GetSingle(Expression<Func<T, bool>> @where = null, params Expression<Func<T, object>>[] includes)
        {
            if (includes == null) return GetSingle(where);
            var query = _set.AsQueryable();
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            if (where == null) return query.SingleOrDefault();
            return query.SingleOrDefault(where);
        }

        public virtual IQueryable<T> Get(Expression<Func<T, bool>> @where = null)
        {
            return @where != null ? _set.AsNoTracking().Where(@where) : _set.AsNoTracking();
        }
     

        public virtual IQueryable<T> Get(Expression<Func<T, bool>> @where = null, params Expression<Func<T, object>>[] includes)
        {
            if (includes == null)
                return Get(where);
            var query = _set.AsQueryable();
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            return @where != null ? query.AsNoTracking().Where(@where) : query.AsNoTracking();
        }
        public class IncludeTree<TPreviousProperty, TProperty, TNextProperty>
        {
            public Expression<Func<T, TProperty>> Property { get; set; }
            public Expression<Func<TProperty, TNextProperty>>[] navigationPropertyPath   { get; set; } 

            public List< IncludeTree<TPreviousProperty, TProperty, TNextProperty>>[] Childs { get; set; }
        }

    //static    IIncludableQueryable<TEntity> IncludeTable<TEntity>(  IIncludableQueryable<TEntity> queryable, List< IncludeTree<T>> includeTrees)
    //    {
    //        foreach (var includeTree in includeTrees)
    //        {
    //            queryable.Include(includeTree.Property).ThenInclude((includeTree.ChildInclude));
         
    //        }
    //    }

      

     

        public virtual IList<TView> SqlQuery<TView>(string sql, params object[] parameters) where TView : class, new()
        {
            return _dbContext.SqlQuery<T, TView>(sql, parameters);
        }

        
        public virtual IEnumerable<T> GetByPagination(Expression<Func<T, bool>> @where, int pageSize, int pageIndex, Expression<Func<T, object>>[] includes, bool asc = true, params Func<T, object>[] @orderby)
        {
            var filter = Get(where).AsQueryable();
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    filter = filter.Include(include);
                }
            }
            if (orderby != null)
            {
                foreach (var func in orderby)
                {
                    filter = asc ? filter.OrderBy(func).AsQueryable() : filter.OrderByDescending(func).AsQueryable();
                }
            }
            return filter.Skip(pageSize * (pageIndex - 1)).Take(pageSize);
        }
        #endregion
    }
}
