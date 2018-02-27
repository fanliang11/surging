using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using Dapper;
using Repository.Dapper.Core.Mapper;
using Repository.Dapper.Core.Sql;
using System.Threading.Tasks;

namespace Repository.Dapper.Core
{
    public interface IDapperImplementor
    {
        ISqlGenerator SqlGenerator { get; }

        #region Get
        T Get<T>(IDbConnection connection, dynamic id, IDbTransaction transaction, int? commandTimeout, string tableName, string schemaName, IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias) where T : class;
        Task<T> GetAsync<T>(IDbConnection connection, dynamic id, IDbTransaction transaction, int? commandTimeout, string tableName, string schemaName, IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias) where T : class;

        #endregion

        #region Insert
        void Insert<T>(IDbConnection connection, IEnumerable<T> entities, IDbTransaction transaction, int? commandTimeout, string tableName, string schemaName) where T : class;
        dynamic Insert<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout, string tableName, string schemaName) where T : class;

        #endregion

        #region Update
        bool Update<T>(IDbConnection connection, T entity, object predicate, IDbTransaction transaction, int? commandTimeout, string tableName, string schemaName, bool ignoreAllKeyProperties) where T : class;
        Task<bool> UpdateAsync<T>(IDbConnection connection, T entity, object predicate, IDbTransaction transaction, int? commandTimeout, string tableName, string schemaName, bool ignoreAllKeyProperties) where T : class;
        bool UpdateSet<T>(IDbConnection connection, object entity, object predicate, IDbTransaction transaction, int? commandTimeout, string tableName, string schemaName) where T : class;
        Task<bool> UpdateSetAsync<T>(IDbConnection connection, object entity, object predicate, IDbTransaction transaction, int? commandTimeout, string tableName, string schemaName) where T : class;

        #endregion

