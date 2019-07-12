using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Surging.Core.CPlatform.Runtime.Server.Implementation
{
    /// <summary>
    /// 默认的服务条目管理者。
    /// </summary>
    public class DefaultServiceEntryManager : IServiceEntryManager
    {
        #region 字段

        /// <summary>
        /// Defines the _allEntries
        /// </summary>
        private IEnumerable<ServiceEntry> _allEntries;

        /// <summary>
        /// Defines the _serviceEntries
        /// </summary>
        private IEnumerable<ServiceEntry> _serviceEntries;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultServiceEntryManager"/> class.
        /// </summary>
        /// <param name="providers">The providers<see cref="IEnumerable{IServiceEntryProvider}"/></param>
        public DefaultServiceEntryManager(IEnumerable<IServiceEntryProvider> providers)
        {
            UpdateEntries(providers);
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The GetAllEntries
        /// </summary>
        /// <returns>The <see cref="IEnumerable{ServiceEntry}"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<ServiceEntry> GetAllEntries()
        {
            return _allEntries;
        }

        /// <summary>
        /// 获取服务条目集合。
        /// </summary>
        /// <returns>服务条目集合。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<ServiceEntry> GetEntries()
        {
            return _serviceEntries;
        }

        /// <summary>
        /// The UpdateEntries
        /// </summary>
        /// <param name="providers">The providers<see cref="IEnumerable{IServiceEntryProvider}"/></param>
        public void UpdateEntries(IEnumerable<IServiceEntryProvider> providers)
        {
            var list = new List<ServiceEntry>();
            var allEntries = new List<ServiceEntry>();
            foreach (var provider in providers)
            {
                var entries = provider.GetEntries().ToArray();
                foreach (var entry in entries)
                {
                    if (list.Any(i => i.Descriptor.Id == entry.Descriptor.Id))
                        throw new InvalidOperationException($"本地包含多个Id为：{entry.Descriptor.Id} 的服务条目。");
                }
                list.AddRange(entries);
                allEntries.AddRange(provider.GetALLEntries());
            }
            _serviceEntries = list.ToArray();
            _allEntries = allEntries;
        }

        #endregion 方法
    }
}