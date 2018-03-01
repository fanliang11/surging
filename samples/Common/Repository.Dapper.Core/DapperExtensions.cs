using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using Repository.Dapper.Core.Sql;
using Repository.Dapper.Core.Mapper;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Repository.Dapper.Core
{
    public static class DapperExtensions
    {
        #region other
        private readonly static object _lock = new object();

        private static Func<IDapperExtensionsConfiguration, IDapperImplementor> _instanceFactory;
        private static IDapperImplementor _instance;
        private static IDapperExtensionsConfiguration _configuration;

        /// <summary>
        /// Gets or sets the default class mapper to use when generating class maps. If not specified, AutoClassMapper<T> is used.
        /// Repository.Dapper.Core.Configure(Type, IList<Assembly>, ISqlDialect) can be used instead to set all values at once
        /// </summary>
        public static Type DefaultMapper
        {
            get
            {
                return _configuration.DefaultMapper;
            }

            set
            {
                Configure(value, _configuration.MappingAssemblies, _configuration.Dialect);
            }
        }

        /// <summary>
        /// Gets or sets the type of sql to be generated.
        /// DapperExtensions.Configure(Type, IList<Assembly>, ISqlDialect) can be used instead to set all values at once
        /// </summary>
        public static ISqlDialect SqlDialect
        {
            get
            {
                return _configuration.Dialect;
            }

            set
            {
                Configure(_configuration.DefaultMapper, _configuration.MappingAssemblies, value);
            }
        }

        /// <summary>
        /// Get or sets the Dapper Extensions Implementation Factory.
        /// </summary>
        public static Func<IDapperExtensionsConfiguration, IDapperImplementor> InstanceFactory
        {
            get
            {
                if (_instanceFactory == null)
                {
                    _instanceFactory = config => new DapperImplementor(new SqlGeneratorImpl(config));
                }

                return _instanceFactory;
            }
            set
            {
                _instanceFactory = value;
                Configure(_configuration.DefaultMapper, _configuration.MappingAssemblies, _configuration.Dialect);
            }
        }

        /// <summary>
        /// Gets the Dapper Extensions Implementation
        /// </summary>
        private static IDapperImplementor Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = InstanceFactory(_configuration);
                        }
                    }
                }

                return _instance;
            }
        }

        //static DapperExtensions()
        //{
        //    Configure(typeof(AutoClassMapper<>), new List<Assembly>(), new SqlServerDialect());
        //}

        /// <summary>
        /// Add other assemblies that Dapper Extensions will search if a mapping is not found in the same assembly of the POCO.
        /// </summary>
        /// <param name="assemblies"></param>
        public static void SetMappingAssemblies(IList<Assembly> assemblies)
        {
            Configure(_configuration.DefaultMapper, assemblies, _configuration.Dialect);
        }

        /// <summary>
        /// Configure DapperExtensions extension methods.
        /// </summary>
        /// <param name="defaultMapper"></param>
        /// <param name="mappingAssemblies"></param>
        /// <param name="sqlDialect"></param>
        public static void Configure(IDapperExtensionsConfiguration configuration)
        {
            _instance = null;
            _configuration = configuration;
        }

        /// <summary>
        /// Configure DapperExtensions extension methods.
        /// </summary>
        /// <param name="defaultMapper"></param>
        /// <param name="mappingAssemblies"></param>
        /// <param name="sqlDialect"></param>
        public static void Configure(Type defaultMapper, IList<Assembly> mappingAssemblies, ISqlDialect sqlDialect)
        {
            Configure(new DapperExtensionsConfiguration(defaultMapper, mappingAssemblies, sqlDialect));
        }

        public static void Configure(ISqlDialect sqlDialect, Type defaultMapper = null, IList<Assembly> mappingAssemblies = null)
        {
            defaultMapper = defaultMapper ?? typeof(AutoClassMapper<>);
            mappingAssemblies = mappingAssemblies ?? new List<Assembly>();
            Configure(new DapperExtensionsConfiguration(defaultMapper, mappingAssemblies, sqlDialect));
        }

        #endregion

        #region Get
        /// <summary>
        /// Executes a query for the specified id, returning the data typed as per T
        /// </summary>
        public static T Get<T>(this IDbConnection connection, dynamic id, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        => Get<T>(connection, null, id, transaction, commandTimeout);

        public static T Get<T>(this IDbConnection connection, string tableName, dynamic id, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        => Get<T>(connection, tableName, null, id, transaction, commandTimeout);

        public static T Get<T>(this IDbConnection connection, string tableName, string schemaName, dynamic id, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
         => (T)Instance.Get<T>(connection, id, transaction, commandTimeout, tableName, schemaName, null, null);

        public static async Task<T> GetAsync<T>(this IDbConnection connection, dynamic id, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
            => await GetAsync<T>(connection, null, id, transaction, commandTimeout);

        public static async Task<T> GetAsync<T>(this IDbConnection connection, string tableName, dynamic id, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
         => await GetAsync<T>(connection, tableName, null, id, transaction, commandTimeout);

        public static async Task<T> GetAsync<T>(this IDbConnection connection, string tableName, string schemaName, dynamic id, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
         => await Instance.GetAsync<T>(connection, id, transaction, commandTimeout, tableName, schemaName, null, null);

        public static T Get<T>(this IDbConnection connection, IList<IJoinPredicate> join, dynamic id, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        => Get<T>(connection, join, null, id, transaction, commandTimeout);

        public static T Get<T>(this IDbConnection connection, IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias, dynamic id, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
         => (T)Instance.Get<T>(connection, id, transaction, commandTimeout, null, null, join, alias);

        public static async Task<T> GetAsync<T>(this IDbConnection connection, IList<IJoinPredicate> join, dynamic id, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
           => await GetAsync<T>(connection, join, null, id, transaction, commandTimeout);

        public static async Task<T> GetAsync<T>(this IDbConnection connection, IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias, dynamic id, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
           => await Instance.GetAsync<T>(connection, id, transaction, commandTimeout, null, null, join, alias);

        #endregion

        #region Insert
        /// <summary>
        /// Executes an insert query for the specified entity.
        /// </summary>
        public static void Insert<T>(this IDbConnection connection, IEnumerable<T> entities, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
            => Insert<T>(connection, null, entities, transaction, commandTimeout);

        public static void Insert<T>(this IDbConnection connection, string tableName, IEnumerable<T> entities, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
            => Insert<T>(connection, tableName, null, entities, transaction, commandTimeout);

        public static void Insert<T>(this IDbConnection connection, string tableName, string schemaName, IEnumerable<T> entities, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
            => Instance.Insert<T>(connection, entities, transaction, commandTimeout, tableName, schemaName);


        /// <summary>
        /// Executes an insert query for the specified entity, returning the primary key.  
        /// If the entity has a single key, just the value is returned.  
        /// If the entity has a composite key, an IDictionary&lt;string, object&gt; is returned with the key values.
        /// The key value for the entity will also be updated if the KeyType is a Guid or Identity.
        /// </summary>
        public static dynamic Insert<T>(this IDbConnection connection, T entity, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
            => Insert<T>(connection, null, entity, transaction, commandTimeout);

        public static dynamic Insert<T>(this IDbConnection connection, string tableName, T entity, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
            => Insert<T>(connection, tableName, null, entity, transaction, commandTimeout);

        public static dynamic Insert<T>(this IDbConnection connection, string tableName, string schemaName, T entity, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
            => Instance.Insert<T>(connection, entity, transaction, commandTimeout, tableName, schemaName);

        #endregion

        #region Update
        /// <summary>
        /// Executes an update query for the specified entity.
        /// </summary>
        public static bool Update<T>(this IDbConnection connection, T entity, IDbTransaction transaction = null, int? commandTimeout = null, bool ignoreAllKeyProperties = false) where T : class
            => Update<T>(connection, null, entity, transaction, commandTimeout, ignoreAllKeyProperties);

        public static bool Update<T>(this IDbConnection connection, string tableName, T entity, IDbTransaction transaction = null, int? commandTimeout = null, bool ignoreAllKeyProperties = false) where T : class
            => Update<T>(connection, tableName, null, entity, transaction, commandTimeout, ignoreAllKeyProperties);

        public static bool Update<T>(this IDbConnection connection, string tableName, string schemaName, T entity, IDbTransaction transaction = null, int? commandTimeout = null, bool ignoreAllKeyProperties = false) where T : class
        => Instance.Update<T>(connection, entity, null, transaction, commandTimeout, tableName, schemaName, ignoreAllKeyProperties);


        public static bool Update<T>(this IDbConnection connection, T entity, object predicate, IDbTransaction transaction = null, int? commandTimeout = null, bool ignoreAllKeyProperties = false) where T : class
            => Update<T>(connection, null, entity, predicate, transaction, commandTimeout, ignoreAllKeyProperties);

        public static bool Update<T>(this IDbConnection connection, string tableName, T entity, object predicate, IDbTransaction transaction = null, int? commandTimeout = null, bool ignoreAllKeyProperties = false) where T : class
            => Update<T>(connection, tableName, null, entity, predicate, transaction, commandTimeout, ignoreAllKeyProperties);

        public static bool Update<T>(this IDbConnection connection, string tableName, string schemaName, T entity, object predicate, IDbTransaction transaction = null, int? commandTimeout = null, bool ignoreAllKeyProperties = false) where T : class
        => Instance.Update<T>(connection, entity, predicate, transaction, commandTimeout, tableName, schemaName, ignoreAllKeyProperties);


        public static async Task<bool> UpdateAsync<T>(this IDbConnection connection, T entity, IDbTransaction transaction = null, int? commandTimeout = null, bool ignoreAllKeyProperties = false) where T : class
            => await UpdateAsync<T>(connection, null, entity, transaction, commandTimeout, ignoreAllKeyProperties);

        public static async Task<bool> UpdateAsync<T>(this IDbConnection connection, string tableName, T entity, IDbTransaction transaction = null, int? commandTimeout = null, bool ignoreAllKeyProperties = false) where T : class
            => await UpdateAsync<T>(connection, tableName, null, entity, transaction, commandTimeout, ignoreAllKeyProperties);

        public static async Task<bool> UpdateAsync<T>(this IDbConnection connection, string tableName, string schemaName, T entity, IDbTransaction transaction = null, int? commandTimeout = null, bool ignoreAllKeyProperties = false) where T : class
        => await Instance.UpdateAsync<T>(connection, entity, null, transaction, commandTimeout, tableName, schemaName, ignoreAllKeyProperties);

        public static async Task<bool> UpdateAsync<T>(this IDbConnection connection, T entity, object predicate, IDbTransaction transaction = null, int? commandTimeout = null, bool ignoreAllKeyProperties = false) where T : class
            => await UpdateAsync<T>(connection, null, entity, predicate, transaction, commandTimeout, ignoreAllKeyProperties);

        public static async Task<bool> UpdateAsync<T>(this IDbConnection connection, string tableName, T entity, object predicate, IDbTransaction transaction = null, int? commandTimeout = null, bool ignoreAllKeyProperties = false) where T : class
            => await UpdateAsync<T>(connection, tableName, null, entity, predicate, transaction, commandTimeout, ignoreAllKeyProperties);

        public static async Task<bool> UpdateAsync<T>(this IDbConnection connection, string tableName, string schemaName, T entity, object predicate, IDbTransaction transaction = null, int? commandTimeout = null, bool ignoreAllKeyProperties = false) where T : class
        => await Instance.UpdateAsync<T>(connection, entity, predicate, transaction, commandTimeout, tableName, schemaName, ignoreAllKeyProperties);



        public static bool UpdateSet<T>(this IDbConnection connection, object entity, object predicate = null, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
            => UpdateSet<T>(connection, null, entity, predicate, transaction, commandTimeout);

        public static bool UpdateSet<T>(this IDbConnection connection, string tableName, object entity, object predicate = null, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
            => UpdateSet<T>(connection, tableName, null, entity, predicate, transaction, commandTimeout);

        public static bool UpdateSet<T>(this IDbConnection connection, string tableName, string schemaName, object entity, object predicate = null, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        => Instance.UpdateSet<T>(connection, entity, predicate, transaction, commandTimeout, tableName, schemaName);


        public static async Task<bool> UpdateSetAsync<T>(this IDbConnection connection, object entity, object predicate = null, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
           => await UpdateSetAsync<T>(connection, null, entity, predicate, transaction, commandTimeout);

        public static async Task<bool> UpdateSetAsync<T>(this IDbConnection connection, string tableName, object entity, object predicate = null, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
            => await UpdateSetAsync<T>(connection, tableName, null, entity, predicate, transaction, commandTimeout);

        public static async Task<bool> UpdateSetAsync<T>(this IDbConnection connection, string tableName, string schemaName, object entity, object predicate = null, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        => await Instance.UpdateSetAsync<T>(connection, entity, predicate, transaction, commandTimeout, tableName, schemaName);


        #endregion

        #region Delete
        /// <summary>
        /// Executes a delete query for the specified entity.
        /// </summary>
        public static bool Delete<T>(this IDbConnection connection, T entity, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
            => Delete<T>(connection, null, entity, transaction, commandTimeout);

        public static bool Delete<T>(this IDbConnection connection, string tableName, T entity, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
            => Delete<T>(connection, tableName, null, entity, transaction, commandTimeout);

        public static bool Delete<T>(this IDbConnection connection, string tableName, string schemaName, T entity, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        => Instance.Delete<T>(connection, entity, transaction, commandTimeout, tableName, schemaName);


        public static async Task<bool> DeleteAsync<T>(this IDbConnection connection, T entity, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
            => await DeleteAsync<T>(connection, null, entity, transaction, commandTimeout);

        public static async Task<bool> DeleteAsync<T>(this IDbConnection connection, string tableName, T entity, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
            => await DeleteAsync<T>(connection, tableName, null, entity, transaction, commandTimeout);

        public static async Task<bool> DeleteAsync<T>(this IDbConnection connection, string tableName, string schemaName, T entity, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        => await Instance.DeleteAsync<T>(connection, entity, transaction, commandTimeout, tableName, schemaName);


        /// <summary>
        /// Executes a delete query using the specified predicate.
        /// </summary>
        public static bool Delete<T>(this IDbConnection connection, object predicate, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
            => Delete<T>(connection, null, predicate, transaction, commandTimeout);

        public static bool Delete<T>(this IDbConnection connection, string tableName, object predicate, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
            => Delete<T>(connection, tableName, null, predicate, transaction, commandTimeout);

        public static bool Delete<T>(this IDbConnection connection, string tableName, string schemaName, object predicate, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        => Instance.Delete<T>(connection, predicate, transaction, commandTimeout, tableName, schemaName);

        public static async Task<bool> DeleteAsync<T>(this IDbConnection connection, object predicate, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
           => await DeleteAsync<T>(connection, null, predicate, transaction, commandTimeout);

        public static async Task<bool> DeleteAsync<T>(this IDbConnection connection, string tableName, object predicate, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
            => await DeleteAsync<T>(connection, tableName, null, predicate, transaction, commandTimeout);

        public static async Task<bool> DeleteAsync<T>(this IDbConnection connection, string tableName, string schemaName, object predicate, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        => await Instance.DeleteAsync<T>(connection, predicate, transaction, commandTimeout, tableName, schemaName);


        #endregion

        #region GetList
        /// <summary>
        /// Executes a select query using the specified predicate, returning an IEnumerable data typed as per T.
        /// </summary>
        public static IEnumerable<T> GetList<T>(this IDbConnection connection, object predicate = null, IList<ISort> sort = null, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false) where T : class
            => GetList<T>(connection, (string)null, null, predicate, sort, transaction, commandTimeout, buffered);

        public static IEnumerable<T> GetList<T>(this IDbConnection connection, string tableName, object predicate = null, IList<ISort> sort = null, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false) where T : class
            => GetList<T>(connection, tableName, null, predicate, sort, transaction, commandTimeout, buffered);

        public static IEnumerable<T> GetList<T>(this IDbConnection connection, string tableName, string schemaName, object predicate = null, IList<ISort> sort = null, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false) where T : class
            => Instance.GetList<T>(connection, predicate, sort, transaction, commandTimeout, buffered, tableName, schemaName, null, null);


        public static async Task<IEnumerable<T>> GetListAsync<T>(this IDbConnection connection, object predicate = null, IList<ISort> sort = null, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
            => await GetListAsync<T>(connection, (string)null, null, predicate, sort, transaction, commandTimeout);

        public static async Task<IEnumerable<T>> GetListAsync<T>(this IDbConnection connection, string tableName, object predicate = null, IList<ISort> sort = null, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
            => await GetListAsync<T>(connection, tableName, null, predicate, sort, transaction, commandTimeout);

        public static async Task<IEnumerable<T>> GetListAsync<T>(this IDbConnection connection, string tableName, string schemaName, object predicate = null, IList<ISort> sort = null, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
            => await Instance.GetListAsync<T>(connection, predicate, sort, transaction, commandTimeout, tableName, schemaName, null, null);


        public static IEnumerable<T> GetList<T>(this IDbConnection connection, IList<IJoinPredicate> join, object predicate = null, IList<ISort> sort = null, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false) where T : class
           => GetList<T>(connection, join, null, predicate, sort, transaction, commandTimeout, buffered);

        public static IEnumerable<T> GetList<T>(this IDbConnection connection, IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias, object predicate = null, IList<ISort> sort = null, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false) where T : class
            => Instance.GetList<T>(connection, predicate, sort, transaction, commandTimeout, buffered, null, null, join, alias);

        public static async Task<IEnumerable<T>> GetListAsync<T>(this IDbConnection connection, IList<IJoinPredicate> join, object predicate = null, IList<ISort> sort = null, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
          => await GetListAsync<T>(connection, join, null, predicate, sort, transaction, commandTimeout);

        public static async Task<IEnumerable<T>> GetListAsync<T>(this IDbConnection connection, IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias, object predicate = null, IList<ISort> sort = null, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
            => await Instance.GetListAsync<T>(connection, predicate, sort, transaction, commandTimeout, null, null, join, alias);


        #endregion

        #region GetPage
        /// <summary>
        /// Executes a select query using the specified predicate, returning an IEnumerable data typed as per T.
        /// Data returned is dependent upon the specified page and resultsPerPage.
        /// </summary>
        public static IEnumerable<T> GetPage<T>(this IDbConnection connection, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false) where T : class
            => GetPage<T>(connection, (string)null, page, resultsPerPage, predicate, sort, transaction, commandTimeout, buffered);

        public static IEnumerable<T> GetPage<T>(this IDbConnection connection, string tableName, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false) where T : class
            => GetPage<T>(connection, tableName, null, page, resultsPerPage, predicate, sort, transaction, commandTimeout, buffered);

        public static IEnumerable<T> GetPage<T>(this IDbConnection connection, string tableName, string schemaName, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false) where T : class
            => Instance.GetPage<T>(connection, predicate, sort, page, resultsPerPage, transaction, commandTimeout, buffered, tableName, schemaName, null, null);


        public static async Task<IEnumerable<T>> GetPageAsync<T>(this IDbConnection connection, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
            => await GetPageAsync<T>(connection, (string)null, page, resultsPerPage, predicate, sort, transaction, commandTimeout);

        public static async Task<IEnumerable<T>> GetPageAsync<T>(this IDbConnection connection, string tableName, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
            => await GetPageAsync<T>(connection, tableName, null, page, resultsPerPage, predicate, sort, transaction, commandTimeout);

        public static async Task<IEnumerable<T>> GetPageAsync<T>(this IDbConnection connection, string tableName, string schemaName, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
            => await Instance.GetPageAsync<T>(connection, predicate, sort, page, resultsPerPage, transaction, commandTimeout, tableName, schemaName, null, null);

        public static IEnumerable<T> GetPage<T>(this IDbConnection connection, IList<IJoinPredicate> join, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false) where T : class
           => GetPage<T>(connection, join, null, page, resultsPerPage, predicate, sort, transaction, commandTimeout, buffered);

        public static IEnumerable<T> GetPage<T>(this IDbConnection connection, IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false) where T : class
            => Instance.GetPage<T>(connection, predicate, sort, page, resultsPerPage, transaction, commandTimeout, buffered, null, null, join, alias);

        public static async Task<IEnumerable<T>> GetPageAsync<T>(this IDbConnection connection, IList<IJoinPredicate> join, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
           => await GetPageAsync<T>(connection, join, null, page, resultsPerPage, predicate, sort, transaction, commandTimeout);

        public static async Task<IEnumerable<T>> GetPageAsync<T>(this IDbConnection connection, IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
             => await Instance.GetPageAsync<T>(connection, predicate, sort, page, resultsPerPage, transaction, commandTimeout, null, null, join, alias);
        #endregion

        #region GetPages

        /// <summary>
        /// Executes a select query using the specified predicate, returning an IEnumerable data typed as per T.
        /// Data returned is dependent upon the specified page and resultsPerPage.
        /// </summary>
        public static Page<T> GetPages<T>(this IDbConnection connection, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
            => GetPages<T>(connection, (string)null, page, resultsPerPage, predicate, sort, transaction, commandTimeout);

        public static Page<T> GetPages<T>(this IDbConnection connection, string tableName, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
            => GetPages<T>(connection, tableName, null, page, resultsPerPage, predicate, sort, transaction, commandTimeout);

        public static Page<T> GetPages<T>(this IDbConnection connection, string tableName, string schemaName, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
            => Instance.GetPages<T>(connection, predicate, sort, page, resultsPerPage, transaction, commandTimeout, tableName, schemaName, null, null);

        public static async Task<Page<T>> GetPagesAsync<T>(this IDbConnection connection, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
           => await GetPagesAsync<T>(connection, (string)null, page, resultsPerPage, predicate, sort, transaction, commandTimeout);

        public static async Task<Page<T>> GetPagesAsync<T>(this IDbConnection connection, string tableName, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
            => await GetPagesAsync<T>(connection, tableName, null, page, resultsPerPage, predicate, sort, transaction, commandTimeout);

        public static async Task<Page<T>> GetPagesAsync<T>(this IDbConnection connection, string tableName, string schemaName, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
            => await Instance.GetPagesAsync<T>(connection, predicate, sort, page, resultsPerPage, transaction, commandTimeout, tableName, schemaName, null, null);

        public static Page<T> GetPages<T>(this IDbConnection connection, IList<IJoinPredicate> join, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
           => GetPages<T>(connection, join, null, page, resultsPerPage, predicate, sort, transaction, commandTimeout);

        public static Page<T> GetPages<T>(this IDbConnection connection, IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
            => Instance.GetPages<T>(connection, predicate, sort, page, resultsPerPage, transaction, commandTimeout, null, null, join, alias);

        public static async Task<Page<T>> GetPagesAsync<T>(this IDbConnection connection, IList<IJoinPredicate> join, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
             => await GetPagesAsync<T>(connection, join, null, page, resultsPerPage, predicate, sort, transaction, commandTimeout);

        public static async Task<Page<T>> GetPagesAsync<T>(this IDbConnection connection, IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias, int page = 1, int resultsPerPage = 10, object predicate = null, IList<ISort> sort = null, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
            => await Instance.GetPagesAsync<T>(connection, predicate, sort, page, resultsPerPage, transaction, commandTimeout, null, null, join, alias);


        #endregion

        #region GetSet
        /// <summary>
        /// Executes a select query using the specified predicate, returning an IEnumerable data typed as per T.
        /// Data returned is dependent upon the specified firstResult and maxResults.
        /// </summary>
        public static IEnumerable<T> GetSet<T>(this IDbConnection connection, object predicate = null, IList<ISort> sort = null, int firstResult = 1, int maxResults = 10, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false) where T : class
            => GetSet<T>(connection, (string)null, predicate, sort, firstResult, maxResults, transaction, commandTimeout, buffered);

        public static IEnumerable<T> GetSet<T>(this IDbConnection connection, string tableName, object predicate = null, IList<ISort> sort = null, int firstResult = 1, int maxResults = 10, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false) where T : class
            => GetSet<T>(connection, tableName, null, predicate, sort, firstResult, maxResults, transaction, commandTimeout, buffered);

        public static IEnumerable<T> GetSet<T>(this IDbConnection connection, string tableName, string schemaName, object predicate = null, IList<ISort> sort = null, int firstResult = 1, int maxResults = 10, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false) where T : class
            => Instance.GetSet<T>(connection, predicate, sort, firstResult, maxResults, transaction, commandTimeout, buffered, tableName, schemaName, null, null);

        public static async Task<IEnumerable<T>> GetSetAsync<T>(this IDbConnection connection, object predicate = null, IList<ISort> sort = null, int firstResult = 1, int maxResults = 10, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
            => await GetSetAsync<T>(connection, (string)null, predicate, sort, firstResult, maxResults, transaction, commandTimeout);

        public static async Task<IEnumerable<T>> GetSetAsync<T>(this IDbConnection connection, string tableName, object predicate = null, IList<ISort> sort = null, int firstResult = 1, int maxResults = 10, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
            => await GetSetAsync<T>(connection, tableName, null, predicate, sort, firstResult, maxResults, transaction, commandTimeout);

        public static async Task<IEnumerable<T>> GetSetAsync<T>(this IDbConnection connection, string tableName, string schemaName, object predicate = null, IList<ISort> sort = null, int firstResult = 1, int maxResults = 10, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
            => await Instance.GetSetAsync<T>(connection, predicate, sort, firstResult, maxResults, transaction, commandTimeout, tableName, schemaName, null, null);

        public static IEnumerable<T> GetSet<T>(this IDbConnection connection, IList<IJoinPredicate> join, object predicate = null, IList<ISort> sort = null, int firstResult = 1, int maxResults = 10, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false) where T : class
            => GetSet<T>(connection, join, null, predicate, sort, firstResult, maxResults, transaction, commandTimeout, buffered);

        public static IEnumerable<T> GetSet<T>(this IDbConnection connection, IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias, object predicate = null, IList<ISort> sort = null, int firstResult = 1, int maxResults = 10, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false) where T : class
             => Instance.GetSet<T>(connection, predicate, sort, firstResult, maxResults, transaction, commandTimeout, buffered, null, null, join, alias);

        public static async Task<IEnumerable<T>> GetSetAsync<T>(this IDbConnection connection, IList<IJoinPredicate> join, object predicate = null, IList<ISort> sort = null, int firstResult = 1, int maxResults = 10, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
            => await GetSetAsync<T>(connection, join, null, predicate, sort, firstResult, maxResults, transaction, commandTimeout);

        public static async Task<IEnumerable<T>> GetSetAsync<T>(this IDbConnection connection, IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias, object predicate = null, IList<ISort> sort = null, int firstResult = 1, int maxResults = 10, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
             => await Instance.GetSetAsync<T>(connection, predicate, sort, firstResult, maxResults, transaction, commandTimeout, null, null, join, alias);

        #endregion

        #region Count
        /// <summary>
        /// Executes a query using the specified predicate, returning an integer that represents the number of rows that match the query.
        /// </summary>
        public static long Count<T>(this IDbConnection connection, object predicate = null, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
            => Count<T>(connection, (string)null, predicate, transaction, commandTimeout);

        public static long Count<T>(this IDbConnection connection, string tableName, object predicate = null, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
            => Count<T>(connection, tableName, null, predicate, transaction, commandTimeout);

        public static long Count<T>(this IDbConnection connection, string tableName, string schemaName, object predicate = null, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        => Instance.Count<T>(connection, predicate, transaction, commandTimeout, tableName, schemaName, null);


        public static async Task<long> CountAsync<T>(this IDbConnection connection, object predicate = null, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
            => await CountAsync<T>(connection, (string)null, predicate, transaction, commandTimeout);

        public static async Task<long> CountAsync<T>(this IDbConnection connection, string tableName, object predicate = null, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
            => await CountAsync<T>(connection, tableName, null, predicate, transaction, commandTimeout);

        public static async Task<long> CountAsync<T>(this IDbConnection connection, string tableName, string schemaName, object predicate = null, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        => await Instance.CountAsync<T>(connection, predicate, transaction, commandTimeout, tableName, schemaName, null);

        public static long Count<T>(this IDbConnection connection, IList<IJoinPredicate> join, object predicate = null, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
           => Instance.Count<T>(connection, predicate, transaction, commandTimeout, null, null, join);

        public static async Task<long> CountAsync<T>(this IDbConnection connection, IList<IJoinPredicate> join, object predicate = null, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
            => await Instance.CountAsync<T>(connection, predicate, transaction, commandTimeout, null, null, join);
        #endregion

        #region GetMultiple

        /// <summary>
        /// Executes a select query for multiple objects, returning IMultipleResultReader for each predicate.
        /// </summary>
        public static IMultipleResultReader GetMultiple(this IDbConnection connection, GetMultiplePredicate predicate = null, IDbTransaction transaction = null, int? commandTimeout = null)
            => GetMultiple(connection, null, null, predicate, transaction, commandTimeout);

        public static IMultipleResultReader GetMultiple(this IDbConnection connection, string tableName, GetMultiplePredicate predicate = null, IDbTransaction transaction = null, int? commandTimeout = null)
            => GetMultiple(connection, tableName, null, predicate, transaction, commandTimeout);

        public static IMultipleResultReader GetMultiple(this IDbConnection connection, string tableName, string schemaName, GetMultiplePredicate predicate = null, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return Instance.GetMultiple(connection, predicate, transaction, commandTimeout, tableName, schemaName);
        }

        public static async Task<IMultipleResultReader> GetMultipleAsync(this IDbConnection connection, GetMultiplePredicate predicate = null, IDbTransaction transaction = null, int? commandTimeout = null)
            => await GetMultipleAsync(connection, null, null, predicate, transaction, commandTimeout);

        public static async Task<IMultipleResultReader> GetMultipleAsync(this IDbConnection connection, string tableName, GetMultiplePredicate predicate = null, IDbTransaction transaction = null, int? commandTimeout = null)
            => await GetMultipleAsync(connection, tableName, null, predicate, transaction, commandTimeout);

        public static async Task<IMultipleResultReader> GetMultipleAsync(this IDbConnection connection, string tableName, string schemaName, GetMultiplePredicate predicate = null, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return await Instance.GetMultipleAsync(connection, predicate, transaction, commandTimeout, tableName, schemaName);
        }
        #endregion

        #region 
        /// <summary>
        /// Gets the appropriate mapper for the specified type T. 
        /// If the mapper for the type is not yet created, a new mapper is generated from the mapper type specifed by DefaultMapper.
        /// </summary>
        public static IClassMapper GetMap<T>() where T : class
        {
            return Instance.SqlGenerator.Configuration.GetMap<T>();
        }

        /// <summary>
        /// Clears the ClassMappers for each type.
        /// </summary>
        public static void ClearCache()
        {
            Instance.SqlGenerator.Configuration.ClearCache();
        }

        /// <summary>
        /// Generates a COMB Guid which solves the fragmented index issue.
        /// See: http://davybrion.com/blog/2009/05/using-the-guidcomb-identifier-strategy
        /// </summary>
        public static Guid GetNextGuid()
        {
            return Instance.SqlGenerator.Configuration.GetNextGuid();
        }


        #endregion
    }
}
