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

namespace Surging.Core.Protocol.Tcp.Runtime.Implementation
{
    public class DefaultTcpServiceEntryProvider : ITcpServiceEntryProvider
    {
        #region Field

        private readonly IEnumerable<Type> _types;
        private readonly ILogger<DefaultTcpServiceEntryProvider> _logger;
        private readonly CPlatformContainer _serviceProvider;
        private Type? _tcpServiceEntryType=null;

        #endregion Field

        #region Constructor

        public DefaultTcpServiceEntryProvider(IServiceEntryProvider serviceEntryProvider,
            ILogger<DefaultTcpServiceEntryProvider> logger,
            CPlatformContainer serviceProvider)
        {
            _types = serviceEntryProvider.GetTypes();
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        #endregion Constructor

        #region Implementation of ITcpServiceEntryProvider

        /// <summary>
        /// 获取服务条目集合。
        /// </summary>
        /// <returns>服务条目集合。</returns>
        public TcpServiceEntry GetEntry()
        {
            var services = _types.ToArray();
            TcpServiceEntry result = new TcpServiceEntry();
            if(_tcpServiceEntryType !=null)
                result = CreateServiceEntry(_tcpServiceEntryType);
            else
            { 
                foreach (var service in services)
                {
                    var entry = CreateServiceEntry(service);
                    if (entry != null)
                    {
                        _tcpServiceEntryType = service;
                        result = entry;
                        break;
                    }
                }
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug($"发现了以下Tcp服务：{result.Type.FullName}。");
                }
            }
            return result;
        }

        public TcpServiceEntry CreateServiceEntry(Type service)
        {
            TcpServiceEntry result = null;
            var routeTemplate = service.GetCustomAttribute<ServiceBundleAttribute>();
            var objInstance = _serviceProvider.GetInstances(service);
            var behavior = objInstance as TcpBehavior;
            var path = RoutePatternParser.Parse(routeTemplate.RouteTemplate, service.Name);
            if (path.Length > 0 && path[0] != '/')
                path = $"/{path}";
            if (behavior != null)
                result = new TcpServiceEntry
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
