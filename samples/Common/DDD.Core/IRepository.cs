using System;

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace DDD.Core
{
    public interface IDBModel<TKey>
    {
        TKey KeyId { get; set; }

        TKey CorporationKeyId { get; set; } 
        DateTime CreateTime { get; set; }
        TKey CreateUserKeyId { get; set; }
        DateTime UpdateTime { get; set; }
        TKey UpdateUserKeyId { get; set; }
        bool IsDelete { get; set; }
        int Version { get; set; } 


    }
    public interface IRepository<T, TKey>: IUnitOfWork where T : IAggregate
    {
        void Add(T aggregate); //where T : IAggregate;
        T FindBy(TKey key); //where T : IAggregate;
        void Update(T aggregate); //where T : IAggregate;
        void Delete(TKey key);


    }
}
