using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DDD.Core
{
   
    /// <summary>Represents an aggregate root.
    /// </summary>
    public  abstract class IAggregate: IDBModel<Guid>
    {
        [Key]
        public virtual Guid KeyId { get; set; }
        /// <summary>
        /// 归属公司
        /// </summary>
        public virtual Guid CorporationKeyId { get; set; } 
        public DateTime CreateTime { get; set; }
        public Guid CreateUserKeyId { get; set; }
        public DateTime UpdateTime { get; set; }
        public Guid UpdateUserKeyId { get; set; }
        public bool IsDelete { get; set; }
        public int Version { get; set; }

        public   string Name { get; set; }

        public   string No { get; set; }
    }
}
