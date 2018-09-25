using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Repository.Dapper.Core.Mapper;

namespace Repository.Dapper.Core.Sql
{
    public interface ISqlGenerator
    {
        IDapperExtensionsConfiguration Configuration { get; }
        
        string Select(IClassMapper classMap, IPredicate predicate, IList<ISort> sort, IDictionary<string, object> parameters, string schemaName, string tableName, IList<IJoinPredicate> join=null, IList<IJoinAliasPredicate> alias = null);

        string SelectPaged(IClassMapper classMap, IPredicate predicate, IList<ISort> sort, int page, int resultsPerPage, IDictionary<string, object> parameters, string schemaName, string tableName, IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias);
        string SelectSet(IClassMapper classMap, IPredicate predicate, IList<ISort> sort, int firstResult, int maxResults, IDictionary<string, object> parameters, string schemaName, string tableName, IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias);
        string Count(IClassMapper classMap, IPredicate predicate, IDictionary<string, object> parameters, string schemaName, string tableName, IList<IJoinPredicate> join);

        string Insert(IClassMapper classMap, string schemaName, string tableName);
        string Update(IClassMapper classMap, IPredicate predicate, IDictionary<string, object> parameters, string schemaName, string tableName);
        string UpdateSet(IClassMapper classMap,object entity, IPredicate predicate, IDictionary<string, object> parameters, string schemaName, string tableName);
        string Delete(IClassMapper classMap, IPredicate predicate, IDictionary<string, object> parameters, string schemaName, string tableName);

        string IdentitySql(IClassMapper classMap, string schemaName, string tableName);
        string GetTableName(IClassMapper map, string schemaName, string tableName, IList<IJoinPredicate> join = null);
        string GetColumnName(bool prefix,IClassMapper map, IPropertyMap property, bool includeAlias, string schemaName, string tableName, string aliasName = null);
        string GetColumnName(bool prefix, IClassMapper map, string propertyName, bool includeAlias, string schemaName, string tableName);
        bool SupportsMultipleStatements();
        string GetOperatorString(Operator Operator, bool Not);
    }

    public class SqlGeneratorImpl : ISqlGenerator
    {
        public SqlGeneratorImpl(IDapperExtensionsConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IDapperExtensionsConfiguration Configuration { get; private set; }

        public virtual string Select(IClassMapper classMap, IPredicate predicate, IList<ISort> sort, IDictionary<string, object> parameters, string schemaName, string tableName, IList<IJoinPredicate> join=null, IList<IJoinAliasPredicate> alias=null)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException("Parameters");
            }

            StringBuilder sql = new StringBuilder(string.Format("SELECT {0} FROM {1}",
                BuildSelectColumns(classMap, schemaName, tableName,join,alias),
                GetTableName(classMap, schemaName, tableName,join)));

            if (predicate != null)
            {
                sql.Append(" WHERE ")
                    .Append(predicate.GetSql(join!=null,this, parameters,schemaName,tableName));
            }

            if (sort != null && sort.Any())
            {
                sql.Append(" ORDER BY ")
                    .Append(sort.Select(s => s.GetSql(join != null,this, schemaName,tableName)).AppendStrings());
            }

            return sql.ToString();
        }

        public virtual string SelectPaged(IClassMapper classMap, IPredicate predicate, IList<ISort> sort, int page, int resultsPerPage, IDictionary<string, object> parameters, string schemaName, string tableName, IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias)
        {
            if (sort == null || !sort.Any())
            {
                sort = classMap.Properties.Where(w => w.KeyType != KeyType.NotAKey).Select(w => (ISort)new SortKey(classMap) { PropertyName = w.Name, Ascending = true }).ToList();
                //throw new ArgumentNullException("Sort", "Sort cannot be null or empty.");
            }

            if (parameters == null)
            {
                throw new ArgumentNullException("Parameters");
            }

            StringBuilder innerSql = new StringBuilder(string.Format("SELECT {0} FROM {1}",
                BuildSelectColumns(classMap, schemaName, tableName,join,alias),
                GetTableName(classMap, schemaName, tableName,join)));
            if (predicate != null)
            {
                innerSql.Append(" WHERE ")
                    .Append(predicate.GetSql(join!=null,this, parameters,schemaName,tableName));
            }

            string orderBy = sort.Select(s => s.GetSql(join != null, this, schemaName, tableName)).AppendStrings();
            innerSql.Append(" ORDER BY " + orderBy);

            string sql = Configuration.Dialect.GetPagingSql(innerSql.ToString(), page, resultsPerPage, parameters);
            return sql;
        }

