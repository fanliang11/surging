using Application.Service.Auth.Dto;
using DTO.Core;
using Surging.Core.CPlatform.Ioc;
using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interface.Auth
{
    [ServiceBundle("api/{Service}")]
    public interface IAuthAppService : IServiceKey
    {

        /// <summary>
        /// 登录系统
        /// </summary>
        /// <returns></returns>
        [Service(Date = "2018-1-30", Director = "刘旭东", Name = "登录系统")]
        Task<TokenDto> SignIn(  LoginReq req);

        /// <summary>
        /// 退出系统
        /// </summary>
        /// <returns></returns>
        [Service(Date = "2018-1-30", Director = "刘旭东", Name = "退出系统")]
        Task<String> SignUp(CommonCMDReq req);

        [Service(Date = "2018-1-30", Director = "刘旭东", Name = "查询系统模块下的所有权限")]
        Task<BaseListResponseDto> FindDomainPermissions(CommonCMDReq req);
    }
}
