using Surging.Core.Common;
using Surging.Core.CPlatform.EventBus.Events;
using Surging.Core.CPlatform.EventBus.Implementation;
using Surging.Core.CPlatform.Ioc;
using Surging.Core.CPlatform.Transport.Implementation;
using Surging.Core.KestrelHttpServer;
using Surging.Core.KestrelHttpServer.Internal;
using Surging.Core.ProxyGenerator;
using Surging.IModuleServices.Common;
using Surging.IModuleServices.Common.Models;
using Surging.IModuleServices.User;
using Surging.Modules.Common.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Surging.Modules.Common.Domain
{
    /// <summary>
    /// Defines the <see cref="UserService" />
    /// </summary>
    [ModuleName("User")]
    public class UserService : ProxyServiceBase, IUserService
    {
        #region 字段

        /// <summary>
        /// Defines the _repository
        /// </summary>
        private readonly UserRepository _repository;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="UserService"/> class.
        /// </summary>
        /// <param name="repository">The repository<see cref="UserRepository"/></param>
        public UserService(UserRepository repository)
        {
            this._repository = repository;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The Authentication
        /// </summary>
        /// <param name="requestData">The requestData<see cref="AuthenticationRequestData"/></param>
        /// <returns>The <see cref="Task{UserModel}"/></returns>
        public Task<UserModel> Authentication(AuthenticationRequestData requestData)
        {
            if (requestData.UserName == "admin" && requestData.Password == "admin")
            {
                return Task.FromResult(new UserModel() { UserId = 22, Name = "admin" });
            }
            return Task.FromResult<UserModel>(null);
        }

        /// <summary>
        /// The DownFile
        /// </summary>
        /// <param name="fileName">The fileName<see cref="string"/></param>
        /// <param name="contentType">The contentType<see cref="string"/></param>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
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

        /// <summary>
        /// The Exists
        /// </summary>
        /// <param name="id">The id<see cref="int"/></param>
        /// <returns>The <see cref="Task{bool}"/></returns>
        public Task<bool> Exists(int id)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// The Get
        /// </summary>
        /// <param name="users">The users<see cref="List{UserModel}"/></param>
        /// <returns>The <see cref="Task{bool}"/></returns>
        public Task<bool> Get(List<UserModel> users)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// The GetAllThings
        /// </summary>
        /// <returns>The <see cref="Task{Dictionary{string, object}}"/></returns>
        public async Task<Dictionary<string, object>> GetAllThings()
        {
            return await Task.FromResult(new Dictionary<string, object> { { "aaa", 12 } });
        }

        /// <summary>
        /// The GetApiResult
        /// </summary>
        /// <returns>The <see cref="Task{ApiResult{UserModel}}"/></returns>
        public Task<ApiResult<UserModel>> GetApiResult()
        {
            return Task.FromResult(new ApiResult<UserModel>() { Value = new UserModel { Name = "fanly" }, StatusCode = 200 });
        }

        /// <summary>
        /// The GetDictionary
        /// </summary>
        /// <returns>The <see cref="Task{bool}"/></returns>
        public Task<bool> GetDictionary()
        {
            return Task.FromResult<bool>(true);
        }

        /// <summary>
        /// The GetUser
        /// </summary>
        /// <param name="idList">The idList<see cref="List{int}"/></param>
        /// <returns>The <see cref="Task{string}"/></returns>
        public Task<string> GetUser(List<int> idList)
        {
            return Task.FromResult("type is List<int>");
        }

        /// <summary>
        /// The GetUser
        /// </summary>
        /// <param name="user">The user<see cref="UserModel"/></param>
        /// <returns>The <see cref="Task{UserModel}"/></returns>
        public Task<UserModel> GetUser(UserModel user)
        {
            return Task.FromResult(new UserModel
            {
                Name = "fanly",
                Age = 18
            });
        }

        /// <summary>
        /// The GetUserById
        /// </summary>
        /// <param name="id">The id<see cref="Guid"/></param>
        /// <returns>The <see cref="Task{UserModel}"/></returns>
        public Task<UserModel> GetUserById(Guid id)
        {
            return Task.FromResult(new UserModel
            {
            });
        }

        /// <summary>
        /// The GetUserId
        /// </summary>
        /// <param name="userName">The userName<see cref="string"/></param>
        /// <returns>The <see cref="Task{int}"/></returns>
        public Task<int> GetUserId(string userName)
        {
            var xid = RpcContext.GetContext().GetAttachment("xid");
            return Task.FromResult(1);
        }

        /// <summary>
        /// The GetUserLastSignInTime
        /// </summary>
        /// <param name="id">The id<see cref="int"/></param>
        /// <returns>The <see cref="Task{DateTime}"/></returns>
        public Task<DateTime> GetUserLastSignInTime(int id)
        {
            return Task.FromResult(new DateTime(DateTime.Now.Ticks));
        }

        /// <summary>
        /// The GetUserName
        /// </summary>
        /// <param name="id">The id<see cref="int"/></param>
        /// <returns>The <see cref="Task{string}"/></returns>
        public async Task<string> GetUserName(int id)
        {
            var text = await this.GetService<IManagerService>().SayHello("fanly");
            return await Task.FromResult<string>(text);
        }

        /// <summary>
        /// The PublishThroughEventBusAsync
        /// </summary>
        /// <param name="evt">The evt<see cref="IntegrationEvent"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task PublishThroughEventBusAsync(IntegrationEvent evt)
        {
            Publish(evt);
            await Task.CompletedTask;
        }

        /// <summary>
        /// The Save
        /// </summary>
        /// <param name="requestData">The requestData<see cref="IdentityUser"/></param>
        /// <returns>The <see cref="Task{IdentityUser}"/></returns>
        public Task<IdentityUser> Save(IdentityUser requestData)
        {
            return Task.FromResult(requestData);
        }

        /// <summary>
        /// The SetSex
        /// </summary>
        /// <param name="sex">The sex<see cref="Sex"/></param>
        /// <returns>The <see cref="Task{Sex}"/></returns>
        public async Task<Sex> SetSex(Sex sex)
        {
            return await Task.FromResult(sex);
        }

        /// <summary>
        /// The Try
        /// </summary>
        /// <returns>The <see cref="Task"/></returns>
        public async Task Try()
        {
            Console.WriteLine("start");
            await Task.Delay(5000);
            Console.WriteLine("end");
        }

        /// <summary>
        /// The TryThrowException
        /// </summary>
        /// <returns>The <see cref="Task"/></returns>
        public Task TryThrowException()
        {
            throw new Exception("用户Id非法！");
        }

        /// <summary>
        /// The Update
        /// </summary>
        /// <param name="id">The id<see cref="int"/></param>
        /// <param name="model">The model<see cref="UserModel"/></param>
        /// <returns>The <see cref="Task{bool}"/></returns>
        public Task<bool> Update(int id, UserModel model)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// The UploadFile
        /// </summary>
        /// <param name="form">The form<see cref="HttpFormCollection"/></param>
        /// <returns>The <see cref="Task{bool}"/></returns>
        public async Task<bool> UploadFile(HttpFormCollection form)
        {
            var files = form.Files;
            foreach (var file in files)
            {
                using (var stream = new FileStream(Path.Combine(AppContext.BaseDirectory, file.FileName), FileMode.OpenOrCreate))
                {
                    await stream.WriteAsync(file.File, 0, (int)file.Length);
                }
            }
            return true;
        }

        #endregion 方法
    }
}