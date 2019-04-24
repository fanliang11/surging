using Autofac;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Routing.Template;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using Surging.Core.Protocol.WS.Attributes;
using Surging.Core.Protocol.WS.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using WebSocketCore.Server;

namespace Surging.Core.Protocol.WS.Runtime.Implementation
{
    public class DefaultWSServiceEntryProvider : IWSServiceEntryProvider
    {
        #region Field

        private readonly IEnumerable<Type> _types;
        private readonly ILogger<DefaultWSServiceEntryProvider> _logger;
        private readonly CPlatformContainer _serviceProvider;
        private List<WSServiceEntry> _wSServiceEntries;
        private WebSocketOptions _options;

        #endregion Field

        #region Constructor

        public DefaultWSServiceEntryProvider(IServiceEntryProvider  serviceEntryProvider,
            ILogger<DefaultWSServiceEntryProvider> logger,
            CPlatformContainer serviceProvider,
            WebSocketOptions options)
        {
            _types = serviceEntryProvider.GetTypes();
            _logger = logger;
            _serviceProvider = serviceProvider;
            _options = options;
        }

        #endregion Constructor

        #region Implementation of IServiceEntryProvider

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
        #endregion

        public WSServiceEntry CreateServiceEntry(Type service)
        {
            WSServiceEntry result = null;
            var routeTemplate = service.GetCustomAttribute<ServiceBundleAttribute>();
            var behaviorContract = service.GetCustomAttribute<BehaviorContractAttribute>();
            var objInstance = _serviceProvider.GetInstances(service);
            var behavior = objInstance as WebSocketBehavior;
            var path = RoutePatternParser.Parse(routeTemplate.RouteTemplate, service.Name);
            if (path.Length>0 && path[0] != '/')
                path = $"/{path}";
            if (behavior != null)
                result = new WSServiceEntry
                {
                    Behavior = behavior,
                    Type = behavior.GetType(),
                    Path = path,
                    FuncBehavior = () =>
                    {
                        return GetWebSocketBehavior(service, _options?.Behavior, behaviorContract);
                    }
                };
            return result;
        }

        private WebSocketBehavior GetWebSocketBehavior(Type service,BehaviorOption option, BehaviorContractAttribute contractAttribute)
        {
            var wsBehavior = _serviceProvider.GetInstances(service) as WebSocketBehavior;
            if (option != null)
            {
                wsBehavior.IgnoreExtensions = option.IgnoreExtensions;
                wsBehavior.Protocol = option.Protocol;
                wsBehavior.EmitOnPing = option.EmitOnPing;
            }
            if (contractAttribute != null)
            {
                wsBehavior.IgnoreExtensions = contractAttribute.IgnoreExtensions;
                wsBehavior.Protocol = contractAttribute.Protocol;
                wsBehavior.EmitOnPing = contractAttribute.EmitOnPing;
            } 
            return wsBehavior;
        }
    }
}
