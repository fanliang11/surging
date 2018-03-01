namespace Repository.EF.Core
{
 
    public class DbContextOption
    {
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public string ConnectionString { get; set; }
        /// <summary>
        /// 实体程序集名称
        /// </summary>
        public string ModelAssemblyName { get; set; }
        /// <summary>
        /// 数据库类型
        /// </summary>
        public DbType DbType { get; set; } = DbType.MSSQLSERVER;
    }

    /// <summary>
    /// 数据库类型枚举
    /// </summary>
    public enum DbType
    {
        /// <summary>
        /// MS SQL Server
        /// </summary>
        MSSQLSERVER=0,
        /// <summary>
        /// Oracle
        /// </summary>
        ORACLE,
        /// <summary>
        /// MySQL
        /// </summary>
        MYSQL,
        /// <summary>
        /// Sqlite
        /// </summary>
        SQLITE,
        /// <summary>
        /// in-memory database
        /// </summary>
        MEMORY,
        /// <summary>
        /// PostgreSQL
        /// </summary>
        NPGSQL
    }
}
