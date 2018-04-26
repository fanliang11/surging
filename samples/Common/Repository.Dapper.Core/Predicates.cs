using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Repository.Dapper.Core.Mapper;
using Repository.Dapper.Core.Sql;

namespace Repository.Dapper.Core
{
    public static class Predicates
    {
        /// <summary>
        /// 创建条件
        /// Factory method that creates a new IFieldPredicate predicate: [FieldName] [Operator] [Value]. 
        /// Example: WHERE FirstName = 'Foo'
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="expression">An expression that returns the left operand [FieldName].</param>
        /// <param name="op">The comparison operator.</param>
        /// <param name="value">The value for the predicate.</param>
        /// <param name="not">Effectively inverts the comparison operator. Example: WHERE FirstName &lt;&gt; 'Foo'.</param>
        /// <returns>An instance of IFieldPredicate.</returns>
        public static IFieldPredicate Field<T>(Expression<Func<T, object>> expression, Operator op, object value, string tableName = null, string schemaName = null, bool not = false) where T : class
        {
            PropertyInfo propertyInfo = ReflectionHelper.GetProperty(expression) as PropertyInfo;
            return new FieldPredicate<T>
            {
                PropertyName = propertyInfo.Name,
                Operator = op,
                Value = value,
                Not = not,
                SchemaName = schemaName,
                TableName = tableName
            };
        }

        public static IFieldPredicate Field(Type type,string PropertyName, Operator op, object value, string tableName = null, string schemaName = null, bool not = false)
        {
            PropertyInfo propertyInfo = type.GetProperties().Where(w => w.Name == PropertyName).First();
            return new FieldPredicate
            {
                FieldType = type,
                PropertyName = propertyInfo.Name,
                Operator = op,
                Value = value,
                Not = not,
                SchemaName = schemaName,
                TableName = tableName
            };
        }

        /// <summary>
        /// Factory method that creates a new IPropertyPredicate predicate: [FieldName1] [Operator] [FieldName2]
        /// Example: WHERE FirstName = LastName
        /// </summary>
        /// <typeparam name="T">The type of the entity for the left operand.</typeparam>
        /// <typeparam name="T2">The type of the entity for the right operand.</typeparam>
        /// <param name="expression">An expression that returns the left operand [FieldName1].</param>
        /// <param name="op">The comparison operator.</param>
        /// <param name="expression2">An expression that returns the right operand [FieldName2].</param>
        /// <param name="not">Effectively inverts the comparison operator. Example: WHERE FirstName &lt;&gt; LastName </param>
        /// <returns>An instance of IPropertyPredicate.</returns>
        public static IPropertyPredicate Property<T, T2>(Expression<Func<T, object>> expression, Operator op, Expression<Func<T2, object>> expression2, string tableName = null, string tableName2 = null, string schemaName = null, string schemaName2 = null, bool not = false)
            where T : class
            where T2 : class
        {
            PropertyInfo propertyInfo = ReflectionHelper.GetProperty(expression) as PropertyInfo;
            PropertyInfo propertyInfo2 = ReflectionHelper.GetProperty(expression2) as PropertyInfo;
            return new PropertyPredicate<T, T2>
            {
                SchemaName = schemaName,
                TableName = tableName,
                SchemaName2 = schemaName2,
                TableName2 = tableName2,
                PropertyName = propertyInfo.Name,
                PropertyName2 = propertyInfo2.Name,
                Operator = op,
                Not = not
            };
        }

        /// <summary>
        /// 关联查询列表重复设置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="expression"></param>
        /// <param name="expression2"></param>
        /// <returns></returns>
        public static IJoinAliasPredicate JoinAlia<T, T2>(Expression<Func<T, object>> expression, Expression<Func<T2, object>> expression2)
            where T : class
            where T2 : class
        {
            PropertyInfo propertyInfo = ReflectionHelper.GetProperty(expression) as PropertyInfo;
            PropertyInfo propertyInfo2 = ReflectionHelper.GetProperty(expression2) as PropertyInfo;
            return new JoinAliasPredicate<T, T2>
            {
                PropertyName = propertyInfo.Name,
                PropertyName2 = propertyInfo2.Name
            };
        }

