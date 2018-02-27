using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using Config.Core.Extensions;
using DDD.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Extensions;
using Microsoft.Extensions.Configuration;
using Surging.Core.CPlatform;
using Z.EntityFramework.Plus;

namespace Repository.EF.Core
{
    public sealed class DefaultDbContext : DbContext
    {
        private DbContextOption _option;


        public DefaultDbContext()
        {
            _option = new DbContextOption
            {
                ConnectionString = DBConfig.Configuration["ConnectionString"],
                ModelAssemblyName = DBConfig.Configuration["ModelAssemblyName"],
                DbType = DbType.MSSQLSERVER
            };
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="option"></param>
        public DefaultDbContext(DbContextOption option)
        {
            if(option==null)
                throw new ArgumentNullException(nameof(option));
            if(string.IsNullOrEmpty(option.ConnectionString))
                throw new ArgumentNullException(nameof(option.ConnectionString));
            if (string.IsNullOrEmpty(option.ModelAssemblyName))
                throw new ArgumentNullException(nameof(option.ModelAssemblyName));
            _option = option;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            switch (_option.DbType)
            {
                case DbType.ORACLE:
                    throw new NotSupportedException("Oracle EF Core Database Provider is not yet available.");
                //case DbType.MYSQL:
                //    optionsBuilder.UseMySql(_option.ConnectionString);
                //    break;
                //case DbType.SQLITE:
                //    optionsBuilder.UseSqlite(_option.ConnectionString);
                //    break;
                //case DbType.MEMORY:
                //    optionsBuilder.UseInMemoryDatabase(_option.ConnectionString);
                //    break;
                //case DbType.NPGSQL:
                //    optionsBuilder.UseNpgsql(_option.ConnectionString);
                //    break;
                default:
                    optionsBuilder.UseSqlServer(_option.ConnectionString);
                    break;
            }
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            AddEntityTypes(modelBuilder);
            base.OnModelCreating(modelBuilder);
        }

        private void AddEntityTypes(ModelBuilder modelBuilder)
        {
            var assembly = Assembly.Load(_option.ModelAssemblyName);
            var types = assembly?.GetTypes();
            var list = types?.Where(t =>
                t.IsClass && !t.IsGenericType && !t.IsAbstract&&
                t.GetInterfaces().Any(m => m.IsGenericType && m.GetGenericTypeDefinition() == typeof(IDBModel<>))).ToList();
            if (list != null && list.Any())
            {
                list.ForEach(t =>
                {
                    if (modelBuilder.Model.FindEntityType(t) == null)
                        modelBuilder.Model.AddEntityType(t);
                });
            }
        }

        /// <summary>
        /// ExecuteSqlWithNonQuery
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public int ExecuteSqlWithNonQuery(string sql, params object[] parameters)
        {
            return Database.ExecuteSqlCommand(sql,
                CancellationToken.None,
                parameters);
        }

        /// <summary>
        /// edit an entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int Edit<T>(T entity) where T : class
        {
            Entry(entity).State = EntityState.Modified;
            return SaveChanges();
        }

        /// <summary>
        /// edit entities.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <returns></returns>
        public int EditRange<T>(ICollection<T> entities) where T : class
        {
            Set<T>().AttachRange(entities.ToArray());
            return SaveChanges();
        }

        /// <summary>
        /// update query datas by columns.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where"></param>
        /// <param name="updateExp"></param>
        /// <returns></returns>
        public int Update<T>(Expression<Func<T, bool>> @where, Expression<Func<T, T>> updateExp)
            where T : class
        {
            return Set<T>().Where(@where).Update(updateExp);
        }

        /// <summary>
        /// update data by columns.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="updateColumns"></param>
        /// <returns></returns>
        public int Update<T>(T model, params string[] updateColumns) where T : class
        {
            if (updateColumns != null && updateColumns.Length > 0)
            {
                if (Entry(model).State == EntityState.Added ||
                    Entry(model).State == EntityState.Detached) Set<T>().Attach(model);
                foreach (var propertyName in updateColumns)
                {
                    Entry(model).Property(propertyName).IsModified = true;
                }
            }
            else
            {
                Entry(model).State = EntityState.Modified;
            }
            return SaveChanges();
        }

        /// <summary>
        /// delete by query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where"></param>
        /// <returns></returns>
        public int Delete<T>(Expression<Func<T, bool>> @where) where T : class
        {
            Set<T>().Where(@where).Delete();
            return SaveChanges();
        }

        /*
        /// <summary>
        /// bulk insert by sqlbulkcopy, and with transaction.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <param name="destinationTableName"></param>
        public void BulkInsert<T>(IList<T> entities, string destinationTableName = null) where T : class
        {
            if (entities == null || !entities.Any()) return;
            if (string.IsNullOrEmpty(destinationTableName))
            {
                var mappingTableName = typeof(T).GetCustomAttribute<TableAttribute>()?.Name;
                destinationTableName = string.IsNullOrEmpty(mappingTableName) ? typeof(T).Name : mappingTableName; 
            }
            using (var dt = entities.ToDataTable())
            {
                using (var conn = new SqlConnection(_option.ConnectionString))
                {
                    conn.Open();
                    using (var tran = conn.BeginTransaction())
                    {
                        try
                        {
                            var bulk = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, tran);
                            bulk.BatchSize = entities.Count;
                            bulk.DestinationTableName = destinationTableName;
                            bulk.EnableStreaming = true;
                            bulk.WriteToServerAsync(dt);
                            tran.Commit();
                        }
                        catch (Exception)
                        {
                            tran.Rollback();
                            throw;
                        }
                    }
                    conn.Close();
                }
            }
        }
        */
        public List<TView> SqlQuery<T,TView>(string sql, params object[] parameters) 
            where T : class
            where TView : class
        {
            return Set<T>().FromSql(sql, parameters).Cast<TView>().ToList();
        }

    }
}