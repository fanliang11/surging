using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Surging.Core.EventBusKafka.Utilities
{
   public static class ExtensionsToFastActivator
    {
        public static void FastInvoke<T>(this T target, Type[] genericTypes, Expression<Action<T>> expression)
        {
            FastInvoker<T>.Current.FastInvoke(target, genericTypes, expression);
        }
    }
}
