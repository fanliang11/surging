using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.System.MongoProvider.Repositories
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IMongoRepository{T}" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IMongoRepository<T> where T : IEntity
    {
        #region 属性

        /// <summary>
        /// Gets the Collection
        /// </summary>
        IMongoCollection<T> Collection { get; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The Add
        /// </summary>
        /// <param name="entities">The entities<see cref="IEnumerable{T}"/></param>
        /// <returns>The <see cref="bool"/></returns>
        bool Add(IEnumerable<T> entities);

        /// <summary>
        /// The Add
        /// </summary>
        /// <param name="entity">The entity<see cref="T"/></param>
        /// <returns>The <see cref="T"/></returns>
        T Add(T entity);

        /// <summary>
        /// The AddAsync
        /// </summary>
        /// <param name="entity">The entity<see cref="T"/></param>
        /// <returns>The <see cref="Task{bool}"/></returns>
        Task<bool> AddAsync(T entity);

        /// <summary>
        /// The AddManyAsync
        /// </summary>
        /// <param name="entities">The entities<see cref="IEnumerable{T}"/></param>
        /// <returns>The <see cref="Task{bool}"/></returns>
        Task<bool> AddManyAsync(IEnumerable<T> entities);

        /// <summary>
        /// The All
        /// </summary>
        /// <returns>The <see cref="IQueryable{T}"/></returns>
        IQueryable<T> All();

        /// <summary>
        /// The All
        /// </summary>
        /// <param name="criteria">The criteria<see cref="Expression{Func{T, bool}}"/></param>
        /// <returns>The <see cref="IQueryable{T}"/></returns>
        IQueryable<T> All(Expression<Func<T, bool>> criteria);

        /// <summary>
        /// The Count
        /// </summary>
        /// <param name="filter">The filter<see cref="FilterDefinition{T}"/></param>
        /// <returns>The <see cref="long"/></returns>
        long Count(FilterDefinition<T> filter);

        /// <summary>
        /// The CountAsync
        /// </summary>
        /// <param name="filter">The filter<see cref="FilterDefinition{T}"/></param>
        /// <returns>The <see cref="Task{long}"/></returns>
        Task<long> CountAsync(FilterDefinition<T> filter);

        /// <summary>
        /// The Delete
        /// </summary>
        /// <param name="filter">The filter<see cref="FilterDefinition{T}"/></param>
        /// <returns>The <see cref="DeleteResult"/></returns>
        DeleteResult Delete(FilterDefinition<T> filter);

        /// <summary>
        /// The DeleteAsync
        /// </summary>
        /// <param name="filter">The filter<see cref="FilterDefinition{T}"/></param>
        /// <returns>The <see cref="Task{DeleteResult}"/></returns>
        Task<DeleteResult> DeleteAsync(FilterDefinition<T> filter);

        /// <summary>
        /// The DeleteMany
        /// </summary>
        /// <param name="filter">The filter<see cref="FilterDefinition{T}"/></param>
        /// <returns>The <see cref="DeleteResult"/></returns>
        DeleteResult DeleteMany(FilterDefinition<T> filter);

        /// <summary>
        /// The DeleteManyAsync
        /// </summary>
        /// <param name="filter">The filter<see cref="FilterDefinition{T}"/></param>
        /// <returns>The <see cref="Task{DeleteResult}"/></returns>
        Task<DeleteResult> DeleteManyAsync(FilterDefinition<T> filter);

        /// <summary>
        /// The Exists
        /// </summary>
        /// <param name="criteria">The criteria<see cref="Expression{Func{T, object}}"/></param>
        /// <param name="exists">The exists<see cref="bool"/></param>
        /// <returns>The <see cref="bool"/></returns>
        bool Exists(Expression<Func<T, object>> criteria, bool exists);

        /// <summary>
        /// The FindOneAndDelete
        /// </summary>
        /// <param name="criteria">The criteria<see cref="Expression{Func{T, bool}}"/></param>
        /// <returns>The <see cref="T"/></returns>
        T FindOneAndDelete(Expression<Func<T, bool>> criteria);

        /// <summary>
        /// The FindOneAndDeleteAsync
        /// </summary>
        /// <param name="criteria">The criteria<see cref="Expression{Func{T, bool}}"/></param>
        /// <returns>The <see cref="Task{T}"/></returns>
        Task<T> FindOneAndDeleteAsync(Expression<Func<T, bool>> criteria);

        /// <summary>
        /// The FindOneAndUpdate
        /// </summary>
        /// <param name="filter">The filter<see cref="FilterDefinition{T}"/></param>
        /// <param name="entity">The entity<see cref="UpdateDefinition{T}"/></param>
        /// <returns>The <see cref="T"/></returns>
        T FindOneAndUpdate(FilterDefinition<T> filter, UpdateDefinition<T> entity);

        /// <summary>
        /// The FindOneAndUpdateAsync
        /// </summary>
        /// <param name="filter">The filter<see cref="FilterDefinition{T}"/></param>
        /// <param name="entity">The entity<see cref="UpdateDefinition{T}"/></param>
        /// <returns>The <see cref="Task{T}"/></returns>
        Task<T> FindOneAndUpdateAsync(FilterDefinition<T> filter, UpdateDefinition<T> entity);

        /// <summary>
        /// The GetById
        /// </summary>
        /// <param name="id">The id<see cref="string"/></param>
        /// <returns>The <see cref="T"/></returns>
        T GetById(string id);

        /// <summary>
        /// The GetList
        /// </summary>
        /// <param name="criteria">The criteria<see cref="Expression{Func{T, bool}}"/></param>
        /// <returns>The <see cref="List{T}"/></returns>
        List<T> GetList(Expression<Func<T, bool>> criteria);

        /// <summary>
        /// The GetListAsync
        /// </summary>
        /// <param name="criteria">The criteria<see cref="Expression{Func{T, bool}}"/></param>
        /// <returns>The <see cref="Task{List{T}}"/></returns>
        Task<List<T>> GetListAsync(Expression<Func<T, bool>> criteria);

        /// <summary>
        /// The GetPageAsc
        /// </summary>
        /// <param name="criteria">The criteria<see cref="Expression{Func{T, bool}}"/></param>
        /// <param name="ascSort">The ascSort<see cref="Expression{Func{T, object}}"/></param>
        /// <param name="pParams">The pParams<see cref="QueryParams"/></param>
        /// <returns>The <see cref="List{T}"/></returns>
        List<T> GetPageAsc(Expression<Func<T, bool>> criteria, Expression<Func<T, object>> ascSort, QueryParams pParams);

        /// <summary>
        /// The GetPageDesc
        /// </summary>
        /// <param name="criteria">The criteria<see cref="Expression{Func{T, bool}}"/></param>
        /// <param name="descSort">The descSort<see cref="Expression{Func{T, object}}"/></param>
        /// <param name="pParams">The pParams<see cref="QueryParams"/></param>
        /// <returns>The <see cref="List{T}"/></returns>
        List<T> GetPageDesc(Expression<Func<T, bool>> criteria, Expression<Func<T, object>> descSort, QueryParams pParams);

        /// <summary>
        /// The GetSingle
        /// </summary>
        /// <param name="criteria">The criteria<see cref="Expression{Func{T, bool}}"/></param>
        /// <returns>The <see cref="T"/></returns>
        T GetSingle(Expression<Func<T, bool>> criteria);

        /// <summary>
        /// The GetSingleAsync
        /// </summary>
        /// <param name="criteria">The criteria<see cref="Expression{Func{T, bool}}"/></param>
        /// <returns>The <see cref="Task{T}"/></returns>
        Task<T> GetSingleAsync(Expression<Func<T, bool>> criteria);

        /// <summary>
        /// The Update
        /// </summary>
        /// <param name="filter">The filter<see cref="FilterDefinition{T}"/></param>
        /// <param name="entity">The entity<see cref="UpdateDefinition{T}"/></param>
        /// <returns>The <see cref="UpdateResult"/></returns>
        UpdateResult Update(FilterDefinition<T> filter, UpdateDefinition<T> entity);

        /// <summary>
        /// The UpdateAsync
        /// </summary>
        /// <param name="filter">The filter<see cref="FilterDefinition{T}"/></param>
        /// <param name="entity">The entity<see cref="UpdateDefinition{T}"/></param>
        /// <returns>The <see cref="Task{UpdateResult}"/></returns>
        Task<UpdateResult> UpdateAsync(FilterDefinition<T> filter, UpdateDefinition<T> entity);

        /// <summary>
        /// The UpdateMany
        /// </summary>
        /// <param name="filter">The filter<see cref="FilterDefinition{T}"/></param>
        /// <param name="entity">The entity<see cref="UpdateDefinition{T}"/></param>
        /// <returns>The <see cref="UpdateResult"/></returns>
        UpdateResult UpdateMany(FilterDefinition<T> filter, UpdateDefinition<T> entity);

        /// <summary>
        /// The UpdateManyAsync
        /// </summary>
        /// <param name="filter">The filter<see cref="FilterDefinition{T}"/></param>
        /// <param name="entity">The entity<see cref="UpdateDefinition{T}"/></param>
        /// <returns>The <see cref="Task{UpdateResult}"/></returns>
        Task<UpdateResult> UpdateManyAsync(FilterDefinition<T> filter, UpdateDefinition<T> entity);

        #endregion 方法
    }

    #endregion 接口
}