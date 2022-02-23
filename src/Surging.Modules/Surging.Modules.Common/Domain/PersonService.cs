﻿using Surging.IModuleServices.Common;
using Surging.IModuleServices.Common.Models;
using Surging.Modules.Common.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Surging.Core.CPlatform.EventBus.Events;
using Newtonsoft.Json.Linq;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Ioc;
using Surging.Core.ProxyGenerator;
using Surging.Core.KestrelHttpServer.Internal;
using System.IO;
using Surging.Core.KestrelHttpServer;
using Surging.Core.Common;
using Surging.Core.Thrift.Attributes;
using static ThriftCore.Calculator;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;

namespace Surging.Modules.Common.Domain
{
    [ModuleName("Person")] 
    public class PersonService : ProxyServiceBase, IUserService
    {
        #region Implementation of IUserService
        private readonly UserRepository _repository;
        public PersonService(UserRepository repository)
        {
            this._repository = repository;
        }

        public Task<string> GetUserName(int id)
        {
            return GetService<IUserService>("User").GetUserName(id);
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

        public Task<bool> Get(List<UserModel> users)
        {
            return Task.FromResult(true);
        }

        public Task<bool> Update(int id, UserModel model)
        {
            return Task.FromResult(true);
        }


        public Task<UserModel> GetUserById(Guid id)
        {
            return Task.FromResult(new UserModel
            {

            });
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
            await Task.CompletedTask;
        }

        public Task<UserModel> Authentication(AuthenticationRequestData requestData)
        {
            if (requestData.UserName == "admin" && requestData.Password == "admin")
            {
                return Task.FromResult(new UserModel());
            }
            return Task.FromResult<UserModel>(null);
        }

        public Task<IdentityUser> Save(IdentityUser requestData)
        {
            return Task.FromResult(requestData);
        }

        public Task<ApiResult<UserModel>> GetApiResult()
        {
            return Task.FromResult(new ApiResult<UserModel>() { Value = new UserModel { Name = "fanly" }, StatusCode=200 });
        }

        public   Task<string> GetUser(List<int> idList)
        {
            return Task.FromResult("type is List<int>");
        }

        public async Task<bool> UploadFile(HttpFormCollection form1)
        {
            var files = form1.Files;
            foreach (var file in files)
            {
                using (var stream = new FileStream(Path.Combine(AppContext.BaseDirectory, file.FileName), FileMode.Create))
                {
                    await stream.WriteAsync(file.File, 0, (int)file.Length);
                }
            }
            return true;
        }

        public async Task<IActionResult> DownFile(string fileName, string contentType)
        {
            string uploadPath = Path.Combine("C:", fileName);
            if (File.Exists(uploadPath))
            {
                using (var stream = new FileStream(uploadPath, FileMode.Open))
                {

                    var bytes = new Byte[stream.Length];
                    await stream.WriteAsync(bytes, 0, bytes.Length);
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

        public async Task<Dictionary<string, object>> GetAllThings()
        {
            return await Task.FromResult(new Dictionary<string, object> { { "aaa", 12 } });
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
