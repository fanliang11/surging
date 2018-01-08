using Autofac;
using System;

namespace Surging.Core.CPlatform.Utilities
{
    public class ServiceLocator
    {
        public static IContainer Current { get; set; }

        public static T GetService<T>()
        {
            return Current.Resolve<T>();
        }

        public static T GetService<T>(string key)
        {
            return Current.ResolveKeyed<T>(key);
        }

        public static object GetService(Type type)
        {
            return Current.Resolve(type);
        }

        public static object GetService(string key, Type type)
        {
            return Current.ResolveKeyed(key, type);
        }
    }
}