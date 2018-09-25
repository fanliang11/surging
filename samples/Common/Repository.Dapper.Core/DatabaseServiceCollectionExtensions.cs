using Repository.Dapper.Core.Sql;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Repository.Dapper.Core
{
    public enum ESqlDialect
    {
        DB2,
        MySQL,
        Oracle,
        PostgreSql,
        SqlCe,
        Sqlite,
        SqlServer
    }
    public class SqlDialectUtil
    {
        public static ISqlDialect ConvertESqlDialect(ESqlDialect sqlDialect)
        {
            ISqlDialect SqlDialect = null;
            switch (sqlDialect)
            {
                case ESqlDialect.DB2:
                    SqlDialect = new DB2Dialect();
                    break;
                case ESqlDialect.MySQL:
                    SqlDialect = new MySqlDialect();
                    break;
                case ESqlDialect.Oracle:
                    SqlDialect = new OracleDialect();
                    break;
                case ESqlDialect.PostgreSql:
                    SqlDialect = new PostgreSqlDialect();
                    break;
                case ESqlDialect.SqlCe:
                    SqlDialect = new SqlCeDialect();
                    break;
                case ESqlDialect.Sqlite:
                    SqlDialect = new SqliteDialect();
                    break;
                case ESqlDialect.SqlServer:
                    SqlDialect = new SqlServerDialect();
                    break;
            }
            return SqlDialect;
        }
    }
    public static class DatabaseServiceCollectionExtensions
    {

        /// <summary>
        /// 设置SqlDialect,使用默认用法
        /// </summary>
        /// <param name="services"></param>
        /// <param name="SqlDialect"></param>
        /// <example>
        /// using(var con=new SqlConnection(Configuration.GetConnectionString("DefaultConnection"))
        /// {
        ///     con.Insert<TestData>(data);
        /// }
        /// </example>
        /// <returns></returns>
        public static IServiceCollection AddDapper(this IServiceCollection services, ESqlDialect SqlDialect)
        {
            DapperExtensions.Configure(SqlDialectUtil.ConvertESqlDialect(SqlDialect));

            return services;
        }

        /// <summary>
        /// 使用IDatabase用法
        /// </summary>
        /// <param name="services"></param>
        /// <param name="SqlDialect"></param>
        /// <param name="CreateConnection"></param>
        /// <param name="UseExtension">是否同时使用扩展方法</param>
        /// <returns></returns>
        public static IServiceCollection AddDapperDataBase(this IServiceCollection services, ESqlDialect sqlDialect, Func<IDbConnection> CreateConnection, bool UseExtension = false)
        {
            var SqlDialect = SqlDialectUtil.ConvertESqlDialect(sqlDialect);
            services.AddOptions();
            services.Configure<DataBaseOptions>(opt =>
            {
                opt.DbConnection = CreateConnection;
                opt.sqlDialect = SqlDialectUtil.ConvertESqlDialect(sqlDialect);
            });

            if (UseExtension)
            {
                var Configuration = new DapperExtensionsConfiguration(SqlDialect);
                DapperExtensions.Configure(Configuration);
                services.AddSingleton<IDapperExtensionsConfiguration>(Configuration);
            }
            else
                services.AddSingleton<IDapperExtensionsConfiguration, DapperExtensionsConfiguration>();

            services.AddTransient<IDatabase, Database>();
            return services;
        }
    }
}