        /// <summary>
        /// 设置关联查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="expression"></param>
        /// <param name="op"></param>
        /// <param name="expression2"></param>
        /// <param name="join"></param>
        /// <param name="not"></param>
        /// <returns></returns>
        public static IJoinPredicate Join<T, T2>(Expression<Func<T, object>> expression, Operator op, Expression<Func<T2, object>> expression2, JoinOperator join = JoinOperator.Inner, string tableName = null, string tableName2 = null, string schemaName = null, string schemaName2 = null, bool not = false)
            where T : class
            where T2 : class
        {
            PropertyInfo propertyInfo = ReflectionHelper.GetProperty(expression) as PropertyInfo;
            PropertyInfo propertyInfo2 = ReflectionHelper.GetProperty(expression2) as PropertyInfo;
            return new JoinPredicate<T, T2>
            {
                SchemaName = schemaName,
                TableName = tableName,
                SchemaName2 = schemaName2,
                TableName2 = tableName2,
                Join = join,
                PropertyName = propertyInfo.Name,
                PropertyName2 = propertyInfo2.Name,
                Operator = op,
                Not = not
            };
        }

        /// <summary>
        /// Factory method that creates a new IPredicateGroup predicate.
        /// Predicate groups can be joined together with other predicate groups.
        /// </summary>
        /// <param name="op">The grouping operator to use when joining the predicates (AND / OR).</param>
        /// <param name="predicate">A list of predicates to group.</param>
        /// <returns>An instance of IPredicateGroup.</returns>
        public static IPredicateGroup Group(GroupOperator op, params IPredicate[] predicate)
        {
            return new PredicateGroup
            {
                Operator = op,
                Predicates = predicate
            };
        }

        /// <summary>
        /// Factory method that creates a new IExistsPredicate predicate.
        /// </summary>
        public static IExistsPredicate Exists<TSub>(IPredicate predicate, bool not = false)
            where TSub : class
        {
            return new ExistsPredicate<TSub>
            {
                Not = not,
                Predicate = predicate
            };
        }

        /// <summary>
        /// Factory method that creates a new IBetweenPredicate predicate. 
        /// </summary>
        public static IBetweenPredicate Between<T>(Expression<Func<T, object>> expression, BetweenValues values, string tableName = null, string schemaName = null, bool not = false)
            where T : class
        {
            PropertyInfo propertyInfo = ReflectionHelper.GetProperty(expression) as PropertyInfo;
            return new BetweenPredicate<T>
            {
                Not = not,
                PropertyName = propertyInfo.Name,
                Value = values,
                SchemaName = schemaName,
                TableName = tableName
            };
        }

        /// <summary>
        /// Factory method that creates a new Sort which controls how the results will be sorted.
        /// </summary>
        public static ISort Sort<T>(Expression<Func<T, object>> expression, bool ascending = true, string tableName = null, string schemaName = null)
        {
            PropertyInfo propertyInfo = ReflectionHelper.GetProperty(expression) as PropertyInfo;
            return new Sort<T>
            {
                PropertyName = propertyInfo.Name,
                Ascending = ascending,
                SchemaName = schemaName,
                TableName = tableName
            };
        }
    }

    /// <summary>
    /// 条件
    /// </summary>
    public interface IPredicate
    {
        /// <summary>
        /// 返回条件的类
        /// </summary>
        /// <returns></returns>
        Type GetPredicateType();

        /// <summary>
        /// 返回条件的sql
        /// </summary>
        /// <param name="sqlGenerator"></param>
        /// <param name="parameters"></param>
        /// <param name="schemaName"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        string GetSql(bool prefix, ISqlGenerator sqlGenerator, IDictionary<string, object> parameters, string schemaName, string tableName);
    }

    /// <summary>
    /// 基本条件
    /// </summary>
    public interface IBasePredicate : IPredicate
    {
        /// <summary>
        /// 前缀
        /// </summary>
        string SchemaName { get; set; }
        /// <summary>
        /// 表名
        /// </summary>
        string TableName { get; set; }

        /// <summary>
        /// 条件属性
        /// </summary>
        string PropertyName { get; set; }

    }

