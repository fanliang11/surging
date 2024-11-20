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
using System.Threading.Tasks;

namespace Surging.Core.Protocol.WebService.Runtime.Implementation
{
    public class DefaultWebServiceEntryProvider : IWebServiceEntryProvider
    {
        #region Field
        private readonly IServiceEntryProvider _serviceEntryProvider;
        private readonly IEnumerable<Type> _types;
        private readonly ILogger<DefaultWebServiceEntryProvider> _logger;
        private readonly CPlatformContainer _serviceProvider;
        private List<WebServiceEntry> _webServiceEntries;

        #endregion Field

        #region Constructor

        public DefaultWebServiceEntryProvider(IServiceEntryProvider serviceEntryProvider,
            ILogger<DefaultWebServiceEntryProvider> logger,
            CPlatformContainer serviceProvider)
        {
            _types = serviceEntryProvider.GetTypes();
            _serviceEntryProvider = serviceEntryProvider;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        #endregion Constructor

        #region Implementation of IServiceEntryProvider

        /// <summary>
        /// 获取服务条目集合。
        /// </summary>
        /// <returns>服务条目集合。</returns>
        public IEnumerable<WebServiceEntry> GetEntries()
        {
            var services = _types.ToArray();
            if (_webServiceEntries == null)
            {
                _webServiceEntries = new List<WebServiceEntry>();
                foreach (var service in services)
                {
                    var entries = CreateServiceEntries(service);
                    if (entries != null)
                    {
                        _webServiceEntries.AddRange(entries);
                    }
                }
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug($"发现了以下WebService服务：{string.Join(",", _webServiceEntries.Select(i => i.Type.FullName))}。");
                }
            }
            return _webServiceEntries;
        }
        #endregion


        public List<WebServiceEntry> CreateServiceEntries(Type service)
        {
            List<WebServiceEntry> result = new List<WebServiceEntry>();
            var routeTemplate = service.GetCustomAttribute<ServiceBundleAttribute>();
            var objInstance = _serviceProvider.GetInstances(service);
            var behavior = objInstance as WebServiceBehavior;
            var path = RoutePatternParser.Parse(routeTemplate?.RouteTemplate, service.Name);
            if (path.Length > 0 && path[0] != '/')
                path = $"/{path}";
            if (behavior != null)
            {
                var entries = _serviceEntryProvider.GetALLEntries().Where(p => p.Type == service).ToList();
                foreach (var entry in entries)
                {
                    result.Add(new WebServiceEntry
                    {
                        Behavior = behavior,
                        BaseType = service,
                        Type = behavior.GetType(),
                        Path = entry.RoutePath,
                    });
                }
            }
            return result;
        }
    }
}
