using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Enums
{
    public enum EnumReturnCode
    {
        /// <summary>
        /// 成功
        /// </summary>
        [Display(Name = "成功")]
        SUCCESS = 200,
        /// <summary>
        /// 参数错误
        /// </summary>
        [Display(Name = "参数错误")]
        PARAM_ERROR = 101, 
        /// <summary>
        /// 登录错误
        /// </summary>
        [Display(Name = "登录错误")]
        LOGIN_ERROR = 105,
        /// <summary>
        /// 操作失败
        /// </summary>
        [Display(Name = "操作失败")]
        FAIL = 1,
        /// <summary>
        /// 服务端出错啦
        /// </summary>
        [Display(Name = "服务端出错啦")]
        GLOBAL_ERROR = 500,
        /// <summary>
        /// 自定义异常
        /// </summary>
        [Display(Name = "自定义异常")]
        CUSTOM_ERROR = 110,
        /// <summary>
        /// 非法请求
        /// </summary>
        [Display(Name = "非法请求")]
        INVALID_REQUEST = 116,
        /// <summary>
        /// 授权失败
        /// </summary>
        [Display(Name = "授权失败")]
        OAUTH_FAIL = 201,
        /// <summary>
        /// 未授权
        /// </summary>
        [Display(Name = "未授权")]
        DENY = 401,
        /// <summary>
        /// 授权访问失败
        /// </summary>
        [Display(Name = "授权访问失败")]
        FORBIDDEN = 403,
        /// <summary>
        /// Bad Request
        /// </summary>
        [Display(Name = "Bad Request")]
        BAD_REQUEST = 400

    }
}
