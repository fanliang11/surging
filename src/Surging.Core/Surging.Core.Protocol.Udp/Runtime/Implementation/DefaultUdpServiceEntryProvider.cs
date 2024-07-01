using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Routing.Template;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Surging.Core.Protocol.Udp.Runtime.Implementation
{
   public class DefaultUdpServiceEntryProvider : IUdpServiceEntryProvider
    {
        #region Field

        private readonly IEnumerable<Type> _types;
        private readonly ILogger<DefaultUdpServiceEntryProvider> _logger;
        private readonly CPlatformContainer _serviceProvider;
        private UdpServiceEntry _udpServiceEntry;

        #endregion Field

        #region Constructor

        public DefaultUdpServiceEntryProvider(IServiceEntryProvider serviceEntryProvider,
            ILogger<DefaultUdpServiceEntryProvider> logger,
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
        public UdpServiceEntry GetEntry()
        {
            var services = _types.ToArray();
            if (_udpServiceEntry == null)
            {
                _udpServiceEntry = new UdpServiceEntry();
                foreach (var service in services)
                {
                    var entry = CreateServiceEntry(service);
                    if (entry != null)
                    {
                        _udpServiceEntry = entry;
                        break;
                    }
                }
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug($"发现了以下Udp服务：{_udpServiceEntry.Type.FullName}。");
                }
            }
            return _udpServiceEntry;
        }

        public UdpServiceEntry CreateServiceEntry(Type service)
        {
            UdpServiceEntry result = null;
            var routeTemplate = service.GetCustomAttribute<ServiceBundleAttribute>();
            var objInstance = _serviceProvider.GetInstances(service);
            var behavior = objInstance as UdpBehavior;
            var path = RoutePatternParser.Parse(routeTemplate.RouteTemplate, service.Name);
            if (path.Length > 0 && path[0] != '/')
                path = $"/{path}";
            if (behavior != null)
                result = new UdpServiceEntry
                {
                    Behavior = behavior,
                    Type = behavior.GetType(),
                    Path = path,
                };
            return result;
        }
        #endregion
    }
}
