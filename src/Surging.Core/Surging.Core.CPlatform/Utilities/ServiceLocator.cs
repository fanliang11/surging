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

        public static bool IsRegistered<T>()
        {
            return Current.IsRegistered<T>();
        }

        public static bool IsRegistered<T>(string key)
        {
            return Current.IsRegisteredWithKey<T>(key);
        }

        public static bool IsRegistered(Type type)
        {
            return Current.IsRegistered(type);
        }

        public static bool IsRegisteredWithKey(string key, Type type)
        {
            return Current.IsRegisteredWithKey(key, type);
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