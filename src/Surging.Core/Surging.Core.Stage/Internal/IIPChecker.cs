using Surging.Core.Stage.Internal.Implementation;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Surging.Core.Stage.Internal
{
   public interface IIPChecker
    {
        bool IsBlackIp(IPAddress ip, string routePath);
        void AddCheckIpAddresses(IpCheckerProperties checkerProperties);

        void Remove(IpCheckerProperties checkerProperties);
    }
}
