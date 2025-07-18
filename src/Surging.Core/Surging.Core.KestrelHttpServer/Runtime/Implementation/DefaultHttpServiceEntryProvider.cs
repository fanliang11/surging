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

namespace Surging.Core.KestrelHttpServer.Runtime.Implementation
{
    internal class DefaultHttpServiceEntryProvider: IHttpServiceEntryProvider
    {

        #region Field

        private readonly IEnumerable<Type> _types;
        private readonly ILogger<DefaultHttpServiceEntryProvider> _logger;
        private readonly CPlatformContainer _serviceProvider;
        private Type? _httpServiceEntryType = null;
        #endregion Field

        #region Constructor

        public DefaultHttpServiceEntryProvider(IServiceEntryProvider serviceEntryProvider,
            ILogger<DefaultHttpServiceEntryProvider> logger,
            CPlatformContainer serviceProvider)
        {
            _types = serviceEntryProvider.GetTypes();
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        #endregion Constructor

        #region Implementation of IServiceEntryProvider


        public HttpServiceEntry GetEntry()
        {
            var services = _types.ToArray();
            HttpServiceEntry result = new HttpServiceEntry();
            if (_httpServiceEntryType != null)
                result = CreateServiceEntry(_httpServiceEntryType);
            else
            {
                foreach (var service in services)
                {
                    var entry = CreateServiceEntry(service);
                    if (entry != null)
                    {
                        _httpServiceEntryType = service;
                        result = entry;
                        break;
                    }
                }
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug($"发现了以下Http服务：{result.Type.FullName}。");
                }
            }
            return result;
        }

        public HttpServiceEntry CreateServiceEntry(Type service)
        {
            HttpServiceEntry result = null;
            var routeTemplate = service.GetCustomAttribute<ServiceBundleAttribute>();
            var objInstance = _serviceProvider.GetInstances(service);
            var behavior = objInstance as HttpBehavior;
            var path = RoutePatternParser.Parse(routeTemplate.RouteTemplate, service.Name);
            if (path.Length > 0 && path[0] != '/')
                path = $"/{path}";
            if (behavior != null)
#pragma warning disable CS8603 // 可能返回 null 引用。
                result = new HttpServiceEntry
                {
                    Behavior = () => _serviceProvider.GetInstances(service) as HttpBehavior,
                    Type = behavior.GetType(),
                    Path = path,
                };

            return result;
#pragma warning restore CS8603 // 可能返回 null 引用。
        }
        #endregion

    }
}
