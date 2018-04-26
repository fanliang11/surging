using DDD.Core;
using Domain.Org.ValueObject;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Domain.Org.Entity
{
    /// <summary>
    /// 公司下的角色
    /// </summary>
    [Table("Org_CorpRoles")]
    public class CorpRole: BaseEntity
    {

        public virtual   List<RolePermission> RolePermissions { get; set; }

    }
}
