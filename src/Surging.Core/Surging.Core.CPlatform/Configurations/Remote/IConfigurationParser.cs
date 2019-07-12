using System.Collections.Generic;
using System.IO;

namespace Surging.Core.CPlatform.Configurations.Remote
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IConfigurationParser" />
    /// </summary>
    public interface IConfigurationParser
    {
        #region 方法

        /// <summary>
        /// The Parse
        /// </summary>
        /// <param name="input">The input<see cref="Stream"/></param>
        /// <param name="initialContext">The initialContext<see cref="string"/></param>
        /// <returns>The <see cref="IDictionary{string, string}"/></returns>
        IDictionary<string, string> Parse(Stream input, string initialContext);

        #endregion 方法
    }

    #endregion 接口
}