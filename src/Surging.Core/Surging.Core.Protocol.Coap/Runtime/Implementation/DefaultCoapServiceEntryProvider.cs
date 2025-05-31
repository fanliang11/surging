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

namespace Surging.Core.Protocol.Coap.Runtime.Implementation
{
    internal class DefaultCoapServiceEntryProvider : ICoapServiceEntryProvider
    {
        #region Field

        private readonly IEnumerable<Type> _types;
        private readonly ILogger<DefaultCoapServiceEntryProvider> _logger;
        private readonly CPlatformContainer _serviceProvider;
        private List<CoapServiceEntry> _coapServiceEntries; 

        #endregion Field

        #region Constructor

        public DefaultCoapServiceEntryProvider(IServiceEntryProvider serviceEntryProvider,
            ILogger<DefaultCoapServiceEntryProvider> logger,
            CPlatformContainer serviceProvider)
        {
            _types = serviceEntryProvider.GetTypes();
            _logger = logger;
            _serviceProvider = serviceProvider; 
        }

        #endregion Constructor

        #region Implementation of IServiceEntryProvider

        /// <summary>
        /// 获取服务条目集合。
        /// </summary>
        /// <returns>服务条目集合。</returns>
        public IEnumerable<CoapServiceEntry> GetEntries()
        {
            var services = _types.ToArray();
            if (_coapServiceEntries == null)
            {
                _coapServiceEntries = new List<CoapServiceEntry>();
                foreach (var service in services)
                {
                    var entry = CreateServiceEntry(service);
                    if (entry != null)
                    {
                        _coapServiceEntries.Add(entry);
                    }
                }
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug($"发现了以下coap服务：{string.Join(",", _coapServiceEntries.Select(i => i.Type.FullName))}。");
                }
            }
            return _coapServiceEntries;
        } 
        #endregion

        public CoapServiceEntry CreateServiceEntry(Type service)
        {
            CoapServiceEntry result = null;
            var routeTemplate = service.GetCustomAttribute<ServiceBundleAttribute>(); 
            var objInstance = _serviceProvider.GetInstances(service);
            var behavior = objInstance as CoapBehavior;
            var path = RoutePatternParser.Parse(routeTemplate.RouteTemplate, service.Name);
            if (path.Length > 0 && path[0] != '/')
                path = $"/{path}";
            if (behavior != null)
                result = new CoapServiceEntry
                {
                    Behavior = behavior,
                    Service = service,
                    Type = behavior.GetType(),
                    Path = path
                  
                };
            return result;
        }
    }
}
