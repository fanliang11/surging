using DDD.Core;
using Domain.Auth.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Domain.Auth.Aggregate
{
    /// <summary>
    /// 服务模块(子领域)
    /// </summary>
    [Table("Auth_SubDomains")]
    public class SubDomain : IAggregate
    {
        
        /// <summary>
        /// 模块下的权限
        /// </summary>
        public virtual   List<SubDomainPermission> SubDomainPermissions { get; set; }
         
    }
}
