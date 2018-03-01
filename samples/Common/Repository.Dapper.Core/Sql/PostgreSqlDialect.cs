using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Repository.Dapper.Core.Sql
{
    public class PostgreSqlDialect : SqlDialectBase
    {
        public override string GetIdentitySql(string tableName)
        {
            return "SELECT LASTVAL() AS Id";
        }

        public override string GetPagingSql(string sql, int page, int resultsPerPage, IDictionary<string, object> parameters)
		{
			int startValue = (page-1) * resultsPerPage;
            return GetSetSql(sql, startValue < 0 ? 0 : startValue, resultsPerPage, parameters);
		}
		
		public override string GetSetSql(string sql, int firstResult, int maxResults, IDictionary<string, object> parameters)
		{
			string result = string.Format("{0} LIMIT @maxResults OFFSET @pageStartRowNbr", sql);
			parameters.Add("@maxResults", maxResults);
			parameters.Add("@pageStartRowNbr", firstResult);
			return result;
		}

        public override string GetColumnName(string prefix, string columnName, string alias)
        {
            return base.GetColumnName(prefix, columnName, alias);//.ToLower();
        }

        public override string GetTableName(string schemaName, string tableName, string alias)
        {
            return base.GetTableName(schemaName, tableName, alias);//.ToLower();
        }
        public override string GetOperatorString(Operator Operator, bool Not)
        {
            switch (Operator)
            {
                case Operator.Gt:
                    return Not ? "<=" : ">";
                case Operator.Ge:
                    return Not ? "<" : ">=";
                case Operator.Lt:
                    return Not ? ">=" : "<";
                case Operator.Le:
                    return Not ? ">" : "<=";
                case Operator.Like:
                    return Not ? "NOT ILIKE" : "ILIKE";
                default:
                    return Not ? "<>" : "=";
            }
        }
    }

}
