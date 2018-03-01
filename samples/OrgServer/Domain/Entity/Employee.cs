using DDD.Core;
using Domain.Org.ValueObject;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Domain.Org.Entity
{
    [Table("Org_Employees")]
    public class Employee : BaseEntity
    {
        public string NameSpell { get; set; }
        public string Mobile { get; set; }
        public string WxNo { get; set; }
        public string Email { get; set; }
        public string PhotoPath { get; set; }
        public string Sex { get; set; }
        public string Signature { get; set; }

        public Guid DepartmentKeyId { get; set; }
        [ForeignKey("DepartmentKeyId")]
        public Department OwnDepartment { get; set; }
        /// <summary>
        /// 员工拥有的角色
        /// </summary>
        public Guid RoleKeyId{ get;set;}
        public string RoleName{ get;set; }
    }
}
