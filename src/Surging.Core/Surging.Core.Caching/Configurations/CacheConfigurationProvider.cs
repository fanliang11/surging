using Microsoft.Extensions.Configuration; 
using Surging.Core.CPlatform.Configurations.Remote;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Surging.Core.Caching.Configurations
{
    class CacheConfigurationProvider : FileConfigurationProvider
    {
        public CacheConfigurationProvider(CacheConfigurationSource source) : base(source) { }

        /// <summary>
        /// 重写数据转换方法
        /// </summary>
        /// <param name="stream"></param>
        public override void Load(Stream stream)
        {
            var parser = new JsonConfigurationParser();
            this.Data = parser.Parse(stream, null);
        }
    }
}