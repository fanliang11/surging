using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Surging.Core.EventBusRabbitMQ.Utilities
{
    /// <summary>
    /// Defines the <see cref="ExtensionsToFastActivator" />
    /// </summary>
    public static class ExtensionsToFastActivator
    {
        #region 方法

        /// <summary>
        /// The FastInvoke
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">The target<see cref="T"/></param>
        /// <param name="genericTypes">The genericTypes<see cref="Type[]"/></param>
        /// <param name="expression">The expression<see cref="Expression{Action{T}}"/></param>
        public static void FastInvoke<T>(this T target, Type[] genericTypes, Expression<Action<T>> expression)
        {
            FastInvoker<T>.Current.FastInvoke(target, genericTypes, expression);
        }

        #endregion 方法
    }
}