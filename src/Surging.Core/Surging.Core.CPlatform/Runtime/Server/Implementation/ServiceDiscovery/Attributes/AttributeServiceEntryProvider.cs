using Autofac;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes
{
    /// <summary>
    /// Service标记类型的服务条目提供程序。
    /// </summary>
    public class AttributeServiceEntryProvider : IServiceEntryProvider
    {
        #region 字段

        /// <summary>
        /// Defines the _clrServiceEntryFactory
        /// </summary>
        private readonly IClrServiceEntryFactory _clrServiceEntryFactory;

        /// <summary>
        /// Defines the _logger
        /// </summary>
        private readonly ILogger<AttributeServiceEntryProvider> _logger;

        /// <summary>
        /// Defines the _serviceProvider
        /// </summary>
        private readonly CPlatformContainer _serviceProvider;

        /// <summary>
        /// Defines the _types
        /// </summary>
        private readonly IEnumerable<Type> _types;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeServiceEntryProvider"/> class.
        /// </summary>
        /// <param name="types">The types<see cref="IEnumerable{Type}"/></param>
        /// <param name="clrServiceEntryFactory">The clrServiceEntryFactory<see cref="IClrServiceEntryFactory"/></param>
        /// <param name="logger">The logger<see cref="ILogger{AttributeServiceEntryProvider}"/></param>
        /// <param name="serviceProvider">The serviceProvider<see cref="CPlatformContainer"/></param>
        public AttributeServiceEntryProvider(IEnumerable<Type> types, IClrServiceEntryFactory clrServiceEntryFactory, ILogger<AttributeServiceEntryProvider> logger, CPlatformContainer serviceProvider)
        {
            _types = types;
            _clrServiceEntryFactory = clrServiceEntryFactory;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The GetALLEntries
        /// </summary>
        /// <returns>The <see cref="IEnumerable{ServiceEntry}"/></returns>
        public IEnumerable<ServiceEntry> GetALLEntries()
        {
            var services = _types.Where(i =>
            {
                var typeInfo = i.GetTypeInfo();
                return typeInfo.IsInterface && typeInfo.GetCustomAttribute<ServiceBundleAttribute>() != null;
            }).Distinct().ToArray();
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation($"发现了以下服务：{string.Join(",", services.Select(i => i.ToString()))}。");
            }
            var entries = new List<ServiceEntry>();
            foreach (var service in services)
            {
                entries.AddRange(_clrServiceEntryFactory.CreateServiceEntry(service));
            }
            return entries;
        }

        /// <summary>
        /// 获取服务条目集合。
        /// </summary>
        /// <returns>服务条目集合。</returns>
        public IEnumerable<ServiceEntry> GetEntries()
        {
            var services = GetTypes();

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation($"发现了以下服务：{string.Join(",", services.Select(i => i.ToString()))}。");
            }
            var entries = new List<ServiceEntry>();
            foreach (var service in services)
            {
                entries.AddRange(_clrServiceEntryFactory.CreateServiceEntry(service));
            }
            return entries;
        }

        /// <summary>
        /// The GetTypes
        /// </summary>
        /// <returns>The <see cref="IEnumerable{Type}"/></returns>
        public IEnumerable<Type> GetTypes()
        {
            var services = _types.Where(i =>
            {
                var typeInfo = i.GetTypeInfo();
                return typeInfo.IsInterface && typeInfo.GetCustomAttribute<ServiceBundleAttribute>() != null && _serviceProvider.Current.IsRegistered(i);
            }).Distinct().ToArray();
            return services;
        }

        #endregion 方法
    }
}