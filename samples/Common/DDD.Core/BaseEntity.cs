using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DDD.Core
{
   public abstract class BaseEntity : IDBModel<Guid>
    {
        [Key]
        public  Guid KeyId { get; set; }
        public virtual Guid CorporationKeyId { get; set; }
        public virtual DateTime CreateTime { get; set; }
        public virtual Guid CreateUserKeyId { get; set; }
        public virtual DateTime UpdateTime { get; set; }
        public virtual Guid UpdateUserKeyId { get; set; }
        public virtual bool IsDelete { get; set; }
        public virtual int Version { get; set; }

        //2-12增加名字、编号
        public string Name { get; set; }

        public string No { get; set; }

        public virtual void SetEditer(Guid? editer)
        {
            if(this.KeyId==Guid.Empty)
            {
                this.CreateTime = DateTime.Now;
                this.CreateUserKeyId = Guid.Empty;
                this.UpdateUserKeyId = Guid.Empty;
                this.UpdateTime = DateTime.Now;
            }
            else
            {
                if (editer.HasValue)
                {
                    this.UpdateUserKeyId = editer.Value;
                    this.UpdateTime = DateTime.Now;
                }
            }
        }
    }
}
