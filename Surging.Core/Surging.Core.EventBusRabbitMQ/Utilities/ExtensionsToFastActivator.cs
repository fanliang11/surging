using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Surging.Core.EventBusRabbitMQ.Utilities
{
    public static class ExtensionsToFastActivator
    {
        public static void FastInvoke<T>(this T target, Type[] genericTypes, Expression<Action<T>> expression)
        {
            FastInvoker<T>.Current.FastInvoke(target, genericTypes,expression);
        }

    }
}
