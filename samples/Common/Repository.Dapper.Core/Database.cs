using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Repository.Dapper.Core.Mapper;
using Repository.Dapper.Core.Sql;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Repository.Dapper.Core
{
    public partial interface IDatabase : IDisposable
    {
        bool HasActiveTransaction { get; }
        IDbConnection Connection { get; }
        void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
        void Commit();
        void Rollback();
        void RunInTransaction(Action action);
        T RunInTransaction<T>(Func<T> func);
        void ClearCache();
        Guid GetNextGuid();
        IClassMapper GetMap<T>() where T : class;

    }

    public partial class Database : IDatabase
    {

        private readonly IDapperImplementor _dapper;

        private IDbTransaction _transaction;

        public Database(IDbConnection connection, ISqlGenerator sqlGenerator)
        {
            _dapper = new DapperImplementor(sqlGenerator);
            Connection = connection;

            if (Connection.State != ConnectionState.Open)
            {
                Connection.Open();
            }
        }
        public Database(IOptions<DataBaseOptions> options, IDapperExtensionsConfiguration Configuration)
        {
            _dapper = new DapperImplementor(new SqlGeneratorImpl(Configuration));
            Connection = options.Value.DbConnection();

            if (Connection.State != ConnectionState.Open)
            {
                Connection.Open();
            }
        }

        public bool HasActiveTransaction
        {
            get
            {
                return _transaction != null;
            }
        }

        public IDbConnection Connection { get; private set; }

        public void Dispose()
        {
            if (Connection.State != ConnectionState.Closed)
            {
                if (_transaction != null)
                {
                    _transaction.Rollback();
                }

                Connection.Close();
            }
        }

        public void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            _transaction = Connection.BeginTransaction(isolationLevel);
        }

        public void Commit()
        {
            _transaction.Commit();
            _transaction = null;
        }

        public void Rollback()
        {
            _transaction.Rollback();
            _transaction = null;
        }

        public void RunInTransaction(Action action)
        {
            BeginTransaction();
            try
            {
                action();
                Commit();
            }
            catch (Exception ex)
            {
                if (HasActiveTransaction)
                {
                    Rollback();
                }

                throw ex;
            }
        }

        public T RunInTransaction<T>(Func<T> func)
        {
            BeginTransaction();
            try
            {
                T result = func();
                Commit();
                return result;
            }
            catch (Exception ex)
            {
                if (HasActiveTransaction)
                {
                    Rollback();
                }

                throw ex;
            }
        }

        public void ClearCache()
        {
            _dapper.SqlGenerator.Configuration.ClearCache();
        }

        public Guid GetNextGuid()
        {
            return _dapper.SqlGenerator.Configuration.GetNextGuid();
        }

        public IClassMapper GetMap<T>() where T : class
        {
            return _dapper.SqlGenerator.Configuration.GetMap<T>();
        }

        
    }

    public class DataBaseOptions
    {
       public ISqlDialect sqlDialect { get; set; }

        public Func<IDbConnection> DbConnection { get; set; }
    }
}
