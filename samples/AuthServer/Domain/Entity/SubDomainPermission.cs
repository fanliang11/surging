using DDD.Core;
using Domain.Auth.Aggregate; 
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Domain.Auth.Entity
{
    /// <summary>
    /// 模块下的权限
    /// </summary>
    [Table("Auth_SubDomain_Permissions")]
    public class SubDomainPermission : BaseEntity
    {
        public string GroupName { get; set; }
        public string GroupNo { get; set; }

        public PermissionCategory PermissionType { get; set; }

        public Guid SubDomainKeyId { get; set; }

        [ForeignKey("SubDomainKeyId")]
        public SubDomain SubDomain { get; set; }
         }

    public enum PermissionCategory
    {
        /// <summary>
        /// 页面
        /// </summary>
        Page=1,
        
        /// <summary>
        /// 动作
        /// </summary>
        Action=2
    }
}
