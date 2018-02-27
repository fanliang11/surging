using DDD.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace  Domain.Org.ValueObject
{
    /// <summary>
    /// 角色下的权限
    /// </summary>
    [Table("Org_Role_Permissions")]
    public class RolePermission : BaseValueObject
    {
        /// <summary>
        /// 服务ID（模块）
        /// </summary>
        public Guid SubDomainKeyId { get; set; }
        /// <summary>
        /// 具体的权限，页面和动作
        /// </summary>
        public Guid SubDomainPermissionKeyId { get; set; }

        public int PermissionType { get; set; }

        

        [ForeignKey("CorpRole")]
        public Guid CorpRoleKeyId { get; set; }

    }
}