        public virtual string SelectSet(IClassMapper classMap, IPredicate predicate, IList<ISort> sort, int firstResult, int maxResults, IDictionary<string, object> parameters, string schemaName, string tableName, IList<IJoinPredicate> join, IList<IJoinAliasPredicate> alias)
        {
            if (sort == null || !sort.Any())
            {
                sort = classMap.Properties.Where(w => w.KeyType != KeyType.NotAKey).Select(w => (ISort)new SortKey(classMap) { PropertyName = w.Name, Ascending = true }).ToList();
                //throw new ArgumentNullException("Sort", "Sort cannot be null or empty.");
            }

            if (parameters == null)
            {
                throw new ArgumentNullException("Parameters");
            }

            StringBuilder innerSql = new StringBuilder(string.Format("SELECT {0} FROM {1}",
                BuildSelectColumns(classMap, schemaName, tableName,join,alias),
                GetTableName(classMap, schemaName, tableName,join)));
            if (predicate != null)
            {
                innerSql.Append(" WHERE ")
                    .Append(predicate.GetSql(join != null, this, parameters,schemaName,tableName));
            }

            string orderBy = sort.Select(s => s.GetSql(join != null, this, schemaName, tableName)).AppendStrings();
            innerSql.Append(" ORDER BY " + orderBy);

            string sql = Configuration.Dialect.GetSetSql(innerSql.ToString(), firstResult, maxResults, parameters);
            return sql;
        }


