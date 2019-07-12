using Surging.Core.CPlatform.Configurations.Watch;

namespace Surging.Core.CPlatform.Configurations
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IConfigurationWatchManager" />
    /// </summary>
    public interface IConfigurationWatchManager
    {
        #region 方法

        /// <summary>
        /// The Register
        /// </summary>
        /// <param name="watch">The watch<see cref="ConfigurationWatch"/></param>
        void Register(ConfigurationWatch watch);

        #endregion 方法
    }

    #endregion 接口
}