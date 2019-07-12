using Microsoft.Extensions.Logging;
using Surging.Core.ProxyGenerator.Interceptors.Implementation;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Surging.Core.ProxyGenerator.Interceptors
{
    /// <summary>
    /// Defines the <see cref="InvocationMethods" />
    /// </summary>
    public class InvocationMethods
    {
        #region 字段

        /// <summary>
        /// Defines the CompositionInvocationConstructor
        /// </summary>
        public static readonly ConstructorInfo CompositionInvocationConstructor =
        typeof(ActionInvocation).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null,
                                                     new[]
                                                     {
                                                             typeof(IDictionary<string, object>),
                                                             typeof(string),
                                                             typeof(string[]),
                                                             typeof(List<Attribute>),
                                                             typeof(Type),
                                                             typeof(object)
                                                     },
                                                     null);

        #endregion 字段
    }
}