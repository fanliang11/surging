using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Ioc;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Surging.Core.Grpc.Runtime.Implementation
{
    public class DefaultGrpcServiceEntryProvider: IGrpcServiceEntryProvider
    { 
        #region Field

        private readonly IEnumerable<Type> _types;
        private readonly ILogger<DefaultGrpcServiceEntryProvider> _logger;
        private readonly CPlatformContainer _serviceProvider;
        private List<GrpcServiceEntry> _grpcServiceEntries;

        #endregion Field

        #region Constructor

        public DefaultGrpcServiceEntryProvider(IServiceEntryProvider serviceEntryProvider,
            ILogger<DefaultGrpcServiceEntryProvider> logger,
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
        public List<GrpcServiceEntry> GetEntries()
        {
            var services = _types.ToArray();
            if (_grpcServiceEntries == null)
            {
                _grpcServiceEntries = new List<GrpcServiceEntry>(); 
                foreach (var service in services)
                {
                    var entry = CreateServiceEntry(service);
                    if (entry != null)
                    {
                        _grpcServiceEntries.Add(entry); 
                    }
                }
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug($"发现了以下grpc服务：{string.Join(",", _grpcServiceEntries.Select(i => i.Type.FullName))}。"); ;
                }
            }
            return _grpcServiceEntries;
        }

        public GrpcServiceEntry CreateServiceEntry(Type service)
        {
            GrpcServiceEntry result = null; 
            var objInstance = _serviceProvider.GetInstances(service);
            var behavior = objInstance as IServiceBehavior;  
            if (behavior != null)
                result = new GrpcServiceEntry
                {
                    Behavior = behavior,
                    Type = behavior.GetType()
                };
            return result;
        }
        #endregion
    }
}
