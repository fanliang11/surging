using DDD.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Domain.Auth.Aggregate
{
    /// <summary>
    /// 平台用户(账号)
    /// </summary>
    [Table("Auth_Users")]
    public class User : IAggregate
    {

        /// <summary>
        /// 密码
        /// </summary>
        public string Pwd { get; set; }


        /// <summary>
        /// 归属员工
        /// </summary>
        public Guid EmployeeKeyID { get; set; }


        //public virtual   List<Department> Departments { get; set; }
         
    }
}
