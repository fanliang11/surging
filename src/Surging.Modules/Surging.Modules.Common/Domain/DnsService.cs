using Surging.Core.DNS.Runtime;
using Surging.IModuleServices.Common;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Modules.Common.Domain
{
    /// <summary>
    /// Defines the <see cref="DnsService" />
    /// </summary>
    public class DnsService : DnsBehavior, IDnsService
    {
        #region 方法

        /// <summary>
        /// The Resolve
        /// </summary>
        /// <param name="domainName">The domainName<see cref="string"/></param>
        /// <returns>The <see cref="Task{IPAddress}"/></returns>
        public override Task<IPAddress> Resolve(string domainName)
        {
            if (domainName == "localhost")
            {
                return Task.FromResult<IPAddress>(IPAddress.Parse("127.0.0.1"));
            }
            return Task.FromResult<IPAddress>(null);
        }

        #endregion 方法
    }
}