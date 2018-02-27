using System;

namespace DDD.Core
{
    public interface IUnitOfWork: IDisposable
    {
        #region for transaction
        /// <summary>
        /// 提交一个聚合内的所有变更
        /// </summary>
        ///<remarks>
        /// If the entity have fixed properties and any optimistic concurrency problem exists,  
        /// then an exception is thrown
        ///</remarks>
        int Commit();
         

        #endregion

      //  int SaveChanges();
        //int SaveChangesAsync();
         
    }
}
