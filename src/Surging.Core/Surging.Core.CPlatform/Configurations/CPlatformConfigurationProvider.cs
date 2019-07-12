using Microsoft.Extensions.Configuration;
using Surging.Core.CPlatform.Configurations.Remote;
using System.IO;

namespace Surging.Core.CPlatform.Configurations
{
    /// <summary>
    /// Defines the <see cref="CPlatformConfigurationProvider" />
    /// </summary>
    public class CPlatformConfigurationProvider : FileConfigurationProvider
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="CPlatformConfigurationProvider"/> class.
        /// </summary>
        /// <param name="source">The source<see cref="CPlatformConfigurationSource"/></param>
        public CPlatformConfigurationProvider(CPlatformConfigurationSource source) : base(source)
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