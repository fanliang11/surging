using DDD.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Repository.Dapper.Core
{
    public class BaseImpQueryOnlyRepository<T> : IQueryOnlyRepository<T, Guid> where T : class, IDBModel<Guid>
    {
        public int Count(Expression<Func<T, bool>> where = null)
        {
            throw new NotImplementedException();
        }

        public int ExecuteSqlWithNonQuery(string sql, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public bool Exist(Expression<Func<T, bool>> where = null)
        {
            throw new NotImplementedException();
        }

        public bool Exist(Expression<Func<T, bool>> where = null, params Expression<Func<T, object>>[] includes)
        {
            throw new NotImplementedException();
        }

        public IQueryable<T> Get(Expression<Func<T, bool>> where = null)
        {
            throw new NotImplementedException();
        }

        public IQueryable<T> Get(Expression<Func<T, bool>> where = null, params Expression<Func<T, object>>[] includes)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> GetByPagination(Expression<Func<T, bool>> where, int pageSize, int pageIndex, Expression<Func<T, object>>[] include, bool asc = true, params Func<T, object>[] orderby)
        {
            throw new NotImplementedException();
        }

        public T GetSingle(Guid key)
        {
            throw new NotImplementedException();
        }

        public T GetSingle(Guid key, params Expression<Func<T, object>>[] includes)
        {
            throw new NotImplementedException();
        }

        public T GetSingle(Expression<Func<T, bool>> where = null)
        {
            throw new NotImplementedException();
        }

        public T GetSingle(Expression<Func<T, bool>> where = null, params Expression<Func<T, object>>[] includes)
        {
            throw new NotImplementedException();
        }

        public IList<TView> SqlQuery<TView>(string sql, params object[] parameters) where TView : class, new()
        {
            throw new NotImplementedException();
        }
    }
}
