using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Routing.Template;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using Surging.Core.DotNettyWSServer.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Surging.Core.DotNettyWSServer.Runtime.Implementation
{
   public class DefaultWSServiceEntryProvider : IWSServiceEntryProvider
    {
        #region Field

        private readonly IEnumerable<Type> _types;
        private readonly ILogger<DefaultWSServiceEntryProvider> _logger;
        private readonly CPlatformContainer _serviceProvider;
        private List<WSServiceEntry> _wSServiceEntries;

        #endregion Field

        #region Constructor

        public DefaultWSServiceEntryProvider(IServiceEntryProvider serviceEntryProvider,
            ILogger<DefaultWSServiceEntryProvider> logger,
            CPlatformContainer serviceProvider)
        {
            _types = serviceEntryProvider.GetTypes();
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        #endregion Constructor

        #region Implementation of IUdpServiceEntryProvider

        /// <summary>
        /// 获取服务条目集合。
        /// </summary>
        /// <returns>服务条目集合。</returns>
        public IEnumerable<WSServiceEntry> GetEntries()
        {
            var services = _types.ToArray();
            if (_wSServiceEntries == null)
            {
                _wSServiceEntries = new List<WSServiceEntry>();
                foreach (var service in services)
                {
                    var entry = CreateServiceEntry(service);
                    if (entry != null)
                    {
                        _wSServiceEntries.Add(entry);
                    }
                }
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug($"发现了以下WS服务：{string.Join(",", _wSServiceEntries.Select(i => i.Type.FullName))}。");
                }
            }
            return _wSServiceEntries;
        }
        public WSServiceEntry CreateServiceEntry(Type service)
        {
            WSServiceEntry result = null;
            var routeTemplate = service.GetCustomAttribute<ServiceBundleAttribute>(); 
            var behaviorContract = service.GetCustomAttribute<BehaviorContractAttribute>();
            var objInstance = _serviceProvider.GetInstances(service);
            var behavior = objInstance as WSBehavior;
            var path = RoutePatternParser.Parse(routeTemplate.RouteTemplate, service.Name);
            if (path.Length > 0 && path[0] != '/')
                path = $"/{path}";
            if (behavior != null)
            {
                behavior.Protocol = behaviorContract?.Protocol;
                result = new WSServiceEntry
                {
                    Behavior = behavior,
                    Type = behavior.GetType(),
                    Path = path,
                };
            }
            return result;
        }
        #endregion
    }
}
