using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Runtime.Client
{
    public interface IServiceHeartbeatManager
    {
        void AddWhitelist(string serviceId);

        bool ExistsWhitelist(string serviceId);
    }
}
