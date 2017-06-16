using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace Surging.Core.ProxyGenerator
{
    /// <summary>
    /// 一个抽象的服务代理生成器。
    /// </summary>
    public interface IServiceProxyGenerater
    {
        /// <summary>
        /// 生成服务代理。
        /// </summary>
        /// <param name="interfacTypes">需要被代理的接口类型。</param>
        /// <returns>服务代理实现。</returns>
        IEnumerable<Type> GenerateProxys(IEnumerable<Type> interfacTypes);

        /// <summary>
        /// 生成服务代理代码树。
        /// </summary>
        /// <param name="interfaceType">需要被代理的接口类型。</param>
        /// <returns>代码树。</returns>
        SyntaxTree GenerateProxyTree(Type interfaceType);
    }
}