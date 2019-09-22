using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Routing.Template;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Surging.Core.ServiceHosting.Extensions.Runtime.Implementation
{
    public class DefaultBackgroundServiceEntryProvider: IBackgroundServiceEntryProvider
    {
        #region Field
        private readonly IEnumerable<Type> _types;
        private readonly ILogger<DefaultBackgroundServiceEntryProvider> _logger;
        private readonly CPlatformContainer _serviceProvider;
        private List<BackgroundServiceEntry> _backgroundServiceEntries;

        #endregion Field

        #region Constructor

        public DefaultBackgroundServiceEntryProvider(IServiceEntryProvider serviceEntryProvider,
            ILogger<DefaultBackgroundServiceEntryProvider> logger,
            CPlatformContainer serviceProvider)
        {
            _types = serviceEntryProvider.GetTypes();
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        #endregion Constructor

        #region Implementation of IUdpServiceEntryProvider


        public IEnumerable<BackgroundServiceEntry> GetEntries()
        {
            var services = _types.ToArray();
            if (_backgroundServiceEntries == null)
            {
                _backgroundServiceEntries = new List<BackgroundServiceEntry>();
                foreach (var service in services)
                {
                    var entry = CreateServiceEntry(service);
                    if (entry != null)
                    {
                        _backgroundServiceEntries.Add(entry);
                    }
                }
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug($"发现了以下后台托管服务：{string.Join(",", _backgroundServiceEntries.Select(i => i.Type.FullName))}。");
                }
            }
            return _backgroundServiceEntries;
        }


        public BackgroundServiceEntry CreateServiceEntry(Type service)
        {
            BackgroundServiceEntry result = null;
            var routeTemplate = service.GetCustomAttribute<ServiceBundleAttribute>();
            var objInstance = _serviceProvider.GetInstances(service);
            var behavior = objInstance as BackgroundServiceBehavior;
            var path = RoutePatternParser.Parse(routeTemplate.RouteTemplate, service.Name);
            if (path.Length > 0 && path[0] != '/')
                path = $"/{path}";
            if (behavior != null)
                result = new BackgroundServiceEntry
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