        #region Delete
        bool Delete<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout, string tableName, string schemaName) where T : class;
        Task<bool> DeleteAsync<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout, string tableName, string schemaName) where T : class;
        bool Delete<T>(IDbConnection connection, object predicate, IDbTransaction transaction, int? commandTimeout, string tableName, string schemaName) where T : class;
        Task<bool> DeleteAsync<T>(IDbConnection connection, object predicate, IDbTransaction transaction, int? commandTimeout, string tableName, string schemaName) where T : class;
        #endregion

        #region GetList
        IEnumerable<T> GetList<T>(IDbConnection connection, object predicate, IList<ISort> sort, IDbTransaction transaction, int? commandTimeout, bool buffered, string tableName, string schemaName, IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias) where T : class;

        Task<IEnumerable<T>> GetListAsync<T>(IDbConnection connection, object predicate, IList<ISort> sort, IDbTransaction transaction, int? commandTimeout, string tableName, string schemaName, IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias) where T : class;
        #endregion

        #region GetPage
        IEnumerable<T> GetPage<T>(IDbConnection connection, object predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction, int? commandTimeout, bool buffered, string tableName, string schemaName, IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias) where T : class;
        Task<IEnumerable<T>> GetPageAsync<T>(IDbConnection connection, object predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction, int? commandTimeout, string tableName, string schemaName, IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias) where T : class;
        #endregion

        #region GetPages
        Page<T> GetPages<T>(IDbConnection connection, object predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction, int? commandTimeout, string tableName, string schemaName, IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias) where T : class;
        Task<Page<T>> GetPagesAsync<T>(IDbConnection connection, object predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction, int? commandTimeout, string tableName, string schemaName, IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias) where T : class;
        #endregion

        #region GetSet
        IEnumerable<T> GetSet<T>(IDbConnection connection, object predicate, IList<ISort> sort, int firstResult, int maxResults, IDbTransaction transaction, int? commandTimeout, bool buffered, string tableName, string schemaName, IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias) where T : class;
        Task<IEnumerable<T>> GetSetAsync<T>(IDbConnection connection, object predicate, IList<ISort> sort, int firstResult, int maxResults, IDbTransaction transaction, int? commandTimeout, string tableName, string schemaName, IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias) where T : class;
        #endregion

        #region Count
        long Count<T>(IDbConnection connection, object predicate, IDbTransaction transaction, int? commandTimeout, string tableName, string schemaName, IList<IJoinPredicate> join) where T : class;
        Task<long> CountAsync<T>(IDbConnection connection, object predicate, IDbTransaction transaction, int? commandTimeout, string tableName, string schemaName, IList<IJoinPredicate> join) where T : class;
        #endregion

        #region GetMultiple
        IMultipleResultReader GetMultiple(IDbConnection connection, GetMultiplePredicate predicate, IDbTransaction transaction, int? commandTimeout, string tableName, string schemaName);

        Task<IMultipleResultReader> GetMultipleAsync(IDbConnection connection, GetMultiplePredicate predicate, IDbTransaction transaction, int? commandTimeout, string tableName, string schemaName);
        #endregion

    }

    public class DapperImplementor : IDapperImplementor
    {
        public DapperImplementor(ISqlGenerator sqlGenerator)
        {
            SqlGenerator = sqlGenerator;
        }

        public ISqlGenerator SqlGenerator { get; private set; }

        #region Get

        public T Get<T>(IDbConnection connection, dynamic id, IDbTransaction transaction, int? commandTimeout, string tableName, string schemaName, IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias) where T : class
        {
            IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
            IPredicate predicate = GetIdPredicate(classMap, id);
            T result = GetList<T>(connection, predicate, null, transaction, commandTimeout, true, tableName,schemaName, join, alias).SingleOrDefault();
            return result;
        }
        public async Task<T> GetAsync<T>(IDbConnection connection, dynamic id, IDbTransaction transaction, int? commandTimeout, string tableName, string schemaName, IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias) where T : class
        {
            IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
            IPredicate predicate = GetIdPredicate(classMap, id);
            return (await GetListAsync<T>(connection, predicate, null, transaction, commandTimeout, tableName, schemaName, join, alias)).SingleOrDefault();
        }

        #endregion

        #region Insert
        public void Insert<T>(IDbConnection connection, IEnumerable<T> entities, IDbTransaction transaction, int? commandTimeout, string tableName, string schemaName) where T : class
        {
            IEnumerable<PropertyInfo> properties = null;
            IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
            var notKeyProperties = classMap.Properties.Where(p => p.KeyType != KeyType.NotAKey);
            var triggerIdentityColumn = classMap.Properties.SingleOrDefault(p => p.KeyType == KeyType.TriggerIdentity);

            var parameters = new List<DynamicParameters>();
            if (triggerIdentityColumn != null)
            {
                properties = typeof(T).GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => p.Name != triggerIdentityColumn.PropertyInfo.Name);
            }

            foreach (var e in entities)
            {
                foreach (var column in notKeyProperties)
                {
                    if (column.KeyType == KeyType.Guid && (Guid)column.PropertyInfo.GetValue(e, null) == Guid.Empty)
                    {
                        Guid comb = SqlGenerator.Configuration.GetNextGuid();
                        column.PropertyInfo.SetValue(e, comb, null);
                    }
                }

                if (triggerIdentityColumn != null)
                {
                    var dynamicParameters = new DynamicParameters();
                    foreach (var prop in properties)
                    {
                        dynamicParameters.Add(prop.Name, prop.GetValue(e, null));
                    }

                    // defaultValue need for identify type of parameter
                    var defaultValue = typeof(T).GetProperty(triggerIdentityColumn.PropertyInfo.Name).GetValue(e, null);
                    dynamicParameters.Add("IdOutParam", direction: ParameterDirection.Output, value: defaultValue);

                    parameters.Add(dynamicParameters);
                }
            }

            string sql = SqlGenerator.Insert(classMap,schemaName, tableName);

            if (triggerIdentityColumn == null)
            {
                connection.Execute(sql, entities, transaction, commandTimeout, CommandType.Text);
            }
            else
            {
                connection.Execute(sql, parameters, transaction, commandTimeout, CommandType.Text);
            }
        }

        public dynamic Insert<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout, string tableName, string schemaName = null) where T : class
        {
            IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
            List<IPropertyMap> nonIdentityKeyProperties = classMap.Properties.Where(p => p.KeyType == KeyType.Guid || p.KeyType == KeyType.Assigned).ToList();
            var identityColumn = classMap.Properties.SingleOrDefault(p => p.KeyType == KeyType.Identity);
            var triggerIdentityColumn = classMap.Properties.SingleOrDefault(p => p.KeyType == KeyType.TriggerIdentity);
            foreach (var column in nonIdentityKeyProperties)
            {
                if (column.KeyType == KeyType.Guid && (Guid)column.PropertyInfo.GetValue(entity, null) == Guid.Empty)
                {
                    Guid comb = SqlGenerator.Configuration.GetNextGuid();
                    column.PropertyInfo.SetValue(entity, comb, null);
                }
            }

            IDictionary<string, object> keyValues = new ExpandoObject();
            string sql = SqlGenerator.Insert(classMap,schemaName, tableName);
            if (identityColumn != null)
            {
                IEnumerable<long> result;
                if (SqlGenerator.SupportsMultipleStatements())
                {
                    sql += SqlGenerator.Configuration.Dialect.BatchSeperator + SqlGenerator.IdentitySql(classMap,schemaName, tableName);
                    result = connection.Query<long>(sql, entity, transaction, false, commandTimeout, CommandType.Text);
                }
                else
                {
                    connection.Execute(sql, entity, transaction, commandTimeout, CommandType.Text);
                    sql = SqlGenerator.IdentitySql(classMap,schemaName, tableName);
                    result = connection.Query<long>(sql, entity, transaction, false, commandTimeout, CommandType.Text);
                }

                // We are only interested in the first identity, but we are iterating over all resulting items (if any).
                // This makes sure that ADO.NET drivers (like MySql) won't actively terminate the query.
                bool hasResult = false;
                int identityInt = 0;
                foreach (var identityValue in result)
                {
                    if (hasResult)
                    {
                        continue;
                    }
                    identityInt = Convert.ToInt32(identityValue);
                    hasResult = true;
                }
                if (!hasResult)
                {
                    throw new InvalidOperationException("The source sequence is empty.");
                }

                keyValues.Add(identityColumn.Name, identityInt);
                identityColumn.PropertyInfo.SetValue(entity, identityInt, null);
            }
            else if (triggerIdentityColumn != null)
            {
                var dynamicParameters = new DynamicParameters();
                foreach (var prop in entity.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => p.Name != triggerIdentityColumn.PropertyInfo.Name))
                {
                    dynamicParameters.Add(prop.Name, prop.GetValue(entity, null));
                }

                // defaultValue need for identify type of parameter
                var defaultValue = entity.GetType().GetProperty(triggerIdentityColumn.PropertyInfo.Name).GetValue(entity, null);
                dynamicParameters.Add("IdOutParam", direction: ParameterDirection.Output, value: defaultValue);

                connection.Execute(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text);

                var value = dynamicParameters.Get<object>(SqlGenerator.Configuration.Dialect.ParameterPrefix + "IdOutParam");
                keyValues.Add(triggerIdentityColumn.Name, value);
                triggerIdentityColumn.PropertyInfo.SetValue(entity, value, null);
            }
            else
            {
                connection.Execute(sql, entity, transaction, commandTimeout, CommandType.Text);
            }

            foreach (var column in nonIdentityKeyProperties)
            {
                keyValues.Add(column.Name, column.PropertyInfo.GetValue(entity, null));
            }

            if (keyValues.Count == 1)
            {
                return keyValues.First().Value;
            }

            return keyValues;
        }


        #endregion

        #region Update

        public bool Update<T>(IDbConnection connection, T entity, object predicate, IDbTransaction transaction, int? commandTimeout, string tableName, string schemaName = null, bool ignoreAllKeyProperties = false) where T : class
        {
            IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
            IPredicate wherePredicate = predicate == null ? GetKeyPredicate<T>(classMap, entity) : GetPredicate(classMap, predicate);
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string sql = SqlGenerator.Update(classMap, wherePredicate, parameters, schemaName, tableName);
            DynamicParameters dynamicParameters = new DynamicParameters();

            var columns = ignoreAllKeyProperties
                ? classMap.Properties.Where(p => !(p.Ignored || p.IsReadOnly) && p.KeyType == KeyType.NotAKey)
                : classMap.Properties.Where(p => !(p.Ignored || p.IsReadOnly || p.KeyType == KeyType.Identity || p.KeyType == KeyType.Assigned));

            foreach (var property in ReflectionHelper.GetObjectValues(entity).Where(property => columns.Any(c => c.Name == property.Key)))
            {
                dynamicParameters.Add(property.Key, property.Value);
            }

            foreach (var parameter in parameters)
            {
                dynamicParameters.Add(parameter.Key, parameter.Value);
            }

            return connection.Execute(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text) > 0;
        }
        public async Task<bool> UpdateAsync<T>(IDbConnection connection, T entity, object predicate, IDbTransaction transaction, int? commandTimeout, string tableName, string schemaName, bool ignoreAllKeyProperties) where T : class
        {
            IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
            IPredicate wherePredicate = predicate == null ? GetKeyPredicate<T>(classMap, entity) : GetPredicate(classMap, predicate);
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string sql = SqlGenerator.Update(classMap, wherePredicate, parameters, schemaName, tableName);
            DynamicParameters dynamicParameters = new DynamicParameters();

            var columns = ignoreAllKeyProperties
                ? classMap.Properties.Where(p => !(p.Ignored || p.IsReadOnly) && p.KeyType == KeyType.NotAKey)
                : classMap.Properties.Where(p => !(p.Ignored || p.IsReadOnly || p.KeyType == KeyType.Identity || p.KeyType == KeyType.Assigned));

            foreach (var property in ReflectionHelper.GetObjectValues(entity).Where(property => columns.Any(c => c.Name == property.Key)))
            {
                dynamicParameters.Add(property.Key, property.Value);
            }

            foreach (var parameter in parameters)
            {
                dynamicParameters.Add(parameter.Key, parameter.Value);
            }

            return await connection.ExecuteAsync(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text) > 0;
        }

        public bool UpdateSet<T>(IDbConnection connection, object entity, object predicate, IDbTransaction transaction, int? commandTimeout, string tableName, string schemaName = null) where T : class
        {
            IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
            IPredicate wherePredicate = predicate == null ? GetSetKeyPredicate<T>(classMap, entity) : GetPredicate(classMap, predicate);
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string sql = SqlGenerator.UpdateSet(classMap, entity, wherePredicate, parameters, schemaName, tableName);
            DynamicParameters dynamicParameters = new DynamicParameters();

            var columns = classMap.Properties.Where(p => !(p.Ignored || p.IsReadOnly || p.KeyType == KeyType.Identity || p.KeyType == KeyType.Assigned));

            foreach (var property in ReflectionHelper.GetObjectValues(entity).Where(property => columns.Any(c => c.Name.Equals(property.Key))))
            {
                dynamicParameters.Add(property.Key, property.Value);
            }

            foreach (var parameter in parameters)
            {
                dynamicParameters.Add(parameter.Key, parameter.Value);
            }

            return connection.Execute(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text) > 0;
        }
        public async Task<bool> UpdateSetAsync<T>(IDbConnection connection, object entity, object predicate, IDbTransaction transaction, int? commandTimeout, string tableName, string schemaName) where T : class
        {
            IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
            IPredicate wherePredicate = predicate == null ? GetSetKeyPredicate<T>(classMap, entity) : GetPredicate(classMap, predicate);
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string sql = SqlGenerator.UpdateSet(classMap, entity, wherePredicate, parameters, schemaName, tableName);
            DynamicParameters dynamicParameters = new DynamicParameters();

            var columns = classMap.Properties.Where(p => !(p.Ignored || p.IsReadOnly || p.KeyType == KeyType.Identity || p.KeyType == KeyType.Assigned));

            foreach (var property in ReflectionHelper.GetObjectValues(entity).Where(property => columns.Any(c => c.Name.Equals(property.Key))))
            {
                dynamicParameters.Add(property.Key, property.Value);
            }

            foreach (var parameter in parameters)
            {
                dynamicParameters.Add(parameter.Key, parameter.Value);
            }

            return await connection.ExecuteAsync(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text) > 0;
        }
        #endregion

        #region Delete
        public bool Delete<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout, string tableName, string schemaName = null) where T : class
        {
            var build = BuildDelete<T>(entity, null, tableName, schemaName);
            return connection.Execute(build.sql, build.dynamicParameters, transaction, commandTimeout, CommandType.Text) > 0;
        }
        public bool Delete<T>(IDbConnection connection, object predicate, IDbTransaction transaction, int? commandTimeout, string tableName, string schemaName) where T : class
        {
            var build = BuildDelete<T>(null, predicate, tableName, schemaName);
            return connection.Execute(build.sql, build.dynamicParameters, transaction, commandTimeout, CommandType.Text) > 0;
        }

        public async Task<bool> DeleteAsync<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout, string tableName, string schemaName) where T : class
        {
            var build = BuildDelete<T>(entity, null, tableName, schemaName);
            return await connection.ExecuteAsync(build.sql, build.dynamicParameters, transaction, commandTimeout, CommandType.Text) > 0;
        }

        
        public async Task<bool> DeleteAsync<T>(IDbConnection connection, object predicate, IDbTransaction transaction, int? commandTimeout, string tableName, string schemaName) where T : class
        {
            var build = BuildDelete<T>(null, predicate, tableName, schemaName);
            return await connection.ExecuteAsync(build.sql, build.dynamicParameters, transaction, commandTimeout, CommandType.Text) > 0;
        }


        protected (string sql, DynamicParameters dynamicParameters) BuildDelete<T>(T entity, object predicate, string tableName, string schemaName) where T : class
        {
            IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
            IPredicate wherePredicate = entity == null && predicate != null ? GetPredicate(classMap, predicate) : GetKeyPredicate<T>(classMap, entity);

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string sql = SqlGenerator.Delete(classMap, wherePredicate, parameters, schemaName, tableName);
            DynamicParameters dynamicParameters = new DynamicParameters();
            foreach (var parameter in parameters)
            {
                dynamicParameters.Add(parameter.Key, parameter.Value);
            }
            return (sql, dynamicParameters);
        }


        #endregion

        #region GetList
        public IEnumerable<T> GetList<T>(IDbConnection connection, object predicate, IList<ISort> sort, IDbTransaction transaction, int? commandTimeout, bool buffered, string tableName, string schemaName, IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias) where T : class
        {
            var build = BuildList<T>(predicate, sort, tableName, schemaName, join, alias);
            return connection.Query<T>(build.sql, build.dynamicParameters, transaction, buffered, commandTimeout, CommandType.Text);
        }

        public async Task<IEnumerable<T>> GetListAsync<T>(IDbConnection connection, object predicate, IList<ISort> sort, IDbTransaction transaction, int? commandTimeout, string tableName, string schemaName, IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias) where T : class
        {
            var build = BuildList<T>(predicate, sort, tableName, schemaName, join, alias);
            return await connection.QueryAsync<T>(build.sql, build.dynamicParameters, transaction, commandTimeout, CommandType.Text);
        }

        protected (string sql, DynamicParameters dynamicParameters) BuildList<T>(object predicate, IList<ISort> sort, string tableName, string schemaName, IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias) where T : class
        {
            VerifyJoinPredicate(join, predicate);

            IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            IPredicate wherePredicate = GetPredicate(classMap, predicate);
            string sql = SqlGenerator.Select(classMap, wherePredicate, sort, parameters, schemaName, tableName, join, alias);


            DynamicParameters dynamicParameters = new DynamicParameters();
            foreach (var parameter in parameters)
            {
                dynamicParameters.Add(parameter.Key, parameter.Value);
            }
            return (sql, dynamicParameters);
        }


        #endregion

        #region GetPage
        public IEnumerable<T> GetPage<T>(IDbConnection connection, object predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction, int? commandTimeout, bool buffered, string tableName, string schemaName, IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias) where T : class
        {
            var build = BuildPage<T>(predicate, sort, page, resultsPerPage, tableName, schemaName, join, alias);
            return connection.Query<T>(build.sql, build.dynamicParameters, transaction, buffered, commandTimeout, CommandType.Text);
        }

        public async Task<IEnumerable<T>> GetPageAsync<T>(IDbConnection connection, object predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction, int? commandTimeout, string tableName, string schemaName, IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias) where T : class
        {
            var build = BuildPage<T>(predicate, sort, page, resultsPerPage, tableName, schemaName, join, alias);
            return await connection.QueryAsync<T>(build.sql, build.dynamicParameters, transaction, commandTimeout, CommandType.Text);
        }

        protected (string sql, DynamicParameters dynamicParameters) BuildPage<T>(object predicate, IList<ISort> sort, int page, int resultsPerPage, string tableName, string schemaName, IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias) where T : class
        {
            VerifyJoinPredicate(join, predicate);

            IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
            IPredicate wherePredicate = GetPredicate(classMap, predicate);
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            string sql = SqlGenerator.SelectPaged(classMap, wherePredicate, sort, page, resultsPerPage, parameters, schemaName, tableName, join, alias);
            DynamicParameters dynamicParameters = new DynamicParameters();
            foreach (var parameter in parameters)
            {
                dynamicParameters.Add(parameter.Key, parameter.Value);
            }
            return (sql, dynamicParameters);
        }


        #endregion

        #region GetPages

        public Page<T> GetPages<T>(IDbConnection connection, object predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction, int? commandTimeout, string tableName, string schemaName, IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias) where T : class
        {
            var PageResult = new Page<T>() { CurrentPage = page, ItemsPerPage = resultsPerPage };
            PageResult.TotalItems = Count<T>(connection, predicate, transaction, commandTimeout, tableName, schemaName, join);
            if (PageResult.TotalItems == 0)
            {
                PageResult.Items = new List<T>();
                return PageResult;
            }
            var build = BuildPage<T>(predicate, sort, page, resultsPerPage, tableName, schemaName, join, alias);
            PageResult.Items = connection.Query<T>(build.sql, build.dynamicParameters, transaction, false, commandTimeout, CommandType.Text);
            return PageResult;
        }
        public async Task<Page<T>> GetPagesAsync<T>(IDbConnection connection, object predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction, int? commandTimeout, string tableName, string schemaName, IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias) where T : class
        {
            var PageResult = new Page<T>() { CurrentPage = page, ItemsPerPage = resultsPerPage };
            PageResult.TotalItems = await CountAsync<T>(connection, predicate, transaction, commandTimeout, tableName, schemaName, join);
            if (PageResult.TotalItems == 0)
            {
                PageResult.Items = new List<T>();
                return PageResult;
            }
            var build = BuildPage<T>(predicate, sort, page, resultsPerPage, tableName, schemaName, join, alias);
            PageResult.Items = await connection.QueryAsync<T>(build.sql, build.dynamicParameters, transaction, commandTimeout, CommandType.Text);
            return PageResult;
        }
        #endregion

        #region GetSet
        public IEnumerable<T> GetSet<T>(IDbConnection connection, object predicate, IList<ISort> sort, int firstResult, int maxResults, IDbTransaction transaction, int? commandTimeout, bool buffered, string tableName, string schemaName, IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias) where T : class
        {
            var build = BuildPage<T>(predicate, sort, firstResult, maxResults, tableName, schemaName, join, alias);
            return connection.Query<T>(build.sql, build.dynamicParameters, transaction, buffered, commandTimeout, CommandType.Text);
        }

        public async Task<IEnumerable<T>> GetSetAsync<T>(IDbConnection connection, object predicate, IList<ISort> sort, int firstResult, int maxResults, IDbTransaction transaction, int? commandTimeout, string tableName, string schemaName, IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias) where T : class
        {
            var build = BuildPage<T>(predicate, sort, firstResult, maxResults, tableName, schemaName, join, alias);
            return await connection.QueryAsync<T>(build.sql, build.dynamicParameters, transaction, commandTimeout, CommandType.Text);
        }

        protected (string sql, DynamicParameters dynamicParameters) BuildGetSet<T>(object predicate, IList<ISort> sort, int firstResult, int maxResults, string tableName, string schemaName, IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias) where T : class
        {
            VerifyJoinPredicate(join, predicate);

            IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
            IPredicate wherePredicate = GetPredicate(classMap, predicate);
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string sql = SqlGenerator.SelectSet(classMap, wherePredicate, sort, firstResult, maxResults, parameters, schemaName, tableName, join, alias);
            DynamicParameters dynamicParameters = new DynamicParameters();
            foreach (var parameter in parameters)
            {
                dynamicParameters.Add(parameter.Key, parameter.Value);
            }
            return (sql, dynamicParameters);
        }

        #endregion

        #region Count
        public long Count<T>(IDbConnection connection, object predicate, IDbTransaction transaction, int? commandTimeout, string tableName, string schemaName, IList<IJoinPredicate> join) where T : class
        {
            var build = BuildCount<T>(predicate, tableName, schemaName, join);
            return (long)(connection.Query(build.sql, build.dynamicParameters, transaction, false, commandTimeout, CommandType.Text).Single().Total);
        }

        public async Task<long> CountAsync<T>(IDbConnection connection, object predicate, IDbTransaction transaction, int? commandTimeout, string tableName, string schemaName, IList<IJoinPredicate> join) where T : class
        {
            var build = BuildCount<T>(predicate, tableName, schemaName, join);
            return (int)(await connection.QueryAsync(build.sql, build.dynamicParameters, transaction, commandTimeout, CommandType.Text)).Single().Total;
        }

        protected (string sql, DynamicParameters dynamicParameters) BuildCount<T>(object predicate, string tableName, string schemaName, IList<IJoinPredicate> join) where T : class
        {
            VerifyJoinPredicate(join, predicate);

            IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
            IPredicate wherePredicate = GetPredicate(classMap, predicate);
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string sql = SqlGenerator.Count(classMap, wherePredicate, parameters, schemaName, tableName, join);
            DynamicParameters dynamicParameters = new DynamicParameters();
            foreach (var parameter in parameters)
            {
                dynamicParameters.Add(parameter.Key, parameter.Value);
            }
            return (sql, dynamicParameters);
        }

        #endregion

        #region GetMultiple

        public IMultipleResultReader GetMultiple(IDbConnection connection, GetMultiplePredicate predicate, IDbTransaction transaction, int? commandTimeout, string tableName, string schemaName)
        {
            if (SqlGenerator.SupportsMultipleStatements())
            {
                return GetMultipleByBatch(connection, predicate, transaction, commandTimeout, tableName, schemaName);
            }

            return GetMultipleBySequence(connection, predicate, transaction, commandTimeout, tableName, schemaName);
        }
        public async Task<IMultipleResultReader> GetMultipleAsync(IDbConnection connection, GetMultiplePredicate predicate, IDbTransaction transaction, int? commandTimeout, string tableName, string schemaName)
        {
            if (SqlGenerator.SupportsMultipleStatements())
            {
                return await GetMultipleByBatchAsync(connection, predicate, transaction, commandTimeout, tableName, schemaName);
            }

            return await GetMultipleBySequenceAsync(connection, predicate, transaction, commandTimeout, tableName, schemaName);
        }

        #endregion

        #region Helpers

        protected IPredicate GetPredicate(IClassMapper classMap, object predicate)
        {
            IPredicate wherePredicate = predicate as IPredicate;
            if (wherePredicate == null && predicate != null)
            {
                wherePredicate = GetEntityPredicate(classMap, predicate);
            }

            return wherePredicate;
        }

        protected IPredicate GetIdPredicate(IClassMapper classMap, object id)
        {
            bool isSimpleType = ReflectionHelper.IsSimpleType(id.GetType());
            var keys = classMap.Properties.Where(p => p.KeyType != KeyType.NotAKey);
            IDictionary<string, object> paramValues = null;
            IList<IPredicate> predicates = new List<IPredicate>();
            if (!isSimpleType)
            {
                paramValues = ReflectionHelper.GetObjectValues(id);
            }

            foreach (var key in keys)
            {
                object value = id;
                if (!isSimpleType)
                {
                    value = paramValues[key.Name];
                }

                Type predicateType = typeof(FieldPredicate<>).MakeGenericType(classMap.EntityType);

                IFieldPredicate fieldPredicate = Activator.CreateInstance(predicateType) as IFieldPredicate;
                fieldPredicate.Not = false;
                fieldPredicate.Operator = Operator.Eq;
                fieldPredicate.PropertyName = key.Name;
                fieldPredicate.Value = value;
                predicates.Add(fieldPredicate);
            }

            return predicates.Count == 1
                       ? predicates[0]
                       : new PredicateGroup
                       {
                           Operator = GroupOperator.And,
                           Predicates = predicates
                       };
        }

        protected IPredicate GetKeyPredicate<T>(IClassMapper classMap, T entity) where T : class
        {
            var whereFields = classMap.Properties.Where(p => p.KeyType != KeyType.NotAKey);
            if (!whereFields.Any())
            {
                throw new ArgumentException("At least one Key column must be defined.");
            }

            IList<IPredicate> predicates = (from field in whereFields
                                            select new FieldPredicate<T>
                                            {
                                                Not = false,
                                                Operator = Operator.Eq,
                                                PropertyName = field.Name,
                                                Value = field.PropertyInfo.GetValue(entity, null)
                                            }).Cast<IPredicate>().ToList();

            return predicates.Count == 1
                       ? predicates[0]
                       : new PredicateGroup
                       {
                           Operator = GroupOperator.And,
                           Predicates = predicates
                       };
        }

        protected IPredicate GetSetKeyPredicate<T>(IClassMapper classMap, object entity) where T : class
        {
            var whereFields = classMap.Properties.Where(p => p.KeyType != KeyType.NotAKey);
            if (!whereFields.Any())
            {
                throw new ArgumentException("At least one Key column must be defined.");
            }
            var vKeyValue = ReflectionHelper.GetObjectValues(entity);
            IList<IPredicate> predicates = (from field in whereFields
                                            select new FieldPredicate<T>
                                            {
                                                Not = false,
                                                Operator = Operator.Eq,
                                                PropertyName = field.Name,
                                                Value = vKeyValue.Where(w => w.Key.Equals(field.Name, StringComparison.OrdinalIgnoreCase)).First().Value
                                            }).Cast<IPredicate>().ToList();

            return predicates.Count == 1
                       ? predicates[0]
                       : new PredicateGroup
                       {
                           Operator = GroupOperator.And,
                           Predicates = predicates
                       };
        }

        protected IPredicate GetEntityPredicate(IClassMapper classMap, object entity)
        {
            Type predicateType = typeof(FieldPredicate<>).MakeGenericType(classMap.EntityType);
            IList<IPredicate> predicates = new List<IPredicate>();
            foreach (var kvp in ReflectionHelper.GetObjectValues(entity))
            {
                IFieldPredicate fieldPredicate = Activator.CreateInstance(predicateType) as IFieldPredicate;
                fieldPredicate.Not = false;
                fieldPredicate.Operator = Operator.Eq;
                fieldPredicate.PropertyName = kvp.Key;
                fieldPredicate.Value = kvp.Value;
                predicates.Add(fieldPredicate);
            }

            return predicates.Count == 1
                       ? predicates[0]
                       : new PredicateGroup
                       {
                           Operator = GroupOperator.And,
                           Predicates = predicates
                       };
        }

        protected GridReaderResultReader GetMultipleByBatch(IDbConnection connection, GetMultiplePredicate predicate, IDbTransaction transaction, int? commandTimeout, string tableName, string schemaName)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            StringBuilder sql = new StringBuilder();
            foreach (var item in predicate.Items)
            {
                IClassMapper classMap = SqlGenerator.Configuration.GetMap(item.Type);
                IPredicate itemPredicate = item.Value as IPredicate;
                if (itemPredicate == null && item.Value != null)
                {
                    itemPredicate = GetPredicate(classMap, item.Value);
                }

                sql.AppendLine(SqlGenerator.Select(classMap, itemPredicate, item.Sort, parameters,schemaName, tableName) + SqlGenerator.Configuration.Dialect.BatchSeperator);
            }

            DynamicParameters dynamicParameters = new DynamicParameters();
            foreach (var parameter in parameters)
            {
                dynamicParameters.Add(parameter.Key, parameter.Value);
            }

            SqlMapper.GridReader grid = connection.QueryMultiple(sql.ToString(), dynamicParameters, transaction, commandTimeout, CommandType.Text);
            return new GridReaderResultReader(grid);
        }

        protected SequenceReaderResultReader GetMultipleBySequence(IDbConnection connection, GetMultiplePredicate predicate, IDbTransaction transaction, int? commandTimeout, string tableName, string schemaName)
        {
            IList<SqlMapper.GridReader> items = new List<SqlMapper.GridReader>();
            foreach (var item in predicate.Items)
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                IClassMapper classMap = SqlGenerator.Configuration.GetMap(item.Type);
                IPredicate itemPredicate = item.Value as IPredicate;
                if (itemPredicate == null && item.Value != null)
                {
                    itemPredicate = GetPredicate(classMap, item.Value);
                }

                string sql = SqlGenerator.Select(classMap, itemPredicate, item.Sort, parameters,schemaName, tableName);
                DynamicParameters dynamicParameters = new DynamicParameters();
                foreach (var parameter in parameters)
                {
                    dynamicParameters.Add(parameter.Key, parameter.Value);
                }

                SqlMapper.GridReader queryResult = connection.QueryMultiple(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text);
                items.Add(queryResult);
            }

            return new SequenceReaderResultReader(items);
        }


        protected async Task<GridReaderResultReader> GetMultipleByBatchAsync(IDbConnection connection, GetMultiplePredicate predicate, IDbTransaction transaction, int? commandTimeout, string tableName, string schemaName)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            StringBuilder sql = new StringBuilder();
            foreach (var item in predicate.Items)
            {
                IClassMapper classMap = SqlGenerator.Configuration.GetMap(item.Type);
                IPredicate itemPredicate = item.Value as IPredicate;
                if (itemPredicate == null && item.Value != null)
                {
                    itemPredicate = GetPredicate(classMap, item.Value);
                }

                sql.AppendLine(SqlGenerator.Select(classMap, itemPredicate, item.Sort, parameters, schemaName, tableName) + SqlGenerator.Configuration.Dialect.BatchSeperator);
            }

            DynamicParameters dynamicParameters = new DynamicParameters();
            foreach (var parameter in parameters)
            {
                dynamicParameters.Add(parameter.Key, parameter.Value);
            }

            SqlMapper.GridReader grid = await connection.QueryMultipleAsync(sql.ToString(), dynamicParameters, transaction, commandTimeout, CommandType.Text);
            return new GridReaderResultReader(grid);
        }

        protected async Task<SequenceReaderResultReader> GetMultipleBySequenceAsync(IDbConnection connection, GetMultiplePredicate predicate, IDbTransaction transaction, int? commandTimeout, string tableName, string schemaName)
        {
            IList<SqlMapper.GridReader> items = new List<SqlMapper.GridReader>();
            foreach (var item in predicate.Items)
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                IClassMapper classMap = SqlGenerator.Configuration.GetMap(item.Type);
                IPredicate itemPredicate = item.Value as IPredicate;
                if (itemPredicate == null && item.Value != null)
                {
                    itemPredicate = GetPredicate(classMap, item.Value);
                }

                string sql = SqlGenerator.Select(classMap, itemPredicate, item.Sort, parameters, schemaName, tableName);
                DynamicParameters dynamicParameters = new DynamicParameters();
                foreach (var parameter in parameters)
                {
                    dynamicParameters.Add(parameter.Key, parameter.Value);
                }

                SqlMapper.GridReader queryResult = await connection.QueryMultipleAsync(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text);
                items.Add(queryResult);
            }

            return new SequenceReaderResultReader(items);
        }

        /// <summary>
        /// 检测join模式下的条件参数类型
        /// </summary>
        /// <param name="join"></param>
        /// <param name="predicate"></param>
        protected void VerifyJoinPredicate(IList<IJoinPredicate> join, object predicate)
        {
            //联合查询时，参数必须是IPredicate格式，不能是anonymoustype、IEnumerable<KeyValuePair<TKey, TValue>>
            if (join != null && join.Count > 0 && predicate != null && (predicate as IPredicate) == null)
            {
                throw new Exception(" join predicate = IPredicate");
            }
        }
        #endregion
    }
}
