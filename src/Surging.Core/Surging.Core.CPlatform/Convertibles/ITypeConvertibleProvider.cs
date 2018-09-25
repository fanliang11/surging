using System;
using System.Collections.Generic;

namespace Surging.Core.CPlatform.Convertibles
{
    /// <summary>
    /// 类型转换。
    /// </summary>
    /// <param name="instance">需要转换的实例。</param>
    /// <param name="conversionType">转换的类型。</param>
    /// <returns>转换之后的类型，如果无法转换则返回null。</returns>
    public delegate object TypeConvertDelegate(object instance, Type conversionType);

    /// <summary>
    /// 一个抽象的类型转换提供程序。
    /// </summary>
    public interface ITypeConvertibleProvider
    {
        /// <summary>
        /// 获取类型转换器。
        /// </summary>
        /// <returns>类型转换器集合。</returns>
        IEnumerable<TypeConvertDelegate> GetConverters();
    }
}