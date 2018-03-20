using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Caching
{
   public class CacheContainer
    {
        public static T GetInstances<T>(string name) where T : class
        {
            var appConfig = AppConfig.DefaultInstance;
            return appConfig.GetContextInstance<T>(name);
        }

        public static T GetInstances<T>() where T : class
        {
            var appConfig = AppConfig.DefaultInstance;
            return appConfig.GetContextInstance<T>();
        }

        public static T GetService<T>(string name)
        {
            if (ServiceLocator.Current == null) return default(T);
            return ServiceLocator.GetService<T>(name);
        }

        public static T GetService<T>()
        {
            if (ServiceLocator.Current == null) return default(T);
            return ServiceLocator.GetService<T>();
        }

        public static bool IsRegistered<T>()
        {
            return ServiceLocator.IsRegistered<T>();
        }

        public static bool IsRegistered<T>(string key)
        {
            return ServiceLocator.IsRegistered<T>(key);
        }

        public static bool IsRegistered(Type type)
        {
            return ServiceLocator.IsRegistered(type);
        }

        public static bool IsRegisteredWithKey(string key, Type type)
        {
            return ServiceLocator.IsRegisteredWithKey(key, type);
        }
    }
}