    /// <summary>
    /// 基本条件
    /// </summary>
    public abstract class BasePredicate : IBasePredicate
    {
        public abstract Type GetPredicateType();

        /// <summary>
        /// 返回sql
        /// </summary>
        /// <param name="sqlGenerator"></param>
        /// <param name="parameters"></param>
        /// <param name="schemaName"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public abstract string GetSql(bool prefix, ISqlGenerator sqlGenerator, IDictionary<string, object> parameters, string schemaName, string tableName);
        /// <summary>
        /// 条件属性
        /// </summary>
        public string PropertyName { get; set; }
        /// <summary>
        /// 前缀
        /// </summary>
        public string SchemaName { get; set; }
        /// <summary>
        /// 表名
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// 返回列名
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="sqlGenerator"></param>
        /// <param name="propertyName"></param>
        /// <param name="schemaName"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        protected virtual string GetColumnName(bool prefix, Type entityType, ISqlGenerator sqlGenerator, string propertyName, string schemaName, string tableName)
        {
            IClassMapper map = GetMapper(entityType, sqlGenerator);
            if (map == null)
            {
                throw new NullReferenceException(string.Format("Map was not found for {0}", entityType));
            }

            IPropertyMap propertyMap = map.Properties.SingleOrDefault(p => p.Name == propertyName);
            if (propertyMap == null)
            {
                throw new NullReferenceException(string.Format("{0} was not found for {1}", propertyName, entityType));
            }
            return sqlGenerator.GetColumnName(prefix,map, propertyMap, false, schemaName, tableName);
        }

        protected virtual IClassMapper GetMapper(Type entityType, ISqlGenerator sqlGenerator)
        {
            return sqlGenerator.Configuration.GetMap(entityType);
        }
    }

    /// <summary>
    /// 比较条件
    /// </summary>
    public interface IComparePredicate : IBasePredicate
    {
        /// <summary>
        /// 操作符
        /// </summary>
        Operator Operator { get; set; }
        /// <summary>
        /// 操作符取反
        /// </summary>
        bool Not { get; set; }
    }

    public abstract class ComparePredicate : BasePredicate
    {
        public Operator Operator { get; set; }
        public bool Not { get; set; }

        public virtual string GetOperatorString(ISqlGenerator sqlGenerator) => sqlGenerator.GetOperatorString(Operator, Not);
    }

    public interface IFieldPredicate : IComparePredicate
    {
        object Value { get; set; }
    }

    public class FieldPredicate<T> : ComparePredicate, IFieldPredicate
        where T : class
    {
        public override Type GetPredicateType()
        {
            return typeof(T);
        }
        public object Value { get; set; }

        public override string GetSql(bool prefix, ISqlGenerator sqlGenerator, IDictionary<string, object> parameters, string schemaName, string tableName)
        {
            string columnName = GetColumnName(prefix,typeof(T), sqlGenerator, PropertyName, SchemaName ?? schemaName, TableName ?? tableName);
            if (Value == null)
            {
                return string.Format("({0} IS {1}NULL)", columnName, Not ? "NOT " : string.Empty);
            }

            if (Value is IEnumerable && !(Value is string))
            {
                if (Operator != Operator.Eq)
                {
                    throw new ArgumentException("Operator must be set to Eq for Enumerable types");
                }

                List<string> @params = new List<string>();
                foreach (var value in (IEnumerable)Value)
                {
                    string valueParameterName = parameters.SetParameterName(this.PropertyName, value, sqlGenerator.Configuration.Dialect.ParameterPrefix);
                    @params.Add(valueParameterName);
                }

                string paramStrings = @params.Aggregate(new StringBuilder(), (sb, s) => sb.Append((sb.Length != 0 ? ", " : string.Empty) + s), sb => sb.ToString());
                return string.Format("({0} {1}IN ({2}))", columnName, Not ? "NOT " : string.Empty, paramStrings);
            }

            string parameterName = parameters.SetParameterName(this.PropertyName, this.Value, sqlGenerator.Configuration.Dialect.ParameterPrefix);
            return string.Format("({0} {1} {2})", columnName, GetOperatorString(sqlGenerator), parameterName);
        }
    }

