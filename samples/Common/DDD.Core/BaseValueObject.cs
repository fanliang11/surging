using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DDD.Core
{
    public abstract class BaseValueObject : IDBModel<Guid>
    {
        [Key]
        public Guid KeyId { get; set; }
        public virtual Guid CorporationKeyId { get; set; }
        public virtual DateTime CreateTime { get; set; }
        public virtual Guid CreateUserKeyId { get; set; }
        public virtual DateTime UpdateTime { get; set; }
        public virtual Guid UpdateUserKeyId { get; set; }
        public virtual bool IsDelete { get; set; }
        public virtual int Version { get; set; }

        public string Name { get; set; }

        public string No { get; set; }
    }
}
