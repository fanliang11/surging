using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
 
using MongoDB.Driver;

namespace Surging.Core.System.MongoProvider.Repositories
{
   public  interface IMongoRepository<T> where T : IEntity
    {
        IMongoCollection<T> Collection { get; }
        T GetById(string id);
        T GetSingle(Expression<Func<T, bool>> criteria);
        Task<T> GetSingleAsync(Expression<Func<T, bool>> criteria);
        Task<List<T>> GetListAsync(Expression<Func<T, bool>> criteria);
        List<T> GetList(Expression<Func<T, bool>> criteria);
        IQueryable<T> All();
        IQueryable<T> All(Expression<Func<T, bool>> criteria);
        T Add(T entity);
        Task<bool> AddAsync(T entity);
        bool Add(IEnumerable<T> entities);
        Task<bool> AddManyAsync(IEnumerable<T> entities);
        UpdateResult Update(FilterDefinition<T> filter, UpdateDefinition<T> entity);
        Task<UpdateResult> UpdateAsync(FilterDefinition<T> filter, UpdateDefinition<T> entity);
        UpdateResult UpdateMany(FilterDefinition<T> filter, UpdateDefinition<T> entity);
        Task<UpdateResult> UpdateManyAsync(FilterDefinition<T> filter, UpdateDefinition<T> entity);
        T FindOneAndUpdate(FilterDefinition<T> filter, UpdateDefinition<T> entity);
        Task<T> FindOneAndUpdateAsync(FilterDefinition<T> filter, UpdateDefinition<T> entity);
        DeleteResult Delete(FilterDefinition<T> filter);
        Task<DeleteResult> DeleteAsync(FilterDefinition<T> filter);
        DeleteResult DeleteMany(FilterDefinition<T> filter);
        Task<DeleteResult> DeleteManyAsync(FilterDefinition<T> filter);
        List<T> GetPageAsc(Expression<Func<T, bool>> criteria, Expression<Func<T, object>> ascSort, QueryParams pParams);
        List<T> GetPageDesc(Expression<Func<T, bool>> criteria, Expression<Func<T, object>> descSort, QueryParams pParams);
        T FindOneAndDelete(Expression<Func<T, bool>> criteria);
        Task<T> FindOneAndDeleteAsync(Expression<Func<T, bool>> criteria);
        long Count(FilterDefinition<T> filter);
        Task<long> CountAsync(FilterDefinition<T> filter);
        bool Exists(Expression<Func<T, object>> criteria, bool exists);
    }

}


