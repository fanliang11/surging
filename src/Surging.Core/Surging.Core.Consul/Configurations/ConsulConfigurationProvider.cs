using Microsoft.Extensions.Configuration;
using Surging.Core.CPlatform.Configurations.Remote;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Surging.Core.Consul.Configurations
{
    /// <summary>
    /// Defines the <see cref="ConsulConfigurationProvider" />
    /// </summary>
    public class ConsulConfigurationProvider : FileConfigurationProvider
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsulConfigurationProvider"/> class.
        /// </summary>
        /// <param name="source">The source<see cref="ConsulConfigurationSource"/></param>
        public ConsulConfigurationProvider(ConsulConfigurationSource source) : base(source)
        {
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The Load
        /// </summary>
        /// <param name="stream">The stream<see cref="Stream"/></param>
        public override void Load(Stream stream)
        {
            var parser = new JsonConfigurationParser();
            this.Data = parser.Parse(stream, null);
        }

        #endregion 方法
    }
}