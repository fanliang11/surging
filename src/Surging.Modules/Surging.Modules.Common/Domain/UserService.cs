﻿
using Surging.Core.Common;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.EventBus.Events;
using Surging.Core.CPlatform.EventBus.Implementation;
using Surging.Core.CPlatform.Ioc;
using Surging.Core.CPlatform.Transport.Implementation;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.KestrelHttpServer;
using Surging.Core.KestrelHttpServer.Internal;
using Surging.Core.ProxyGenerator;
using Surging.Core.Thrift.Attributes;
using Surging.IModuleServices.Common;
using Surging.IModuleServices.Common.Models;
using Surging.IModuleServices.User;
using Surging.Modules.Common.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using static ThriftCore.Calculator;

namespace Surging.Modules.Common.Domain
{
    [ModuleName("User")]  
    public class UserService : ProxyServiceBase, IUserService
    {
        #region Implementation of IUserService
        private readonly UserRepository _repository;
        public UserService(UserRepository repository)
        {
            this._repository = repository;
        }

        public async Task<string> GetUserName(int id)
        {
            var text = await GetService<IManagerService>().SayHello("fanly"); 
            return await Task.FromResult<string>(text);
        }

        public Task<bool> Exists(int id)
        {
            return Task.FromResult(true);
        }

        public Task<UserModel> GetUserById(Guid id)
        {
            return Task.FromResult(new UserModel
            {

            });
        }

        public Task<int> GetUserId(string userName)
        {
            var xid = RpcContext.GetContext().GetAttachment("xid");
            return Task.FromResult(1);
        }

        public Task<DateTime> GetUserLastSignInTime(int id)
        {
            return Task.FromResult(new DateTime(DateTime.Now.Ticks));
        }

        public Task<bool> Get(List<UserModel> users)
        {
            return Task.FromResult(true);
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

        public Task<bool> GetDictionary()
        {
            return Task.FromResult<bool>(true);
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
            Publish(evt);
            await Task.CompletedTask;
        }

        public Task<UserModel> Authentication(AuthenticationRequestData requestData)
        {
            if (requestData.UserName == "admin" && requestData.Password == "admin")
            {
                return Task.FromResult(new UserModel() { UserId = 22, Name = "admin" });
            }
            return Task.FromResult<UserModel>(null);
        }

        public Task<IdentityUser> Save(IdentityUser requestData)
        {
            return Task.FromResult(requestData);
        }

        public Task<ApiResult<UserModel>> GetApiResult()
        {
            return Task.FromResult(new ApiResult<UserModel>() { Value = new UserModel { Name = "fanly" }, StatusCode = 200 });
        }

        public async Task<bool> UploadFile(HttpFormCollection form1)
        {
            var files = form1.Files;
            foreach (var file in files)
            {
                using (var stream = new FileStream(Path.Combine(AppContext.BaseDirectory, file.FileName), FileMode.OpenOrCreate))
                {
                    await stream.WriteAsync(file.File, 0, (int)file.Length);
                }
            }
            return true;
        }

        public Task<string> GetUser(List<int> idList)
        {
            return Task.FromResult("type is List<int>");
        }

        public async Task<Dictionary<string, object>> GetAllThings()
        {
            return await Task.FromResult(new Dictionary<string, object> { { "aaa", 12 } });
        }

        public async Task<IActionResult> DownFile(string fileName, string contentType)
        {
            string uploadPath = Path.Combine(AppContext.BaseDirectory, fileName);
            if (File.Exists(uploadPath))
            {
                using (var stream = new FileStream(uploadPath, FileMode.Open))
                {
                    var bytes = new Byte[stream.Length];
                    await stream.ReadAsync(bytes, 0, bytes.Length);
                    stream.Dispose();
                    return new FileContentResult(bytes, contentType, fileName);
                }
            }
            else
            {
                throw new FileNotFoundException(fileName);
            }
        }

        public async Task<Sex> SetSex(Sex sex)
        {
            return await Task.FromResult(sex);
        }

        public Task<bool> RemoveUser(UserModel user)
        {
            return Task.FromResult(true);
        }
        public Task<int> ReactiveTest(int value)
        { 
            ISubject<int> subject = new ReplaySubject<int>();
            var result = 0;
            var exception = new Exception("");
            subject.Subscribe((temperature) => result = temperature, temperature => exception = temperature, async () => await Write(result, default, exception.Message));
             
            GetGenerateObservable().Subscribe(subject);
            return Task.FromResult<int>(default);
        }
        #endregion Implementation of IUserService

        private static IObservable<int> GetGenerateObservable()
        { 
            return Observable.Return(30, ThreadPoolScheduler.Instance);
        }
    }
}