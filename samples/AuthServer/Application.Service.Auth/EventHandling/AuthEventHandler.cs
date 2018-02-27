using App.Core.Security;
using Application.Interface.Auth.EVENT;
using Application.Interface.Org.EVENT;
using Domain.Auth.Aggregate;
using Repository.Auth;
using Surging.Core.ProxyGenerator;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Application.Service.Auth.EventHandling
{
    //[QueueConsumerAttribute]
    public class AuthEventHandler : IAuthEventHandler
    {
        private readonly UserRepository _userRepository;
        private readonly SubDomainRepository _subDomainRepository;
        private readonly UserQueryRepository _queryUserRepository;
        private readonly SubDomainQueryRepository _querySubDomainRepository;
        //  private ICacheProvider _cacheProvider;
        public AuthEventHandler(UserRepository userRepository, UserQueryRepository queryUserRepository,
         SubDomainRepository subDomainRepository, SubDomainQueryRepository querySubDomainRepository)
        {
            _userRepository = userRepository;
            _subDomainRepository = subDomainRepository;
            _queryUserRepository = queryUserRepository;
            _querySubDomainRepository = querySubDomainRepository;
        }


        public Task Handle(CorporationActivatedEvent @event)
        {
            //需要为企业管理做初始化，并生成管理员账号
            return Task.Run(() =>
            {
                var encryptionService = new EncryptionService();
                _userRepository.Add(new User
                {
                    CorporationKeyId = @event.CorpId,
                    IsDelete = false,
                    EmployeeKeyID = Guid.Parse(@event.EmpId),
                    KeyId = Guid.NewGuid(),
                    Name = "超级管理员",
                    No = "SuperMan",
                    Version = 1,
                    Pwd = encryptionService.EncryptText("123456")
                });

                _userRepository.Commit();
            });

        }
    }
}
