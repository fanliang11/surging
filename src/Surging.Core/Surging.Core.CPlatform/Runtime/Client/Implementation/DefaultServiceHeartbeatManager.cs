﻿using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Support;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Runtime.Client.Implementation
{
    public class DefaultServiceHeartbeatManager : IServiceHeartbeatManager
    {
        private readonly ConcurrentBag<string> _whitelist = new ConcurrentBag<string>();
       
         
        public void AddWhitelist(string serviceId)
        {
            if(!_whitelist.Contains(serviceId))
            _whitelist.Add(serviceId);
        }

        public bool ExistsWhitelist(string serviceId)
        {
            return _whitelist.Contains(serviceId);
        }
    }
}
