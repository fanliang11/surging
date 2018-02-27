using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace DDD.Core
{
    /// <summary>
    /// 只读的查询资源库接口
    /// </summary>
   public interface IQueryOnlyRepository<T, TKey> where T : class,IDBModel<Guid>
    {
         
        int Count(Expression<Func<T, bool>> @where = null);
        
          bool Exist(Expression<Func<T, bool>> @where = null);
          bool Exist(Expression<Func<T, bool>> @where = null, params Expression<Func<T, object>>[] includes);
          int ExecuteSqlWithNonQuery(string sql, params object[] parameters);
          T GetSingle(TKey key);
          T GetSingle(TKey key, params Expression<Func<T, object>>[] includes);
          T GetSingle(Expression<Func<T, bool>> @where = null);
          T GetSingle(Expression<Func<T, bool>> @where = null, params Expression<Func<T, object>>[] includes);
          IQueryable<T> Get(Expression<Func<T, bool>> @where = null);
          IQueryable<T> Get(Expression<Func<T, bool>> @where = null, params Expression<Func<T, object>>[] includes);
          IEnumerable<T> GetByPagination(Expression<Func<T, bool>> @where, int pageSize, int pageIndex, 
               Expression<Func<T, object>>[] include,bool asc = true, params Func<T, object>[] @orderby);
        
          IList<TView> SqlQuery<TView>(string sql, params object[] parameters) where TView : class, new();
          
         
    }
}
