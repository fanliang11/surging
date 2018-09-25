using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Dapper.Core
{
    public partial interface IDatabase
    {
        #region Count
        long Count<T>(object predicate = null, int? commandTimeout = null) where T : class;
        long Count<T>(string tableName, object predicate = null, int? commandTimeout = null) where T : class;
        long Count<T>(string tableName, string schemaName, object predicate = null, int? commandTimeout = null) where T : class;

        Task<long> CountAsync<T>(object predicate = null, int? commandTimeout = null) where T : class;
        Task<long> CountAsync<T>(string tableName, object predicate = null, int? commandTimeout = null) where T : class;
        Task<long> CountAsync<T>(string tableName, string schemaName, object predicate = null, int? commandTimeout = null) where T : class;


        long Count<T>(IList<IJoinPredicate> join, object predicate = null, int? commandTimeout = null) where T : class;

        Task<long> CountAsync<T>(IList<IJoinPredicate> join, object predicate = null, int? commandTimeout = null) where T : class;

        #endregion

        #region Delete
        bool Delete<T>(T entity, int? commandTimeout = null) where T : class;
        bool Delete<T>(string tableName, T entity, int? commandTimeout = null) where T : class;
        bool Delete<T>(string tableName, string schemaName, T entity, int? commandTimeout = null) where T : class;

        bool Delete<T>(object predicate, int? commandTimeout = null) where T : class;
        bool Delete<T>(string tableName, object predicate, int? commandTimeout = null) where T : class;
        bool Delete<T>(string tableName, string schemaName, object predicate, int? commandTimeout = null) where T : class;

        Task<bool> DeleteAsync<T>(T entity, int? commandTimeout = null) where T : class;
        Task<bool> DeleteAsync<T>(string tableName, T entity, int? commandTimeout = null) where T : class;
        Task<bool> DeleteAsync<T>(string tableName, string schemaName, T entity, int? commandTimeout = null) where T : class;

        Task<bool> DeleteAsync<T>(object predicate, int? commandTimeout = null) where T : class;
        Task<bool> DeleteAsync<T>(string tableName, object predicate, int? commandTimeout = null) where T : class;
        Task<bool> DeleteAsync<T>(string tableName, string schemaName, object predicate, int? commandTimeout = null) where T : class;
        #endregion

        #region Get
        T Get<T>(dynamic id, int? commandTimeout = null) where T : class;
        T Get<T>(dynamic id, string tableName, int? commandTimeout = null) where T : class;
        T Get<T>(dynamic id, string tableName, string schemaName, int? commandTimeout = null) where T : class;

        Task<T> GetAsync<T>(dynamic id, int? commandTimeout = null) where T : class;
        Task<T> GetAsync<T>(dynamic id, string tableName, int? commandTimeout = null) where T : class;
        Task<T> GetAsync<T>(dynamic id, string tableName, string schemaName, int? commandTimeout = null) where T : class;

        T Get<T>(IList<IJoinPredicate> join, dynamic id, int? commandTimeout = null) where T : class;
        T Get<T>(IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias, dynamic id, int? commandTimeout = null) where T : class;

        Task<T> GetAsync<T>(IList<IJoinPredicate> join, dynamic id, int? commandTimeout = null) where T : class;
        Task<T> GetAsync<T>(IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias, dynamic id, int? commandTimeout = null) where T : class;
        #endregion

        #region GetList
        IEnumerable<T> GetList<T>(object predicate = null, IList<ISort> sort = null, int? commandTimeout = null, bool buffered = true) where T : class;
        IEnumerable<T> GetList<T>(string tableName, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null, bool buffered = true) where T : class;
        IEnumerable<T> GetList<T>(string tableName, string schemaName, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null, bool buffered = true) where T : class;

        Task<IEnumerable<T>> GetListAsync<T>(object predicate = null, IList<ISort> sort = null, int? commandTimeout = null) where T : class;
        Task<IEnumerable<T>> GetListAsync<T>(string tableName, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null) where T : class;
        Task<IEnumerable<T>> GetListAsync<T>(string tableName, string schemaName, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null) where T : class;


        IEnumerable<T> GetList<T>(IList<IJoinPredicate> join, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null, bool buffered = true) where T : class;

        IEnumerable<T> GetList<T>(IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias = null, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null, bool buffered = true) where T : class;

        Task<IEnumerable<T>> GetListAsync<T>(IList<IJoinPredicate> join, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null) where T : class;

        Task<IEnumerable<T>> GetListAsync<T>(IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias = null, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null) where T : class;

        #endregion

        #region GetMultiple
        IMultipleResultReader GetMultiple(GetMultiplePredicate predicate = null, int? commandTimeout = null);
        IMultipleResultReader GetMultiple(string tableName, GetMultiplePredicate predicate = null, int? commandTimeout = null);
        IMultipleResultReader GetMultiple(string tableName, string schemaName, GetMultiplePredicate predicate = null, int? commandTimeout = null);
        Task<IMultipleResultReader> GetMultipleAsync(GetMultiplePredicate predicate = null, int? commandTimeout = null);
        Task<IMultipleResultReader> GetMultipleAsync(string tableName, GetMultiplePredicate predicate = null, int? commandTimeout = null);
        Task<IMultipleResultReader> GetMultipleAsync(string tableName, string schemaName, GetMultiplePredicate predicate = null, int? commandTimeout = null);
        #endregion

        #region GetPage
        IEnumerable<T> GetPage<T>(int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null, bool buffered = true) where T : class;
        IEnumerable<T> GetPage<T>(string tableName, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null, bool buffered = true) where T : class;
        IEnumerable<T> GetPage<T>(string tableName, string schemaName, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null, bool buffered = true) where T : class;


        Task<IEnumerable<T>> GetPageAsync<T>(int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null) where T : class;
        Task<IEnumerable<T>> GetPageAsync<T>(string tableName, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null) where T : class;
        Task<IEnumerable<T>> GetPageAsync<T>(string tableName, string schemaName, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null) where T : class;

        IEnumerable<T> GetPage<T>(IList<IJoinPredicate> join, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null, bool buffered = true) where T : class;

        IEnumerable<T> GetPage<T>(IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null, bool buffered = true) where T : class;

        Task<IEnumerable<T>> GetPageAsync<T>(IList<IJoinPredicate> join, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null) where T : class;

        Task<IEnumerable<T>> GetPageAsync<T>(IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null) where T : class;

        #endregion


        #region GetPages
        Page<T> GetPages<T>(int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null) where T : class;
        Page<T> GetPages<T>(string tableName, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null) where T : class;
        Page<T> GetPages<T>(string tableName, string schemaName, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null) where T : class;

        Task<Page<T>> GetPagesAsync<T>(int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null) where T : class;
        Task<Page<T>> GetPagesAsync<T>(string tableName, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null) where T : class;
        Task<Page<T>> GetPagesAsync<T>(string tableName, string schemaName, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null) where T : class;

        Page<T> GetPages<T>(IList<IJoinPredicate> join, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null) where T : class;
        Page<T> GetPages<T>(IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null) where T : class;
        Task<Page<T>> GetPagesAsync<T>(IList<IJoinPredicate> join, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null) where T : class;

        Task<Page<T>> GetPagesAsync<T>(IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null) where T : class;

        #endregion

        #region GetSet
        IEnumerable<T> GetSet<T>(object predicate = null, IList<ISort> sort = null, int firstResult = 1, int maxResults = 10, int? commandTimeout = null, bool buffered = true) where T : class;
        IEnumerable<T> GetSet<T>(string tableName, object predicate = null, IList<ISort> sort = null, int firstResult = 1, int maxResults = 10, int? commandTimeout = null, bool buffered = true) where T : class;
        IEnumerable<T> GetSet<T>(string tableName, string schemaName, object predicate = null, IList<ISort> sort = null, int firstResult = 1, int maxResults = 10, int? commandTimeout = null, bool buffered = true) where T : class;
        Task<IEnumerable<T>> GetSetAsync<T>(object predicate = null, IList<ISort> sort = null, int firstResult = 1, int maxResults = 10, int? commandTimeout = null) where T : class;
        Task<IEnumerable<T>> GetSetAsync<T>(string tableName, object predicate = null, IList<ISort> sort = null, int firstResult = 1, int maxResults = 10, int? commandTimeout = null) where T : class;
        Task<IEnumerable<T>> GetSetAsync<T>(string tableName, string schemaName, object predicate = null, IList<ISort> sort = null, int firstResult = 1, int maxResults = 10, int? commandTimeout = null) where T : class;

        IEnumerable<T> GetSet<T>(IList<IJoinPredicate> join, object predicate = null, IList<ISort> sort = null, int firstResult = 1, int maxResults = 10, int? commandTimeout = null, bool buffered = true) where T : class;

        IEnumerable<T> GetSet<T>(IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias, object predicate = null, IList<ISort> sort = null, int firstResult = 1, int maxResults = 10, int? commandTimeout = null, bool buffered = true) where T : class;
        Task<IEnumerable<T>> GetSetAsync<T>(IList<IJoinPredicate> join, object predicate = null, IList<ISort> sort = null, int firstResult = 1, int maxResults = 10, int? commandTimeout = null) where T : class;

        Task<IEnumerable<T>> GetSetAsync<T>(IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias, object predicate = null, IList<ISort> sort = null, int firstResult = 1, int maxResults = 10, int? commandTimeout = null) where T : class;

        #endregion

        #region Insert
        void Insert<T>(IEnumerable<T> entities, int? commandTimeout = null) where T : class;
        void Insert<T>(string tableName, IEnumerable<T> entities, int? commandTimeout = null) where T : class;
        void Insert<T>(string tableName, string schemaName, IEnumerable<T> entities, int? commandTimeout = null) where T : class;

        dynamic Insert<T>(T entity, int? commandTimeout = null) where T : class;
        dynamic Insert<T>(string tableName, T entity, int? commandTimeout = null) where T : class;
        dynamic Insert<T>(string tableName, string schemaName, T entity, int? commandTimeout = null) where T : class;
        #endregion

        #region Update

        bool Update<T>(T entity, object predicate = null, int? commandTimeout = null, bool ignoreAllKeyProperties = false) where T : class;
        bool Update<T>(string tableName, T entity, object predicate = null, int? commandTimeout = null, bool ignoreAllKeyProperties = false) where T : class;
        bool Update<T>(string tableName, string schemaName, T entity, object predicate = null, int? commandTimeout = null, bool ignoreAllKeyProperties = false) where T : class;


        bool UpdateSet<T>(object entity, object predicate = null, int? commandTimeout = null) where T : class;
        bool UpdateSet<T>(string tableName, object entity, object predicate = null, int? commandTimeout = null) where T : class;
        bool UpdateSet<T>(string tableName, string schemaName, object entity, object predicate = null, int? commandTimeout = null) where T : class;




        Task<bool> UpdateAsync<T>(T entity, object predicate = null, int? commandTimeout = null, bool ignoreAllKeyProperties = false) where T : class;
        Task<bool> UpdateAsync<T>(string tableName, T entity, object predicate = null, int? commandTimeout = null, bool ignoreAllKeyProperties = false) where T : class;
        Task<bool> UpdateAsync<T>(string tableName, string schemaName, T entity, object predicate = null, int? commandTimeout = null, bool ignoreAllKeyProperties = false) where T : class;

        Task<bool> UpdateSetAsync<T>(object entity, object predicate = null, int? commandTimeout = null) where T : class;
        Task<bool> UpdateSetAsync<T>(string tableName, object entity, object predicate = null, int? commandTimeout = null) where T : class;
        Task<bool> UpdateSetAsync<T>(string tableName, string schemaName, object entity, object predicate = null, int? commandTimeout = null) where T : class;


        #endregion

    }
    public partial class Database
    {
        #region Count
        public long Count<T>(object predicate = null, int? commandTimeout = null) where T : class
            => Count<T>((string)null, predicate, commandTimeout);

        public long Count<T>(string tableName, object predicate = null, int? commandTimeout = null) where T : class
            => Count<T>(tableName, null, predicate, commandTimeout);

        public long Count<T>(string tableName, string schemaName, object predicate = null, int? commandTimeout = null) where T : class
            => _dapper.Count<T>(Connection, predicate, _transaction, commandTimeout, tableName, schemaName, null);

        public async Task<long> CountAsync<T>(object predicate = null, int? commandTimeout = null) where T : class
            => await CountAsync<T>((string)null, predicate, commandTimeout);

        public async Task<long> CountAsync<T>(string tableName, object predicate = null, int? commandTimeout = null) where T : class
            => await CountAsync<T>(tableName, null, predicate, commandTimeout);

        public async Task<long> CountAsync<T>(string tableName, string schemaName, object predicate = null, int? commandTimeout = null) where T : class
            => await _dapper.CountAsync<T>(Connection, predicate, _transaction, commandTimeout, tableName, schemaName, null);

        public long Count<T>(IList<IJoinPredicate> join, object predicate = null, int? commandTimeout = null) where T : class
            => _dapper.Count<T>(Connection, predicate, _transaction, commandTimeout, null, null, join);

        public async Task<long> CountAsync<T>(IList<IJoinPredicate> join, object predicate = null, int? commandTimeout = null) where T : class
          => await _dapper.CountAsync<T>(Connection, predicate, _transaction, commandTimeout, null, null, join);
        #endregion

        #region Delete
        public bool Delete<T>(T entity, int? commandTimeout = null) where T : class
            => Delete<T>(null, entity, commandTimeout);

        public bool Delete<T>(string tableName, T entity, int? commandTimeout = null) where T : class
            => Delete<T>(tableName, null, entity, commandTimeout);

        public bool Delete<T>(string tableName, string schemaName, T entity, int? commandTimeout = null) where T : class
            => _dapper.Delete(Connection, entity, _transaction, commandTimeout, tableName, schemaName);


        public bool Delete<T>(object predicate, int? commandTimeout = null) where T : class
            => Delete<T>(null, predicate, commandTimeout);

        public bool Delete<T>(string tableName, object predicate, int? commandTimeout = null) where T : class
            => Delete<T>(tableName, null, predicate, commandTimeout);

        public bool Delete<T>(string tableName, string schemaName, object predicate, int? commandTimeout = null) where T : class
            => _dapper.Delete<T>(Connection, predicate, _transaction, commandTimeout, tableName, schemaName);


        public async Task<bool> DeleteAsync<T>(T entity, int? commandTimeout = null) where T : class
            => await DeleteAsync<T>(null, entity, commandTimeout);

        public async Task<bool> DeleteAsync<T>(string tableName, T entity, int? commandTimeout = null) where T : class
            => await DeleteAsync<T>(tableName, null, entity, commandTimeout);

        public async Task<bool> DeleteAsync<T>(string tableName, string schemaName, T entity, int? commandTimeout = null) where T : class
            => await _dapper.DeleteAsync<T>(Connection, entity, _transaction, commandTimeout, tableName, schemaName);


        public async Task<bool> DeleteAsync<T>(object predicate, int? commandTimeout = null) where T : class
            => await DeleteAsync<T>(null, predicate, commandTimeout);

        public async Task<bool> DeleteAsync<T>(string tableName, object predicate, int? commandTimeout = null) where T : class
            => await DeleteAsync<T>(tableName, null, predicate, commandTimeout);

        public async Task<bool> DeleteAsync<T>(string tableName, string schemaName, object predicate, int? commandTimeout = null) where T : class
            => await _dapper.DeleteAsync<T>(Connection, predicate, _transaction, commandTimeout, tableName, schemaName);

        #endregion

        #region Get
        public T Get<T>(dynamic id, int? commandTimeout = null) where T : class
           => Get<T>(id, (string)null, commandTimeout);

        public T Get<T>(dynamic id, string tableName, int? commandTimeout = null) where T : class
           => Get<T>(id, tableName, (string)null, commandTimeout);

        public T Get<T>(dynamic id, string tableName, string schemaName, int? commandTimeout = null) where T : class
            => (T)_dapper.Get<T>(Connection, id, _transaction, commandTimeout, tableName, schemaName, null, null);


        public async Task<T> GetAsync<T>(dynamic id, int? commandTimeout = null) where T : class
            => await GetAsync<T>(id, (string)null, commandTimeout);

        public async Task<T> GetAsync<T>(dynamic id, string tableName, int? commandTimeout = null) where T : class
            => await GetAsync<T>(id, tableName, (string)null, commandTimeout);

        public async Task<T> GetAsync<T>(dynamic id, string tableName, string schemaName, int? commandTimeout = null) where T : class
            => await _dapper.GetAsync<T>(Connection, id, _transaction, commandTimeout, tableName, schemaName, null, null);



        public T Get<T>(IList<IJoinPredicate> join, dynamic id, int? commandTimeout = null) where T : class
           => Get<T>(join, null, id, commandTimeout);

        public T Get<T>(IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias, dynamic id, int? commandTimeout = null) where T : class
           => (T)_dapper.Get<T>(Connection, id, _transaction, commandTimeout, null, null, join, alias);

        public async Task<T> GetAsync<T>(IList<IJoinPredicate> join, dynamic id, int? commandTimeout = null) where T : class
           => await GetAsync<T>(join, null, id, commandTimeout);
        public async Task<T> GetAsync<T>(IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias, dynamic id, int? commandTimeout = null) where T : class
           => await _dapper.GetAsync<T>(Connection, id, _transaction, commandTimeout, null, null, join, alias);
        #endregion

        #region GetList

        public IEnumerable<T> GetList<T>(object predicate = null, IList<ISort> sort = null, int? commandTimeout = null, bool buffered = true) where T : class
           => GetList<T>((string)null, predicate, sort, commandTimeout, buffered);

        public IEnumerable<T> GetList<T>(string tableName, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null, bool buffered = true) where T : class
            => GetList<T>(tableName, null, predicate, sort, commandTimeout, buffered);

        public IEnumerable<T> GetList<T>(string tableName, string schemaName, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null, bool buffered = true) where T : class
            => _dapper.GetList<T>(Connection, predicate, sort, _transaction, commandTimeout, buffered, tableName, schemaName, null, null);


        public async Task<IEnumerable<T>> GetListAsync<T>(object predicate = null, IList<ISort> sort = null, int? commandTimeout = null) where T : class
            => await GetListAsync<T>((string)null, predicate, sort, commandTimeout);

        public async Task<IEnumerable<T>> GetListAsync<T>(string tableName, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null) where T : class
            => await GetListAsync<T>(tableName, null, predicate, sort, commandTimeout);

        public async Task<IEnumerable<T>> GetListAsync<T>(string tableName, string schemaName, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null) where T : class
            => await _dapper.GetListAsync<T>(Connection, predicate, sort, _transaction, commandTimeout, tableName, schemaName, null, null);



        public IEnumerable<T> GetList<T>(IList<IJoinPredicate> join, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null, bool buffered = true) where T : class
           => GetList<T>(join, null, predicate, sort, commandTimeout, buffered);

        public IEnumerable<T> GetList<T>(IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias = null, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null, bool buffered = true) where T : class
            => _dapper.GetList<T>(Connection, predicate, sort, _transaction, commandTimeout, buffered, null, null, join, alias);

        public async Task<IEnumerable<T>> GetListAsync<T>(IList<IJoinPredicate> join, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null) where T : class
           => await GetListAsync<T>(join, null, predicate, sort, commandTimeout);

        public async Task<IEnumerable<T>> GetListAsync<T>(IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias = null, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null) where T : class
            => await _dapper.GetListAsync<T>(Connection, predicate, sort, _transaction, commandTimeout, null, null, join, alias);

        #endregion

        #region GetMultiple

        public IMultipleResultReader GetMultiple(GetMultiplePredicate predicate = null, int? commandTimeout = null)
            => GetMultiple(null, predicate, commandTimeout);
        public IMultipleResultReader GetMultiple(string tableName, GetMultiplePredicate predicate = null, int? commandTimeout = null)
            => GetMultiple(tableName, null, predicate, commandTimeout);
        public IMultipleResultReader GetMultiple(string tableName, string schemaName, GetMultiplePredicate predicate = null, int? commandTimeout = null)
            => _dapper.GetMultiple(Connection, predicate, _transaction, commandTimeout, tableName, schemaName);




        public async Task<IMultipleResultReader> GetMultipleAsync(GetMultiplePredicate predicate = null, int? commandTimeout = null)
        => await GetMultipleAsync(null, predicate, commandTimeout);

        public async Task<IMultipleResultReader> GetMultipleAsync(string tableName, GetMultiplePredicate predicate = null, int? commandTimeout = null)
        => await GetMultipleAsync(tableName, null, predicate, commandTimeout);

        public async Task<IMultipleResultReader> GetMultipleAsync(string tableName, string schemaName, GetMultiplePredicate predicate = null, int? commandTimeout = null)
        => await _dapper.GetMultipleAsync(Connection, predicate, _transaction, commandTimeout, tableName, schemaName);

        #endregion


        #region GetPage
        public IEnumerable<T> GetPage<T>(int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null, bool buffered = true) where T : class
           => GetPage<T>((string)null, page, resultsPerPage, predicate, sort, commandTimeout);

        public IEnumerable<T> GetPage<T>(string tableName, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null, bool buffered = true) where T : class
            => GetPage<T>(tableName, null, page, resultsPerPage, predicate, sort, commandTimeout);

        public IEnumerable<T> GetPage<T>(string tableName, string schemaName, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null, bool buffered = true) where T : class
            => _dapper.GetPage<T>(Connection, predicate, sort, page, resultsPerPage, _transaction, commandTimeout, buffered, tableName, schemaName, null, null);


        public async Task<IEnumerable<T>> GetPageAsync<T>(int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null) where T : class
            => await GetPageAsync<T>((string)null, page, resultsPerPage, predicate, sort, commandTimeout);

        public async Task<IEnumerable<T>> GetPageAsync<T>(string tableName, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null) where T : class
            => await GetPageAsync<T>(tableName, null, page, resultsPerPage, predicate, sort, commandTimeout);

        public async Task<IEnumerable<T>> GetPageAsync<T>(string tableName, string schemaName, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null) where T : class
            => await _dapper.GetPageAsync<T>(Connection, predicate, sort, page, resultsPerPage, _transaction, commandTimeout, tableName, schemaName, null, null);

        public IEnumerable<T> GetPage<T>(IList<IJoinPredicate> join, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null, bool buffered = true) where T : class
           => GetPage<T>(join, null, page, resultsPerPage, predicate, sort, commandTimeout);
        public IEnumerable<T> GetPage<T>(IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null, bool buffered = true) where T : class
            => _dapper.GetPage<T>(Connection, predicate, sort, page, resultsPerPage, _transaction, commandTimeout, buffered, null, null, join, alias);
        public async Task<IEnumerable<T>> GetPageAsync<T>(IList<IJoinPredicate> join, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null) where T : class
          => await GetPageAsync<T>(join, null, page, resultsPerPage, predicate, sort, commandTimeout);
        public async Task<IEnumerable<T>> GetPageAsync<T>(IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null) where T : class
          => await _dapper.GetPageAsync<T>(Connection, predicate, sort, page, resultsPerPage, _transaction, commandTimeout, null, null, join, alias);

        #endregion

        #region GetPages

        public Page<T> GetPages<T>(int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null) where T : class
           => GetPages<T>((string)null, page, resultsPerPage, predicate, sort, commandTimeout);

        public Page<T> GetPages<T>(string tableName, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null) where T : class
            => GetPages<T>(tableName, null, page, resultsPerPage, predicate, sort, commandTimeout);

        public Page<T> GetPages<T>(string tableName, string schemaName, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null) where T : class
            => _dapper.GetPages<T>(Connection, predicate, sort, page, resultsPerPage, _transaction, commandTimeout, tableName, schemaName, null, null);


        public async Task<Page<T>> GetPagesAsync<T>(int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null) where T : class
            => await GetPagesAsync<T>((string)null, page, resultsPerPage, predicate, sort, commandTimeout);

        public async Task<Page<T>> GetPagesAsync<T>(string tableName, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null) where T : class
            => await GetPagesAsync<T>(tableName, null, page, resultsPerPage, predicate, sort, commandTimeout);

        public async Task<Page<T>> GetPagesAsync<T>(string tableName, string schemaName, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null) where T : class
            => await _dapper.GetPagesAsync<T>(Connection, predicate, sort, page, resultsPerPage, _transaction, commandTimeout, tableName, schemaName, null, null);

        public Page<T> GetPages<T>(IList<IJoinPredicate> join, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null) where T : class
           => GetPages<T>(join, null, page, resultsPerPage, predicate, sort, commandTimeout);

        public Page<T> GetPages<T>(IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null) where T : class
           => _dapper.GetPages<T>(Connection, predicate, sort, page, resultsPerPage, _transaction, commandTimeout, null, null, join, alias);
        public async Task<Page<T>> GetPagesAsync<T>(IList<IJoinPredicate> join, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null) where T : class
           => await GetPagesAsync<T>(join, null, page, resultsPerPage, predicate, sort, commandTimeout);
        public async Task<Page<T>> GetPagesAsync<T>(IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, int? commandTimeout = null) where T : class
             => await _dapper.GetPagesAsync<T>(Connection, predicate, sort, page, resultsPerPage, _transaction, commandTimeout, null, null, join, alias);


        #endregion

        #region GetSet
        public IEnumerable<T> GetSet<T>(object predicate = null, IList<ISort> sort = null, int firstResult = 1, int maxResults = 10, int? commandTimeout = null, bool buffered = true) where T : class
           => GetSet<T>((string)null, predicate, sort, firstResult, maxResults, commandTimeout, buffered);

        public IEnumerable<T> GetSet<T>(string tableName, object predicate = null, IList<ISort> sort = null, int firstResult = 1, int maxResults = 10, int? commandTimeout = null, bool buffered = true) where T : class
            => GetSet<T>(tableName, null, predicate, sort, firstResult, maxResults, commandTimeout, buffered);

        public IEnumerable<T> GetSet<T>(string tableName, string schemaName, object predicate = null, IList<ISort> sort = null, int firstResult = 1, int maxResults = 10, int? commandTimeout = null, bool buffered = true) where T : class
            => _dapper.GetSet<T>(Connection, predicate, sort, firstResult, maxResults, _transaction, commandTimeout, buffered, tableName, schemaName, null, null);


        public async Task<IEnumerable<T>> GetSetAsync<T>(object predicate = null, IList<ISort> sort = null, int firstResult = 1, int maxResults = 10, int? commandTimeout = null) where T : class
            => await GetSetAsync<T>((string)null, predicate, sort, firstResult, maxResults, commandTimeout);

        public async Task<IEnumerable<T>> GetSetAsync<T>(string tableName, object predicate = null, IList<ISort> sort = null, int firstResult = 1, int maxResults = 10, int? commandTimeout = null) where T : class
            => await GetSetAsync<T>(tableName, null, predicate, sort, firstResult, maxResults, commandTimeout);

        public async Task<IEnumerable<T>> GetSetAsync<T>(string tableName, string schemaName, object predicate = null, IList<ISort> sort = null, int firstResult = 1, int maxResults = 10, int? commandTimeout = null) where T : class
           => await _dapper.GetSetAsync<T>(Connection, predicate, sort, firstResult, maxResults, _transaction, commandTimeout, tableName, schemaName, null, null);

        public IEnumerable<T> GetSet<T>(IList<IJoinPredicate> join, object predicate = null, IList<ISort> sort = null, int firstResult = 1, int maxResults = 10, int? commandTimeout = null, bool buffered = true) where T : class
           => GetSet<T>(join, null, predicate, sort, firstResult, maxResults, commandTimeout, buffered);

        public IEnumerable<T> GetSet<T>(IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias, object predicate = null, IList<ISort> sort = null, int firstResult = 1, int maxResults = 10, int? commandTimeout = null, bool buffered = true) where T : class
            => _dapper.GetSet<T>(Connection, predicate, sort, firstResult, maxResults, _transaction, commandTimeout, buffered, null, null, join, alias);

        public async Task<IEnumerable<T>> GetSetAsync<T>(IList<IJoinPredicate> join, object predicate = null, IList<ISort> sort = null, int firstResult = 1, int maxResults = 10, int? commandTimeout = null) where T : class
           => await GetSetAsync<T>(join, null, predicate, sort, firstResult, maxResults, commandTimeout);

        public async Task<IEnumerable<T>> GetSetAsync<T>(IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias, object predicate = null, IList<ISort> sort = null, int firstResult = 1, int maxResults = 10, int? commandTimeout = null) where T : class
           => await _dapper.GetSetAsync<T>(Connection, predicate, sort, firstResult, maxResults, _transaction, commandTimeout, null, null, join, alias);

        #endregion

        #region Insert
        public void Insert<T>(IEnumerable<T> entities, int? commandTimeout = null) where T : class
           => Insert<T>(null, entities, commandTimeout);

        public void Insert<T>(string tableName, IEnumerable<T> entities, int? commandTimeout = null) where T : class
            => Insert<T>(tableName, null, entities, commandTimeout);

        public void Insert<T>(string tableName, string schemaName, IEnumerable<T> entities, int? commandTimeout = null) where T : class
            => _dapper.Insert<T>(Connection, entities, _transaction, commandTimeout, tableName, schemaName);

        public dynamic Insert<T>(T entity, int? commandTimeout = null) where T : class
            => Insert<T>(null, entity, commandTimeout);

        public dynamic Insert<T>(string tableName, T entity, int? commandTimeout = null) where T : class
            => Insert<T>(tableName, null, entity, commandTimeout);

        public dynamic Insert<T>(string tableName, string schemaName, T entity, int? commandTimeout = null) where T : class
            => _dapper.Insert<T>(Connection, entity, _transaction, commandTimeout, tableName, schemaName);
        #endregion

        #region Update

        public bool Update<T>(T entity, object predicate = null, int? commandTimeout = null, bool ignoreAllKeyProperties = false) where T : class
           => Update<T>(null, entity, predicate, commandTimeout, ignoreAllKeyProperties);

        public bool Update<T>(string tableName, T entity, object predicate = null, int? commandTimeout = null, bool ignoreAllKeyProperties = false) where T : class
            => Update<T>(tableName, null, entity, predicate, commandTimeout, ignoreAllKeyProperties);

        public bool Update<T>(string tableName, string schemaName, T entity, object predicate = null, int? commandTimeout = null, bool ignoreAllKeyProperties = false) where T : class
            => _dapper.Update<T>(Connection, entity, predicate, _transaction, commandTimeout, tableName, schemaName, ignoreAllKeyProperties);



        public bool UpdateSet<T>(object entity, object predicate = null, int? commandTimeout = null) where T : class
           => UpdateSet<T>(null, entity, predicate, commandTimeout);

        public bool UpdateSet<T>(string tableName, object entity, object predicate = null, int? commandTimeout = null) where T : class
            => UpdateSet<T>(tableName, null, entity, predicate, commandTimeout);

        public bool UpdateSet<T>(string tableName, string schemaName, object entity, object predicate = null, int? commandTimeout = null) where T : class
            => _dapper.UpdateSet<T>(Connection, entity, predicate, _transaction, commandTimeout, tableName, schemaName);




        public async Task<bool> UpdateAsync<T>(T entity, object predicate, int? commandTimeout = null, bool ignoreAllKeyProperties = false) where T : class
            => await UpdateAsync<T>(null, entity, predicate, commandTimeout, ignoreAllKeyProperties);

        public async Task<bool> UpdateAsync<T>(string tableName, T entity, object predicate, int? commandTimeout = null, bool ignoreAllKeyProperties = false) where T : class
            => await UpdateAsync<T>(tableName, null, entity, predicate, commandTimeout, ignoreAllKeyProperties);

        public async Task<bool> UpdateAsync<T>(string tableName, string schemaName, T entity, object predicate, int? commandTimeout = null, bool ignoreAllKeyProperties = false) where T : class
            => await _dapper.UpdateAsync<T>(Connection, entity, predicate, _transaction, commandTimeout, tableName, schemaName, ignoreAllKeyProperties);



        public async Task<bool> UpdateSetAsync<T>(object entity, object predicate = null, int? commandTimeout = null) where T : class
          => await UpdateSetAsync<T>(null, entity, predicate, commandTimeout);

        public async Task<bool> UpdateSetAsync<T>(string tableName, object entity, object predicate = null, int? commandTimeout = null) where T : class
            => await UpdateSetAsync<T>(tableName, null, entity, predicate, commandTimeout);

        public async Task<bool> UpdateSetAsync<T>(string tableName, string schemaName, object entity, object predicate = null, int? commandTimeout = null) where T : class
            => await _dapper.UpdateSetAsync<T>(Connection, entity, predicate, _transaction, commandTimeout, tableName, schemaName);


        #endregion

    }
}
