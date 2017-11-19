using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Surging.Core.Caching.Configurations.Remote
{
    /// <summary>
    ///  对于配置进行解析
    /// </summary>
    public interface IConfigurationParser
    {
        /// <summary>
        /// 对于配置信息解析成键值对集合
        /// </summary>
        /// <param name="input">序列化的字节流</param>
        /// <param name="initialContext">配置信息KEY前缀</param>
        /// <returns>返回键值对泛型集合</returns>
        IDictionary<string, string> Parse(Stream input, string initialContext);
    }
}
