using DDD.Core;
using Domain.Org.Aggregate;
using Domain.Org.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Domain.Org.Entity
{
    [Table("Org_Departments")]
     public class Department : BaseEntity
    {
        public Guid ParentKeyId { get; set; }

        public string NameSpell { get; set; }

        public string Telephone { get; set; }
        public string Address { get; set; }
        public DepartmentCategory DepartmentType { get; set; }


        [ForeignKey("CorporationKeyId")]
        public Corporation OwnCorporation { get; set; }
        public virtual List<Employee> Employees { get; set; }
    }

    public enum DepartmentCategory
    {
        /// <summary>
        /// 系统为企业内置的虚拟部门
        /// </summary>
        Sysinternal=0,
        /// <summary>
        /// 公司自己添加维护的部门
        /// </summary>
        Bizz =1
    }
}
