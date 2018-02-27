using DDD.Core;
using Domain.Org.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Domain.Org.Aggregate
{
    [Table("Org_Corporations")]
    public class Corporation : IAggregate
    {
        /// <summary>
        /// 这个字段没有用，禁止使用
        /// </summary>
        [NotMapped]
        public override Guid KeyId { get => base.KeyId; set => base.KeyId = value; }

        [Key]
        public override Guid CorporationKeyId { get; set; }

        public virtual   List<Department> Departments { get; set; }
        public virtual   List<CorpRole> CorpRoles { get; set; }

        /// <summary>
        /// 注册后激活，初始化管理员数据
        /// </summary>
        public string Activate()
        {
            //是否已经存在虚拟部门
            var dept = this.Departments.Find(a => !a.IsDelete && a.Name == "" && a.ParentKeyId == CorporationKeyId);
            if(dept==null)
            {
                //添加虚拟部门
                dept = new Department()
                {
                     Name=string.Empty,
                      CorporationKeyId=CorporationKeyId,
                       KeyId=Guid.NewGuid(),
                        CreateTime=DateTime.Now,
                         CreateUserKeyId=Guid.Empty,
                          No="000000",
                           ParentKeyId=CorporationKeyId,
                            UpdateTime=DateTime.Now,
                             UpdateUserKeyId=Guid.Empty,
                              Address=string.Empty,
                               NameSpell=string.Empty,
                                Telephone=string.Empty,
                                DepartmentType= DepartmentCategory.Sysinternal,
                                 Employees=new List<Employee> (),
                                  IsDelete=false,
                                   Version=1
                                
                };
                //添加管理员对应的员工
                var empid = Guid.NewGuid();
                dept.Employees.Add(new Employee()
                {
                     Name="超级管理员",
                      No = "SuperManger",
                       KeyId=empid,
                        CorporationKeyId=CorporationKeyId,
                         CreateTime=DateTime.Now,
                          CreateUserKeyId=Guid.Empty,
                           DepartmentKeyId= dept.KeyId,
                            UpdateTime=DateTime.Now,
                             UpdateUserKeyId=Guid.Empty,
                              Version=1
                });
                return empid.ToString();
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
