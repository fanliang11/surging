using System;
using System.Collections.Generic;
using System.Text;

namespace DTO.Core
{
    public abstract class BaseDto
    {

    }
    public class TokenDto: BaseDto
    {
        /// <summary>
        /// 员工编号
        /// </summary>
        public string Token { get; set; }
        public Guid CorporationKeyId { get; set; }
        
    }
    public class IdentifyDto : BaseDto
    {
        /// <summary>
        /// 当前用户keyId
        /// </summary>
     
        public Guid UserKeyId { get; set; }

        /// <summary>
        /// 当前用户名
        /// </summary>
     
        public string UserName { get; set; }

        /// <summary>
        /// 当前用户部门keyId
        /// </summary>
     
        public Guid DepartmentKeyId { get; set; }

        /// <summary>
        /// 部门编号
        /// </summary>
     
        public string DepartmentNo { get; set; }

        /// <summary>
        /// 当前用户部门名称
        /// </summary>
     
        public string DepartmentName { get; set; }

        /// <summary>
        /// 当前用户所属公司keyId
        /// </summary>
     
        public Guid CorporationKeyId { get; set; }


        /// <summary>
        /// 用户登录方式
        /// </summary>
     
        public string LoginType { get; set; }

        /// <summary>
        /// 用户账号
        /// </summary>
     
        public string UserNo { get; set; }

        /// <summary>
        /// 当前用户手机号
        /// </summary>
     
        public string UserPhone { get; set; }

        /// <summary>
        /// 角色code
        /// </summary>
     
        public string RoleCode { get; set; }

        /// <summary>
        /// 角色名称
        /// </summary>
     
        public string RoleName { get; set; }
    }
    }
