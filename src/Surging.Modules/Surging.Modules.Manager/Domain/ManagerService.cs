using Surging.Core.ProxyGenerator;
using Surging.IModuleServices.User;
using System.Threading.Tasks;

namespace Surging.Modules.Manager.Domain
{
    /// <summary>
    /// Defines the <see cref="ManagerService" />
    /// </summary>
    public class ManagerService : ProxyServiceBase, IManagerService
    {
        #region 方法

        /// <summary>
        /// The SayHello
        /// </summary>
        /// <param name="name">The name<see cref="string"/></param>
        /// <returns>The <see cref="Task{string}"/></returns>
        public Task<string> SayHello(string name)
        {
            return Task.FromResult($"{name} say:hello");
        }

        #endregion 方法
    }
}