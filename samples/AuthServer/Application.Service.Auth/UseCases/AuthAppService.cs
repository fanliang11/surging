using App.Core.Security;
using Application.Interface.Auth;
using Application.Service.Auth.Dto;
using Domain.Auth.Aggregate;
using DTO.Core;
using Repository.Auth;
using Surging.Core.Caching;
using Surging.Core.CPlatform.Ioc;
using Surging.Core.ProxyGenerator;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Service.Auth
{
    [ModuleName("Auth")]
    public partial class AuthAppService : ProxyServiceBase, IAuthAppService
    {
        private readonly UserRepository _userRepository;
        private readonly SubDomainRepository _subDomainRepository;
        private readonly UserQueryRepository _queryUserRepository;
        private readonly SubDomainQueryRepository _querySubDomainRepository;
        //  private ICacheProvider _cacheProvider;
        public AuthAppService(UserRepository userRepository, UserQueryRepository queryUserRepository,
         SubDomainRepository subDomainRepository, SubDomainQueryRepository querySubDomainRepository)
        {
            _userRepository = userRepository;
            _subDomainRepository = subDomainRepository;
            _queryUserRepository = queryUserRepository;
            _querySubDomainRepository = querySubDomainRepository;
        }

        public Task<TokenDto> SignIn(LoginReq req)
        {
            User user=null;
            try { 
            //缓存增加当前用户相关信息
            //_cacheProvider= CacheContainer.GetInstances<ICacheProvider>("Redis");
            //_cacheProvider.Add("UserKeyId=22", "{ UserKeyId=22,TenantId=3}");
            var encryptionService = new EncryptionService();
            var pwde = encryptionService.EncryptText(req.Pwd);
              user = _queryUserRepository.GetSingle(a => a.CorporationKeyId == req.CorporationKeyId && !a.IsDelete && a.Pwd == pwde && a.No == req.UserName);
            }
            catch (Exception ex)
            {
                string ss = ex.Message;
            }
            if (user != null)
            {
                return Task.FromResult(new TokenDto { CorporationKeyId = user.CorporationKeyId, Token =  user.EmployeeKeyID.ToString() });

            }
            else
            {
                return Task.FromResult<TokenDto>(null);
            }
        }

        public Task<string> SignUp(CommonCMDReq req)
        {
            throw new NotImplementedException();
        }
    }
}