    public class FieldPredicate : ComparePredicate, IFieldPredicate
    {
        public override Type GetPredicateType()
        {
            return FieldType;
        }
        public Type FieldType { get; set; }

        public object Value { get; set; }

        public override string GetSql(bool prefix, ISqlGenerator sqlGenerator, IDictionary<string, object> parameters, string schemaName, string tableName)
        {
            string columnName = GetColumnName(prefix,FieldType, sqlGenerator, PropertyName, SchemaName ?? schemaName, TableName ?? tableName);
            if (Value == null)
            {
                return string.Format("({0} IS {1}NULL)", columnName, Not ? "NOT " : string.Empty);
            }

            if (Value is IEnumerable && !(Value is string))
            {
                if (Operator != Operator.Eq)
                {
                    throw new ArgumentException("Operator must be set to Eq for Enumerable types");
                }

                List<string> @params = new List<string>();
                foreach (var value in (IEnumerable)Value)
                {
                    string valueParameterName = parameters.SetParameterName(this.PropertyName, value, sqlGenerator.Configuration.Dialect.ParameterPrefix);
                    @params.Add(valueParameterName);
                }

                string paramStrings = @params.Aggregate(new StringBuilder(), (sb, s) => sb.Append((sb.Length != 0 ? ", " : string.Empty) + s), sb => sb.ToString());
                return string.Format("({0} {1}IN ({2}))", columnName, Not ? "NOT " : string.Empty, paramStrings);
            }

            string parameterName = parameters.SetParameterName(this.PropertyName, this.Value, sqlGenerator.Configuration.Dialect.ParameterPrefix);
            return string.Format("({0} {1} {2})", columnName, GetOperatorString(sqlGenerator), parameterName);
        }
    }

    public interface IJoinPredicate : IComparePredicate
    {

        string SchemaName2 { get; set; }
        string TableName2 { get; set; }
        string PropertyName2 { get; set; }
        JoinOperator Join { get; set; }
        IClassMapper GetLeftMapper(ISqlGenerator sqlGenerator);
        IClassMapper GetRightMapper(ISqlGenerator sqlGenerator);
    }

    public class JoinPredicate<T, T2> : ComparePredicate, IJoinPredicate
        where T : class
        where T2 : class
    {
        public override Type GetPredicateType()
        {
            return typeof(T);
        }
        public string PropertyName2 { get; set; }

        public JoinOperator Join { get; set; }

        public string SchemaName2 { get; set; }
        public string TableName2 { get; set; }

        public IClassMapper GetLeftMapper(ISqlGenerator sqlGenerator)
        {
            return GetMapper(typeof(T), sqlGenerator);
        }

        public IClassMapper GetRightMapper(ISqlGenerator sqlGenerator)
        {
            return GetMapper(typeof(T2), sqlGenerator);
        }

        public override string GetSql(bool prefix, ISqlGenerator sqlGenerator, IDictionary<string, object> parameters, string schemaName, string tableName)
        {
            string righttable = sqlGenerator.GetTableName(GetRightMapper(sqlGenerator), SchemaName2, TableName2);
            string columnName = GetColumnName(true,typeof(T), sqlGenerator, PropertyName, SchemaName ?? schemaName, TableName ?? tableName);
            string columnName2 = GetColumnName(true,typeof(T2), sqlGenerator, PropertyName2, SchemaName2, TableName2);
            string vjoin = Join == JoinOperator.Full ? "full join" : Join == JoinOperator.Left ? "left join" : Join == JoinOperator.Right ? "right join" : "inner join";
            return $" {vjoin} {righttable} on {columnName} {GetOperatorString(sqlGenerator)} {columnName2}";
        }
    }

    public interface IJoinAliasPredicate : IComparePredicate
    {
        string PropertyName2 { get; set; }

