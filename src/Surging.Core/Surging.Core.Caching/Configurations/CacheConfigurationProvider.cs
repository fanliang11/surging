using Microsoft.Extensions.Configuration;
using Surging.Core.CPlatform.Configurations.Remote;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Surging.Core.Caching.Configurations
{
    /// <summary>
    /// Defines the <see cref="CacheConfigurationProvider" />
    /// </summary>
    internal class CacheConfigurationProvider : FileConfigurationProvider
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheConfigurationProvider"/> class.
        /// </summary>
        /// <param name="source">The source<see cref="CacheConfigurationSource"/></param>
        public CacheConfigurationProvider(CacheConfigurationSource source) : base(source)
        {
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// 重写数据转换方法
        /// </summary>
        /// <param name="stream"></param>
        public override void Load(Stream stream)
        {
            var parser = new JsonConfigurationParser();
            this.Data = parser.Parse(stream, null);
        }

        #endregion 方法
    }
}