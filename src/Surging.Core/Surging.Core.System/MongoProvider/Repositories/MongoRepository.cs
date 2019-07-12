using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Surging.Core.System.MongoProvider.Repositories
{
    /// <summary>
    /// Defines the <see cref="MongoRepository{T}" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MongoRepository<T> : IMongoRepository<T>
            where T : IEntity
    {
        #region 字段

        /// <summary>
        /// Defines the _collection
        /// </summary>
        private readonly IMongoCollection<T> _collection;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoRepository{T}"/> class.
        /// </summary>
        public MongoRepository()
            : this(Util.GetDefaultConnectionString())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoRepository{T}"/> class.
        /// </summary>
        /// <param name="connectionString">The connectionString<see cref="string"/></param>
        public MongoRepository(string connectionString)
        {
            _collection = Util.GetCollectionFromConnectionString<T>(Util.GetDefaultConnectionString());
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the Collection
        /// </summary>
        public IMongoCollection<T> Collection
        {
            get { return _collection; }
        }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The Add
        /// </summary>
        /// <param name="entities">The entities<see cref="IEnumerable{T}"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public bool Add(IEnumerable<T> entities)
        {
            var result = true;
            try
            {
                _collection.InsertMany(entities);
            }
            catch
            {
                result = false;
            }
            return result;
        }

        /// <summary>
        /// The Add
        /// </summary>
        /// <param name="entity">The entity<see cref="T"/></param>
        /// <returns>The <see cref="T"/></returns>
        public T Add(T entity)
        {
            try
            {
                _collection.InsertOne(entity);
                return entity;
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary>
        /// The AddAsync
        /// </summary>
        /// <param name="entity">The entity<see cref="T"/></param>
        /// <returns>The <see cref="Task{bool}"/></returns>
        public async Task<bool> AddAsync(T entity)
        {
            var result = true;
            try
            {
                await _collection.InsertOneAsync(entity);
            }
            catch
            {
                result = false;
            }
            return result;
        }

        /// <summary>
        /// The AddManyAsync
        /// </summary>
        /// <param name="entities">The entities<see cref="IEnumerable{T}"/></param>
        /// <returns>The <see cref="Task{bool}"/></returns>
        public async Task<bool> AddManyAsync(IEnumerable<T> entities)
        {
            var result = true;
            try
            {
                await _collection.InsertManyAsync(entities);
            }
            catch
            {
                result = false;
            }
            return result;
        }

        /// <summary>
        /// The All
        /// </summary>
        /// <returns>The <see cref="IQueryable{T}"/></returns>
        public IQueryable<T> All()
        {
            return _collection.AsQueryable();
        }

        /// <summary>
        /// The All
        /// </summary>
        /// <param name="criteria">The criteria<see cref="Expression{Func{T, bool}}"/></param>
        /// <returns>The <see cref="IQueryable{T}"/></returns>
        public IQueryable<T> All(Expression<Func<T, bool>> criteria)
        {
            return _collection.AsQueryable().Where(criteria);
        }

        /// <summary>
        /// The Count
        /// </summary>
        /// <param name="filter">The filter<see cref="FilterDefinition{T}"/></param>
        /// <returns>The <see cref="long"/></returns>
        public long Count(FilterDefinition<T> filter)
        {
            var result = _collection.Count(filter);
            return result;
        }

        /// <summary>
        /// The CountAsync
        /// </summary>
        /// <param name="filter">The filter<see cref="FilterDefinition{T}"/></param>
        /// <returns>The <see cref="Task{long}"/></returns>
        public async Task<long> CountAsync(FilterDefinition<T> filter)
        {
            var result = await _collection.CountAsync(filter);
            return result;
        }

        /// <summary>
        /// The Delete
        /// </summary>
        /// <param name="filter">The filter<see cref="FilterDefinition{T}"/></param>
        /// <returns>The <see cref="DeleteResult"/></returns>
        public DeleteResult Delete(FilterDefinition<T> filter)
        {
            return _collection.DeleteOne(filter);
        }

        /// <summary>
        /// The DeleteAsync
        /// </summary>
        /// <param name="filter">The filter<see cref="FilterDefinition{T}"/></param>
        /// <returns>The <see cref="Task{DeleteResult}"/></returns>
        public async Task<DeleteResult> DeleteAsync(FilterDefinition<T> filter)
        {
            var result = await _collection.DeleteOneAsync(filter);
            return result;
        }

        /// <summary>
        /// The DeleteMany
        /// </summary>
        /// <param name="filter">The filter<see cref="FilterDefinition{T}"/></param>
        /// <returns>The <see cref="DeleteResult"/></returns>
        public DeleteResult DeleteMany(FilterDefinition<T> filter)
        {
            var result = _collection.DeleteMany(filter);
            return result;
        }

        /// <summary>
        /// The DeleteManyAsync
        /// </summary>
        /// <param name="filter">The filter<see cref="FilterDefinition{T}"/></param>
        /// <returns>The <see cref="Task{DeleteResult}"/></returns>
        public async Task<DeleteResult> DeleteManyAsync(FilterDefinition<T> filter)
        {
            var result = await _collection.DeleteManyAsync(filter);
            return result;
        }

        /// <summary>
        /// The Exists
        /// </summary>
        /// <param name="criteria">The criteria<see cref="Expression{Func{T, object}}"/></param>
        /// <param name="exists">The exists<see cref="bool"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public bool Exists(Expression<Func<T, object>> criteria, bool exists)
        {
            var builder = Builders<T>.Filter;
            var filter = builder.Exists(criteria, exists);
            return _collection.Find(filter).Any();
        }

        /// <summary>
        /// The FindOneAndDelete
        /// </summary>
        /// <param name="criteria">The criteria<see cref="Expression{Func{T, bool}}"/></param>
        /// <returns>The <see cref="T"/></returns>
        public T FindOneAndDelete(Expression<Func<T, bool>> criteria)
        {
            var result = _collection.FindOneAndDelete(criteria);
            return result;
        }

        /// <summary>
        /// The FindOneAndDeleteAsync
        /// </summary>
        /// <param name="criteria">The criteria<see cref="Expression{Func{T, bool}}"/></param>
        /// <returns>The <see cref="Task{T}"/></returns>
        public async Task<T> FindOneAndDeleteAsync(Expression<Func<T, bool>> criteria)
        {
            var result = await _collection.FindOneAndDeleteAsync(criteria);
            return result;
        }

        /// <summary>
        /// The FindOneAndUpdate
        /// </summary>
        /// <param name="filter">The filter<see cref="FilterDefinition{T}"/></param>
        /// <param name="entity">The entity<see cref="UpdateDefinition{T}"/></param>
        /// <returns>The <see cref="T"/></returns>
        public T FindOneAndUpdate(FilterDefinition<T> filter, UpdateDefinition<T> entity)
        {
            var result = _collection.FindOneAndUpdate(filter, entity);
            return result;
        }

        /// <summary>
        /// The FindOneAndUpdateAsync
        /// </summary>
        /// <param name="filter">The filter<see cref="FilterDefinition{T}"/></param>
        /// <param name="entity">The entity<see cref="UpdateDefinition{T}"/></param>
        /// <returns>The <see cref="Task{T}"/></returns>
        public async Task<T> FindOneAndUpdateAsync(FilterDefinition<T> filter, UpdateDefinition<T> entity)
        {
            var result = await _collection.FindOneAndUpdateAsync(filter, entity);
            return result;
        }

        /// <summary>
        /// The GetById
        /// </summary>
        /// <param name="id">The id<see cref="string"/></param>
        /// <returns>The <see cref="T"/></returns>
        public T GetById(string id)
        {
            var builder = Builders<T>.Filter;
            var filter = builder.Eq("_id", ObjectId.Parse(id));
            return _collection.Find(filter).FirstOrDefault();
        }

        /// <summary>
        /// The GetList
        /// </summary>
        /// <param name="criteria">The criteria<see cref="Expression{Func{T, bool}}"/></param>
        /// <returns>The <see cref="List{T}"/></returns>
        public List<T> GetList(Expression<Func<T, bool>> criteria)
        {
            var result = _collection.Find(criteria).ToList();
            return result;
        }

        /// <summary>
        /// The GetListAsync
        /// </summary>
        /// <param name="criteria">The criteria<see cref="Expression{Func{T, bool}}"/></param>
        /// <returns>The <see cref="Task{List{T}}"/></returns>
        public async Task<List<T>> GetListAsync(Expression<Func<T, bool>> criteria)
        {
            var result = await _collection.FindSync(criteria).ToListAsync();
            return result;
        }

        /// <summary>
        /// The GetPageAsc
        /// </summary>
        /// <param name="criteria">The criteria<see cref="Expression{Func{T, bool}}"/></param>
        /// <param name="ascSort">The ascSort<see cref="Expression{Func{T, object}}"/></param>
        /// <param name="pParams">The pParams<see cref="QueryParams"/></param>
        /// <returns>The <see cref="List{T}"/></returns>
        public List<T> GetPageAsc(Expression<Func<T, bool>> criteria, Expression<Func<T, object>> ascSort, QueryParams pParams)
        {
            var sort = new SortDefinitionBuilder<T>();
            var result = _collection.Find(criteria).Sort(sort.Ascending(ascSort)).Skip((pParams.Index - 1) * pParams.Size).Limit(
                            pParams.Size).ToList();
            return result;
        }

        /// <summary>
        /// The GetPageDesc
        /// </summary>
        /// <param name="criteria">The criteria<see cref="Expression{Func{T, bool}}"/></param>
        /// <param name="descSort">The descSort<see cref="Expression{Func{T, object}}"/></param>
        /// <param name="pParams">The pParams<see cref="QueryParams"/></param>
        /// <returns>The <see cref="List{T}"/></returns>
        public List<T> GetPageDesc(Expression<Func<T, bool>> criteria, Expression<Func<T, object>> descSort, QueryParams pParams)
        {
            var sort = new SortDefinitionBuilder<T>();
            var result = _collection.Find(criteria).Sort(sort.Descending(descSort)).Skip((pParams.Index - 1) * pParams.Size).Limit(
                            pParams.Size).ToList();
            return result;
        }

        /// <summary>
        /// The GetSingle
        /// </summary>
        /// <param name="criteria">The criteria<see cref="Expression{Func{T, bool}}"/></param>
        /// <returns>The <see cref="T"/></returns>
        public T GetSingle(Expression<Func<T, bool>> criteria)
        {
            return _collection.Find(criteria).FirstOrDefault();
        }

        /// <summary>
        /// The GetSingleAsync
        /// </summary>
        /// <param name="criteria">The criteria<see cref="Expression{Func{T, bool}}"/></param>
        /// <returns>The <see cref="Task{T}"/></returns>
        public async Task<T> GetSingleAsync(Expression<Func<T, bool>> criteria)
        {
            var result = await _collection.FindSync(criteria).FirstOrDefaultAsync();
            return result;
        }

        /// <summary>
        /// The Update
        /// </summary>
        /// <param name="filter">The filter<see cref="FilterDefinition{T}"/></param>
        /// <param name="entity">The entity<see cref="UpdateDefinition{T}"/></param>
        /// <returns>The <see cref="UpdateResult"/></returns>
        public UpdateResult Update(FilterDefinition<T> filter, UpdateDefinition<T> entity)
        {
            return _collection.UpdateOne(filter, entity, new UpdateOptions()
            {
                IsUpsert = true,
                BypassDocumentValidation = true,
            });
        }

        /// <summary>
        /// The UpdateAsync
        /// </summary>
        /// <param name="filter">The filter<see cref="FilterDefinition{T}"/></param>
        /// <param name="entity">The entity<see cref="UpdateDefinition{T}"/></param>
        /// <returns>The <see cref="Task{UpdateResult}"/></returns>
        public async Task<UpdateResult> UpdateAsync(FilterDefinition<T> filter, UpdateDefinition<T> entity)
        {
            var result = await _collection.UpdateOneAsync(filter, entity);
            return result;
        }

        /// <summary>
        /// The UpdateMany
        /// </summary>
        /// <param name="filter">The filter<see cref="FilterDefinition{T}"/></param>
        /// <param name="entity">The entity<see cref="UpdateDefinition{T}"/></param>
        /// <returns>The <see cref="UpdateResult"/></returns>
        public UpdateResult UpdateMany(FilterDefinition<T> filter, UpdateDefinition<T> entity)
        {
            return _collection.UpdateMany(filter, entity);
        }

        /// <summary>
        /// The UpdateManyAsync
        /// </summary>
        /// <param name="filter">The filter<see cref="FilterDefinition{T}"/></param>
        /// <param name="entity">The entity<see cref="UpdateDefinition{T}"/></param>
        /// <returns>The <see cref="Task{UpdateResult}"/></returns>
        public async Task<UpdateResult> UpdateManyAsync(FilterDefinition<T> filter, UpdateDefinition<T> entity)
        {
            var result = await _collection.UpdateManyAsync(filter, entity);
            return result;
        }

        #endregion 方法
    }
}