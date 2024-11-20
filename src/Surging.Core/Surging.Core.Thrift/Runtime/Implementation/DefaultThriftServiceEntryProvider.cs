using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Ioc;
using Surging.Core.CPlatform.Runtime.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Surging.Core.Thrift.Runtime.Implementation
{
   public  class DefaultThriftServiceEntryProvider : IThriftServiceEntryProvider
    {
        #region Field

        private readonly IEnumerable<Type> _types;
        private readonly ILogger<DefaultThriftServiceEntryProvider> _logger;
        private readonly CPlatformContainer _serviceProvider;
        private List<ThriftServiceEntry> _thriftServiceEntries;

        #endregion Field

        #region Constructor

        public DefaultThriftServiceEntryProvider(IServiceEntryProvider serviceEntryProvider,
            ILogger<DefaultThriftServiceEntryProvider> logger,
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
        public List<ThriftServiceEntry> GetEntries()
        {
            var services = _types.ToArray();
            if (_thriftServiceEntries == null)
            {
                _thriftServiceEntries = new List<ThriftServiceEntry>();
                foreach (var service in services)
                {
                    var entry = CreateServiceEntry(service);
                    if (entry != null)
                    {
                        _thriftServiceEntries.Add(entry);
                    }
                }
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug($"发现了以下thrift服务：{string.Join(",", _thriftServiceEntries.Select(i => i.Type.FullName))}。"); ;
                }
            }
            return _thriftServiceEntries;
        }

        public ThriftServiceEntry CreateServiceEntry(Type service)
        {
            ThriftServiceEntry result = null;
            var objInstance = _serviceProvider.GetInstances(service);
            var behavior = objInstance as IServiceBehavior;
            if (behavior != null)
                result = new ThriftServiceEntry
                {
                    Behavior = behavior,
                    Type = behavior.GetType()
                };
            return result;
        }
        #endregion
    }
}
