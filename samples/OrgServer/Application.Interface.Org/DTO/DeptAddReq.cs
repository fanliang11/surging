using DTO.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interface.Org
{
    public class DeptEditReq : EntityCUDReq
    {
        /// <summary>
        /// 所属公司ID
        /// </summary>

        public Guid CorporationKeyId { get; set; }
        public Guid? DepartmentKeyId { get; set; }

        /// <summary>
        /// 部门名称
        /// </summary>

        public string DepartmentName { get; set; }

        /// <summary>
        /// 部门名称简拼
        /// </summary>
     
        public string NameSpell { get; set; }

        /// <summary>
        /// 部门编号
        /// </summary>
     
        public string DepartmentNo { get; set; }

        

        /// <summary>
        /// 部门负责人KeyId
        /// </summary>
     
        public Guid? DeptCheifKeyId { get; set; }
        /// <summary>
        /// 部门上一级负责人
        /// </summary>
     
        public Guid? ParentDeptCheifKeyId { get; set; }
        /// <summary>
        /// 部门上一级负责人部门ID
        /// </summary>
     
        public Guid? ParentDeptCheifDeptId { get; set; }
        /// <summary>
        /// 父部门KeyId
        /// </summary>
     
        public Guid? ParentDeptKeyId { get; set; }

        /// <summary>
        /// 部门电话
        /// </summary>
     
        public string Telephone { get; set; }

        /// <summary>
        /// 部门来源（null为A+系统新增，1为CCAI系统新增）
        /// </summary>
     
        public int? SourceObjectFlag { get; set; }
 
        /// <summary>
        /// 纬度
        /// </summary>
     
        public string Latitude { get; set; }
        /// <summary>
        /// 经度
        /// </summary>
     
        public string Longitude { get; set; }

        /// <summary>
        /// 部门类型（普通额外公客池、客源市场）
        /// </summary>
     
       // public DepartmentTypeEnum? DepartmentType { get; set; }
    }

    public class CorpEditReq: EntityCUDReq
    {
        public string CorpName { get; set; }

    }

    public class RoleEditReq : EntityCUDReq
    {
        public string RoleName { get; set; }

    }

    public class EmployeeEditReq : EntityCUDReq
    {
        public string Name { get; set; }
        public string Mobile { get; set; }
        public Guid DeptKeyId { get; set; }
        public Guid RoleKeyId { get; set; }
        public string RoleName { get; set; }



    }
}
