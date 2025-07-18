using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.ProxyGenerator;
using Surging.IModuleServices.Common;
using Surging.IModuleServices.Common.Models;
using Surging.IModuleServices.Manager; 

namespace Surging.Modules.Manager.Domain
{
    public class ManagerService : ProxyServiceBase, IManagerService
    {
        private readonly IServiceProxyProvider _serviceProxyProvider;
        public ManagerService(IServiceProxyProvider serviceProxyProvider)
        {
            _serviceProxyProvider = serviceProxyProvider;
        }

        public async Task<string> SayHello(string name)
        {
            //GetService<IUserService>("User").GetUserId("fanly");
            Dictionary<string, object> model = new Dictionary<string, object>();
            model.Add("name", name);
            string path = "api/hello/say";

            var watch = Stopwatch.StartNew();

            for (var i = 0; i < 10000; i++)
            {
                try
                {
                   // string result =await _serviceProxyProvider.Invoke<string>(model, path, null);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            watch.Stop(); 
            return watch.ElapsedMilliseconds.ToString();
        }

        public async Task<string> Say(string name)
        {
            var result = await GetService<IUserService>().GetUser(new UserModel { Name = "fanly1", Age = 12, Sex = Sex.Man, UserId = 21 });
            return await Task.FromResult($"{name}: say hello");
        }

        public Task<bool> Test(string test, string ToList, string DomainID)
        {
            return Task.FromResult(true);
        }
    }
}
