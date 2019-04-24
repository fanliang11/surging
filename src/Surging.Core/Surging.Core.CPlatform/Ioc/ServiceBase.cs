using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Ioc
{
    public abstract class ServiceBase: IServiceBehavior
    {
        public virtual T GetService<T>() where T : class
        {
            return ServiceLocator.GetService<T>();
        }

        public virtual T GetService<T>(string key) where T : class
        {
            return ServiceLocator.GetService<T>(key);
        }

        public virtual object GetService(Type type)
        {
            return ServiceLocator.GetService(type);
        }

        public virtual object GetService(string key, Type type)
        {
            return ServiceLocator.GetService(key, type);
        }
    }
}
