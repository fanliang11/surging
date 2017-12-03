using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Ioc
{
    public abstract class ServiceBase
    {
        public T GetService<T>()
        {
            return ServiceLocator.GetService<T>();
        }

        public T GetService<T>(string key)
        {
            return ServiceLocator.GetService<T>(key);
        }

        public object GetService(Type type)
        {
            return ServiceLocator.GetService(type);
        }

        public object GetService(string key, Type type)
        {
            return ServiceLocator.GetService(key, type);
        }
    }
}
