using System;
using System.Collections.Generic;
using System.Text;

namespace DTO.Core
{
    /// <summary>
    /// 接口参数 dto抽象基类
    /// </summary>
    public abstract class BaseRequestDto : BaseDto
    {
        /// <summary>
        /// 服务访问身份信息
        /// </summary>
        public TokenDto Identify { get; set; }

       
    }
    /// <summary>
    /// 实体的增、删、改
    /// </summary>
    public class EntityCUDReq : BaseRequestDto
    {
        public Guid? KeyId { get; set; }
    }
    /// <summary>
    /// 实体上的命令
    /// </summary>
    public class EntityCMDReq : BaseRequestDto
    {

    }

    /// <summary>
    /// 简单的请求
    /// 参数不确定
    /// </summary>
    public class CommonCMDReq : BaseRequestDto
    {
        public string CommonCMD { get; set; }
    }

   
    /// <summary>
    /// 树形列表的搜索
    /// </summary>
    public class BaseTreeSearchReq : BaseRequestDto
    {
        /// <summary>
        /// 搜索条件
        /// </summary>
        public string SearchKey { get; set; }

    }
}