        public virtual string Count(IClassMapper classMap, IPredicate predicate, IDictionary<string, object> parameters, string schemaName, string tableName, IList<IJoinPredicate> join)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException("Parameters");
            }

            StringBuilder sql = new StringBuilder(string.Format("SELECT COUNT(*) AS {0}Total{1} FROM {2}",
                                Configuration.Dialect.OpenQuote,
                                Configuration.Dialect.CloseQuote,
                                GetTableName(classMap, schemaName, tableName,join)));
            if (predicate != null)
            {
                sql.Append(" WHERE ")
                    .Append(predicate.GetSql(join != null, this, parameters,schemaName,tableName));
            }

            return sql.ToString();
        }
        
        public virtual string Insert(IClassMapper classMap, string schemaName, string tableName)
        {
            var columns = classMap.Properties.Where(p => !(p.Ignored || p.IsReadOnly || p.KeyType == KeyType.Identity || p.KeyType == KeyType.TriggerIdentity));
            if (!columns.Any())
            {
                throw new ArgumentException("No columns were mapped.");
            }

            var columnNames = columns.Select(p => GetColumnName(false,classMap, p, false, schemaName, tableName));
            var parameters = columns.Select(p => Configuration.Dialect.ParameterPrefix + p.Name);

            string sql = string.Format("INSERT INTO {0} ({1}) VALUES ({2})",
                                       GetTableName(classMap, schemaName, tableName),
                                       columnNames.AppendStrings(),
                                       parameters.AppendStrings());

            var triggerIdentityColumn = classMap.Properties.Where(p => p.KeyType == KeyType.TriggerIdentity).ToList();

            if (triggerIdentityColumn.Count > 0)
            {
                if (triggerIdentityColumn.Count > 1)
                    throw new ArgumentException("TriggerIdentity generator cannot be used with multi-column keys");

                sql += string.Format(" RETURNING {0} INTO {1}IdOutParam", triggerIdentityColumn.Select(p => GetColumnName(false,classMap, p, false, schemaName, tableName)).First(), Configuration.Dialect.ParameterPrefix);
            }

            return sql;
        }

        public virtual string Update(IClassMapper classMap, IPredicate predicate, IDictionary<string, object> parameters, string schemaName, string tableName)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException("Predicate");
            }

            if (parameters == null)
            {
                throw new ArgumentNullException("Parameters");
            }

            var columns = classMap.Properties.Where(p => !(p.Ignored || p.IsReadOnly || p.KeyType == KeyType.Identity || p.KeyType == KeyType.Assigned));
            if (!columns.Any())
            {
                throw new ArgumentException("No columns were mapped.");
            }

            var setSql =
                columns.Select(
                    p =>
                    string.Format(
                        "{0} = {1}{2}", GetColumnName(false,classMap, p, false, schemaName, tableName), Configuration.Dialect.ParameterPrefix, p.Name));

            return string.Format("UPDATE {0} SET {1} WHERE {2}",
                GetTableName(classMap, schemaName, tableName),
                setSql.AppendStrings(),
                predicate.GetSql(false,this, parameters,schemaName,tableName));
        }

        public virtual string UpdateSet(IClassMapper classMap, object entity, IPredicate predicate, IDictionary<string, object> parameters, string schemaName, string tableName)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException("Predicate");
            }

            if (parameters == null)
            {
                throw new ArgumentNullException("Parameters");
            }

            var columns = classMap.Properties.Where(p => !(p.Ignored || p.IsReadOnly || p.KeyType == KeyType.Identity || p.KeyType == KeyType.Assigned));
            if (!columns.Any())
            {
                throw new ArgumentException("No columns were mapped.");
            }
            var vColName = ReflectionHelper.GetObjectValues(entity).Select(w => w.Key);
            var setSql =
                columns.Where(w => vColName.Contains(w.Name)).Select(
                    p =>
                    string.Format(
                        "{0} = {1}{2}", GetColumnName(false,classMap, p, false, schemaName, tableName), Configuration.Dialect.ParameterPrefix, p.Name));

            return string.Format("UPDATE {0} SET {1} WHERE {2}",
                GetTableName(classMap, schemaName, tableName),
                setSql.AppendStrings(),
                predicate.GetSql(false,this, parameters, schemaName, tableName));
        }

        public virtual string Delete(IClassMapper classMap, IPredicate predicate, IDictionary<string, object> parameters, string schemaName, string tableName)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException("Predicate");
            }

            if (parameters == null)
            {
                throw new ArgumentNullException("Parameters");
            }

            StringBuilder sql = new StringBuilder(string.Format("DELETE FROM {0}", GetTableName(classMap, schemaName, tableName)));
            sql.Append(" WHERE ").Append(predicate.GetSql(false,this, parameters,schemaName,tableName));
            return sql.ToString();
        }
        
        public virtual string IdentitySql(IClassMapper classMap, string schemaName, string tableName)
        {
            return Configuration.Dialect.GetIdentitySql(GetTableName(classMap, schemaName, tableName));
        }

        public virtual string GetTableName(IClassMapper map, string schemaName, string tableName, IList<IJoinPredicate> join = null)
        {
            if (join != null && join.Count > 0)
            {
                var baseClassMap = join[0].GetLeftMapper(this);
                var baseTable = GetTableName(baseClassMap, join[0].SchemaName, join[0].TableName);
                return $"{baseTable} {string.Join(" ", join.Select(w => w.GetSql(join != null, this, null, null, null)))}";
            }

            schemaName = string.IsNullOrWhiteSpace(schemaName) ? map.SchemaName : schemaName;
            tableName = string.IsNullOrWhiteSpace(tableName) ? map.TableName : tableName;
            return Configuration.Dialect.GetTableName(schemaName, tableName, null);
        }

        public virtual string GetColumnName(bool prefix, IClassMapper map, IPropertyMap property, bool includeAlias, string schemaName, string tableName, string aliasName = null)
        {
            string alias = null;
            string propertyName = property.Name;
            if (aliasName != null)
            {
                propertyName = aliasName;
            }
            if (property.ColumnName != propertyName && includeAlias)
            {
                alias = propertyName;
            }

            return Configuration.Dialect.GetColumnName(prefix ? GetTableName(map, schemaName, tableName) : null, property.ColumnName, alias);
        }

        public virtual string GetColumnName(bool prefix, IClassMapper map, string propertyName, bool includeAlias, string schemaName, string tableName)
        {
            IPropertyMap propertyMap = map.Properties.SingleOrDefault(p => p.Name.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase));
            if (propertyMap == null)
            {
                throw new ArgumentException(string.Format("Could not find '{0}' in Mapping.", propertyName));
            }

            return GetColumnName(prefix,map, propertyMap, includeAlias,schemaName, tableName);
        }

        public virtual bool SupportsMultipleStatements()
        {
            return Configuration.Dialect.SupportsMultipleStatements;
        }

        public virtual string BuildSelectColumns(IClassMapper classMap, string schemaName, string tableName, IList<IJoinPredicate> join = null, IList<IJoinAliasPredicate> alias = null)
        {
            if (join != null && join.Count > 0)
            {
                Dictionary<string, JoinMapper> joinMap = new Dictionary<string, JoinMapper>();

                foreach (var itemJoin in join)
                {
                    var leftMap = itemJoin.GetLeftMapper(this);
                    var tablename = $"{leftMap.SchemaName}.{leftMap.TableName}";
                    if (!joinMap.ContainsKey(tablename))
                        joinMap.Add(tablename, new JoinMapper()
                        {
                            Mapper = leftMap,
                            SchemaName = itemJoin.SchemaName,
                            TableName = itemJoin.TableName
                        });
                    var rightMap = itemJoin.GetRightMapper(this);
                    tablename = $"{rightMap.SchemaName}.{rightMap.TableName}";
                    if (!joinMap.ContainsKey(tablename))
                        joinMap.Add(tablename, new JoinMapper()
                        {
                            Mapper = rightMap,
                            SchemaName = itemJoin.SchemaName2,
                            TableName = itemJoin.TableName2
                        });
                }

                var vMapPro = classMap.Properties.Select(w => w.Name).ToList();

                List<JoinMapper> ListColumn = new List<JoinMapper>();

                foreach (var itemMap in joinMap)
                {
                    if (vMapPro.Count == 0)
                        break;
                    var vAllProperty = itemMap.Value.Mapper.Properties.Where(w => !w.Ignored);
                    for (int i = vMapPro.Count - 1; i >= 0; i--)
                    {
                        var jitem = vAllProperty.Where(w => w.Name.Equals(vMapPro[i], StringComparison.OrdinalIgnoreCase));
                        if (jitem.Count() > 0)
                        {
                            ListColumn.Add(new JoinMapper()
                            {
                                AliasName = null,
                                Mapper = itemMap.Value.Mapper,
                                Propertry = jitem.First(),
                                SchemaName = itemMap.Value.SchemaName,
                                TableName = itemMap.Value.TableName
                            });
                            vMapPro.RemoveAt(i);
                        }
                    }
                }

                if (alias != null && vMapPro.Count > 0)
                {
                    foreach (var itemAlia in alias)
                    {
                        if (vMapPro.Count == 0)
                            break;

                        var rightMap = itemAlia.GetRightMapper(this);
                        var tablename = $"{rightMap.SchemaName}.{rightMap.TableName}";
                        if (!joinMap.ContainsKey(tablename))
                            break;

                        var vrightSchemaMap = joinMap[tablename];
                        var propertyMap = itemAlia.GetPropertyMap(this);
                        for (int i = vMapPro.Count - 1; i >= 0; i--)
                        {
                            if (itemAlia.PropertyName.Equals(vMapPro[i]))
                            {
                                ListColumn.Add(new JoinMapper()
                                {
                                    AliasName = itemAlia.PropertyName,
                                    Mapper = vrightSchemaMap.Mapper,
                                    Propertry = propertyMap,
                                    SchemaName = vrightSchemaMap.SchemaName,
                                    TableName = vrightSchemaMap.TableName
                                });
                                vMapPro.RemoveAt(i);
                                break;
                            }
                        }
                    }
                }

                var jcolumns = ListColumn.Select(p => GetColumnName(true,p.Mapper, p.Propertry, true, p.SchemaName, p.TableName, p.AliasName));
                return jcolumns.AppendStrings();
            }

            var columns = classMap.Properties
                .Where(p => !p.Ignored)
                .Select(p => GetColumnName(false,classMap, p, true, schemaName, tableName));
            return columns.AppendStrings();
        }

        public string GetOperatorString(Operator Operator, bool Not) => Configuration.Dialect.GetOperatorString(Operator, Not);

        internal class JoinMapper
        {
            public IPropertyMap Propertry { get; set; }
            public IClassMapper Mapper { get; set; }
            public string SchemaName { get; set; }
            public string TableName { get; set; }

            public string AliasName { get; set; }
        }
    }
}