        IClassMapper GetRightMapper(ISqlGenerator sqlGenerator);
        IPropertyMap GetPropertyMap(ISqlGenerator sqlGenerator);
    }
    public class JoinAliasPredicate<T, T2> : ComparePredicate, IJoinAliasPredicate
      where T : class
      where T2 : class
    {
        public override Type GetPredicateType()
        {
            return typeof(T);
        }
        public string PropertyName2 { get; set; }

        public IClassMapper GetRightMapper(ISqlGenerator sqlGenerator)
        {
            return GetMapper(typeof(T2), sqlGenerator);
        }

        public IPropertyMap GetPropertyMap(ISqlGenerator sqlGenerator)
        {
            var rightMap = GetRightMapper(sqlGenerator);
            var property = rightMap.Properties.Where(w => w.Name.Equals(PropertyName2)).First();
            return property;
        }

        public override string GetSql(bool prefix, ISqlGenerator sqlGenerator, IDictionary<string, object> parameters, string schemaName, string tableName)
        {
            return string.Empty;
        }
    }

    public interface IPropertyPredicate : IComparePredicate
    {
        string SchemaName2 { get; set; }
        string TableName2 { get; set; }
        string PropertyName2 { get; set; }
    }


    public class PropertyPredicate<T, T2> : ComparePredicate, IPropertyPredicate
        where T : class
        where T2 : class
    {
        public override Type GetPredicateType()
        {
            return typeof(T);
        }
        public string PropertyName2 { get; set; }
        public string SchemaName2 { get; set; }
        public string TableName2 { get; set; }

        public override string GetSql(bool prefix, ISqlGenerator sqlGenerator, IDictionary<string, object> parameters, string schemaName, string tableName)
        {
            string columnName = GetColumnName(prefix,typeof(T), sqlGenerator, PropertyName, SchemaName ?? schemaName, TableName ?? tableName);
            string columnName2 = GetColumnName(prefix, typeof(T2), sqlGenerator, PropertyName2, schemaName, tableName);
            return string.Format("({0} {1} {2})", columnName, GetOperatorString(sqlGenerator), columnName2);
        }
    }

    public struct BetweenValues
    {
        public object Value1 { get; set; }
        public object Value2 { get; set; }
    }

    public interface IBetweenPredicate : IPredicate
    {
        string PropertyName { get; set; }
        BetweenValues Value { get; set; }
        bool Not { get; set; }

    }

    public class BetweenPredicate<T> : BasePredicate, IBetweenPredicate
        where T : class
    {
        public override Type GetPredicateType()
        {
            return typeof(T);
        }
        public override string GetSql(bool prefix, ISqlGenerator sqlGenerator, IDictionary<string, object> parameters, string schemaName, string tableName)
        {
            string columnName = GetColumnName(prefix, typeof(T), sqlGenerator, PropertyName, SchemaName ?? schemaName, TableName ?? tableName);
            string propertyName1 = parameters.SetParameterName(this.PropertyName, this.Value.Value1, sqlGenerator.Configuration.Dialect.ParameterPrefix);
            string propertyName2 = parameters.SetParameterName(this.PropertyName, this.Value.Value2, sqlGenerator.Configuration.Dialect.ParameterPrefix);
            return string.Format("({0} {1}BETWEEN {2} AND {3})", columnName, Not ? "NOT " : string.Empty, propertyName1, propertyName2);
        }

        public BetweenValues Value { get; set; }

        public bool Not { get; set; }
    }

    /// <summary>
    /// Comparison operator for predicates.
    /// </summary>
    public enum Operator
    {
        /// <summary>
        /// Equal to =
        /// </summary>
        Eq,

        /// <summary>
        /// Greater than >
        /// </summary>
        Gt,

        /// <summary>
        /// Greater than or equal to >=
        /// </summary>
        Ge,

        /// <summary>
        /// Less than <
        /// </summary>
        Lt,

        /// <summary>
        /// Less than or equal to <=
        /// </summary>
        Le,

        /// <summary>
        /// Like (You can use % in the value to do wilcard searching) 
        /// </summary>
        Like
    }

    public interface IPredicateGroup : IPredicate
    {
        GroupOperator Operator { get; set; }
        IList<IPredicate> Predicates { get; set; }
    }

