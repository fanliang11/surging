using DDD.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Centa.Agency.Repository.EF
{
    public class UnitOfWork : IUnitOfWork
    {
        private bool disposed = false;
        private DefaultDbContext _context;
        public UnitOfWork()//DefaultDbContext context)
        {
            _context = new DefaultDbContext();// context;
        }
        /*
          dbContext = CallContext.GetData(provider.ConnectionStringName) as AppDbContext;
                               if (dbContext == null)
                               {
                                   dbContext = new AppDbContext(provider.ConnectionString);
                                   dbContext.Configuration.ValidateOnSaveEnabled = false;
                                   //将新创建的 ef上下文对象 存入线程
                                   CallContext.SetData(provider.ConnectionStringName, dbContext);
                               }

                    */
        public int Commit()
        {
            using (var tran = _context.Database.CurrentTransaction ?? _context.Database.BeginTransaction())
            {
                try
                {
                    var result = _context.SaveChanges();
                    tran.Commit();
                    return result;
                }
                catch (Exception)
                {
                    tran.Rollback();
                    throw;
                }
            }
        }

         protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
         
    }
}
