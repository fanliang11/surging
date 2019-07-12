using Microsoft.Extensions.Configuration;
using Surging.Core.CPlatform.Configurations.Remote;
using System.IO;

namespace Surging.Core.Zookeeper.Configurations
{
    /// <summary>
    /// Defines the <see cref="ZookeeperConfigurationProvider" />
    /// </summary>
    public class ZookeeperConfigurationProvider : FileConfigurationProvider
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ZookeeperConfigurationProvider"/> class.
        /// </summary>
        /// <param name="source">The source<see cref="ZookeeperConfigurationSource"/></param>
        public ZookeeperConfigurationProvider(ZookeeperConfigurationSource source) : base(source)
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