    /// <summary>
    /// Groups IPredicates together using the specified group operator.
    /// </summary>
    public class PredicateGroup : IPredicateGroup
    {
        public Type GetPredicateType()
        {
            return null;
        }
        public GroupOperator Operator { get; set; }
        public IList<IPredicate> Predicates { get; set; }
        public string GetSql(bool prefix, ISqlGenerator sqlGenerator, IDictionary<string, object> parameters, string schemaName, string tableName)
        {
            string seperator = Operator == GroupOperator.And ? " AND " : " OR ";
            return "(" + Predicates.Aggregate(new StringBuilder(),
                                        (sb, p) => (sb.Length == 0 ? sb : sb.Append(seperator)).Append(p.GetSql(prefix, sqlGenerator, parameters, schemaName, tableName)),
                sb =>
                {
                    var s = sb.ToString();
                    if (s.Length == 0) return sqlGenerator.Configuration.Dialect.EmptyExpression;
                    return s;
                }
                                        ) + ")";
        }
    }

    public interface IExistsPredicate : IPredicate
    {
        IPredicate Predicate { get; set; }
        bool Not { get; set; }
    }

    public class ExistsPredicate<TSub> : IExistsPredicate
        where TSub : class
    {
        public Type GetPredicateType()
        {
            return typeof(TSub);
        }
        public IPredicate Predicate { get; set; }
        public bool Not { get; set; }

        public string GetSql(bool prefix, ISqlGenerator sqlGenerator, IDictionary<string, object> parameters, string schemaName, string tableName)
        {
            IClassMapper mapSub = sqlGenerator.Configuration.GetMap(typeof(TSub));
            string sql = string.Format("({0}EXISTS (SELECT 1 FROM {1} WHERE {2}))",
                Not ? "NOT " : string.Empty,
                sqlGenerator.GetTableName(mapSub, schemaName, tableName),
                Predicate.GetSql(prefix, sqlGenerator, parameters, schemaName, tableName));
            return sql;
        }
    }

    public interface ISort
    {
        string SchemaName { get; set; }
        /// <summary>
        /// 表名
        /// </summary>
        string TableName { get; set; }
        string PropertyName { get; set; }
        bool Ascending { get; set; }
        string GetSql(bool prefix, ISqlGenerator sqlGenerator, string schemaName, string tableName);
    }

    public class Sort<T> : ISort
    {
        /// <summary>
        /// 前缀
        /// </summary>
        public string SchemaName { get; set; }
        /// <summary>
        /// 表名
        /// </summary>
        public string TableName { get; set; }
        public string PropertyName { get; set; }
        public bool Ascending { get; set; }

        public string GetSql(bool prefix, ISqlGenerator sqlGenerator, string schemaName, string tableName)
        {
            return sqlGenerator.GetColumnName(prefix,sqlGenerator.Configuration.GetMap(typeof(T)), PropertyName, false, SchemaName ?? schemaName, TableName ?? tableName) + (Ascending ? " ASC" : " DESC");
        }
    }
    internal class SortKey : ISort
    {
        private IClassMapper classMapper { get; set; }
        public SortKey(IClassMapper classMapper)
        {
            this.classMapper = classMapper;
        }

        /// <summary>
        /// 前缀
        /// </summary>
        public string SchemaName { get; set; }
        /// <summary>
        /// 表名
        /// </summary>
        public string TableName { get; set; }
        public string PropertyName { get; set; }
        public bool Ascending { get; set; }

        public string GetSql(bool prefix, ISqlGenerator sqlGenerator, string schemaName, string tableName)
        {
            return sqlGenerator.GetColumnName(prefix,classMapper, PropertyName, false, SchemaName ?? schemaName, TableName ?? tableName) + (Ascending ? " ASC" : " DESC");
        }
    }

    /// <summary>
    /// 在条件组中的操作符 Operator to use when joining predicates in a PredicateGroup.
    /// </summary>
    public enum GroupOperator
    {
        And,
        Or
    }

    public enum JoinOperator
    {
        /// <summary>
        /// 内关联
        /// </summary>
        Inner,
        /// <summary>
        /// 左关联
        /// </summary>
        Left,
        /// <summary>
        /// 右关联
        /// </summary>
        Right,
        /// <summary>
        /// 全关联
        /// </summary>
        Full
    }
}