using Surging.Core.System.Ioc;
using Surging.IModuleServices.Common;
using Surging.IModuleServices.Common.Models;
using Surging.Modules.Common.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Surging.Core.CPlatform.EventBus.Events;
using Surging.Core.CPlatform.EventBus.Implementation;
using System.Threading;

namespace Surging.Modules.Common.Domain
{
    [ModuleName("User")]
    public class UserService : IUserService
    {
        #region Implementation of IUserService
        private readonly UserRepository _repository;
        private readonly IEventBus _eventBus;
        public UserService(UserRepository repository, IEventBus eventBus)
        {
            this._repository = repository;
            this._eventBus = eventBus;
        }

        public Task<string> GetUserName(int id)
        {
            return Task.FromResult($"id:{id} is name fanly.");
        }

        public Task<bool> Exists(int id)
        {
            return Task.FromResult(true);
        }

        public Task<int> GetUserId(string userName)
        {
            return Task.FromResult(1);
        }

        public Task<DateTime> GetUserLastSignInTime(int id)
        {
            return Task.FromResult(DateTime.Now);
        }

        public Task<UserModel> GetUser(UserModel user)
        {
            return Task.FromResult(new UserModel
            {
                Name = "fanly",
                Age = 18
            });
        }

        public Task<bool> Update(int id, UserModel model)
        {
            return Task.FromResult(true);
        }

        public Task<IDictionary<string, string>> GetDictionary()
        {
            return Task.FromResult<IDictionary<string, string>>(new Dictionary<string, string> { { "key", "value" } });
        }

        public async Task Try()
        {
            Console.WriteLine("start");
            await Task.Delay(5000);
            Console.WriteLine("end");
        }

        public Task TryThrowException()
        {
            throw new Exception("用户Id非法！");
        }

        public async Task PublishThroughEventBusAsync(IntegrationEvent evt)
        {
            _eventBus.Publish(evt);
            await Task.CompletedTask;
        }

        #endregion Implementation of IUserService
    }
}