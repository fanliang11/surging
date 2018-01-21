using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks; 


namespace Surging.Core.System.MongoProvider.Repositories
{
    public class MongoRepository<T> : IMongoRepository<T>
            where T : IEntity
    {
        private readonly IMongoCollection<T> _collection;

        public MongoRepository()
            : this(Util.GetDefaultConnectionString())
        {
        }

        public MongoRepository(string connectionString)
        {
            _collection = Util.GetCollectionFromConnectionString<T>(Util.GetDefaultConnectionString());
        }

        public IMongoCollection<T> Collection
        {
            get { return _collection; }
        }

        public T GetById(string id)
        {
            var builder = Builders<T>.Filter;
            var filter = builder.Eq("_id", ObjectId.Parse(id));
            return _collection.Find(filter).FirstOrDefault();
        }

        public T GetSingle(Expression<Func<T, bool>> criteria)
        {
            return _collection.Find(criteria).FirstOrDefault();
        }

        public async Task<T> GetSingleAsync(Expression<Func<T, bool>> criteria)
        {
            var result = await _collection.FindSync(criteria).FirstOrDefaultAsync();
            return result;
        }

        public List<T> GetPageDesc(Expression<Func<T, bool>> criteria, Expression<Func<T, object>> descSort, QueryParams pParams)
        {
            var sort = new SortDefinitionBuilder<T>();
            var result = _collection.Find(criteria).Sort(sort.Descending(descSort)).Skip((pParams.Index - 1) * pParams.Size).Limit(
                            pParams.Size).ToList();
            return result;
        }

        public List<T> GetPageAsc(Expression<Func<T, bool>> criteria, Expression<Func<T, object>> ascSort, QueryParams pParams)
        {
            var sort = new SortDefinitionBuilder<T>();
            var result = _collection.Find(criteria).Sort(sort.Ascending(ascSort)).Skip((pParams.Index - 1) * pParams.Size).Limit(
                            pParams.Size).ToList();
            return result;
        }

        public async Task<List<T>> GetListAsync(Expression<Func<T, bool>> criteria)
        {
            var result = await _collection.FindSync(criteria).ToListAsync();
            return result;
        }

        public List<T> GetList(Expression<Func<T, bool>> criteria)
        {
            var result = _collection.Find(criteria).ToList();
            return result;
        }

        public IQueryable<T> All()
        {
            return _collection.AsQueryable();
        }

        public IQueryable<T> All(Expression<Func<T, bool>> criteria)
        {
            return _collection.AsQueryable().Where(criteria);
        }

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

        public UpdateResult Update(FilterDefinition<T> filter, UpdateDefinition<T> entity)
        {
            return _collection.UpdateOne(filter, entity, new UpdateOptions()
            {
                IsUpsert = true,
                BypassDocumentValidation = true,
            });
        }

        public async Task<UpdateResult> UpdateAsync(FilterDefinition<T> filter, UpdateDefinition<T> entity)
        {
            var result = await _collection.UpdateOneAsync(filter, entity);
            return result;
        }

        public UpdateResult UpdateMany(FilterDefinition<T> filter, UpdateDefinition<T> entity)
        {
            return _collection.UpdateMany(filter, entity);
        }

        public async Task<UpdateResult> UpdateManyAsync(FilterDefinition<T> filter, UpdateDefinition<T> entity)
        {
            var result = await _collection.UpdateManyAsync(filter, entity);
            return result;
        }

        public T FindOneAndUpdate(FilterDefinition<T> filter, UpdateDefinition<T> entity)
        {
            var result = _collection.FindOneAndUpdate(filter, entity);
            return result;
        }

        public async Task<T> FindOneAndUpdateAsync(FilterDefinition<T> filter, UpdateDefinition<T> entity)
        {
            var result = await _collection.FindOneAndUpdateAsync(filter, entity);
            return result;
        }

        public DeleteResult Delete(FilterDefinition<T> filter)
        {
            return _collection.DeleteOne(filter);
        }

        public async Task<DeleteResult> DeleteAsync(FilterDefinition<T> filter)
        {
            var result = await _collection.DeleteOneAsync(filter);
            return result;
        }

        public DeleteResult DeleteMany(FilterDefinition<T> filter)
        {
            var result = _collection.DeleteMany(filter);
            return result;
        }

        public async Task<DeleteResult> DeleteManyAsync(FilterDefinition<T> filter)
        {
            var result = await _collection.DeleteManyAsync(filter);
            return result;
        }

        public T FindOneAndDelete(Expression<Func<T, bool>> criteria)
        {
            var result = _collection.FindOneAndDelete(criteria);
            return result;
        }

        public async Task<T> FindOneAndDeleteAsync(Expression<Func<T, bool>> criteria)
        {
            var result = await _collection.FindOneAndDeleteAsync(criteria);
            return result;
        }


        public long Count(FilterDefinition<T> filter)
        {
            var result = _collection.Count(filter);
            return result;
        }

        public async Task<long> CountAsync(FilterDefinition<T> filter)
        {
            var result = await _collection.CountAsync(filter);
            return result;
        }

        public bool Exists(Expression<Func<T, object>> criteria, bool exists)
        {
            var builder = Builders<T>.Filter;
            var filter = builder.Exists(criteria, exists);
            return _collection.Find(filter).Any();
        }
    }
}
