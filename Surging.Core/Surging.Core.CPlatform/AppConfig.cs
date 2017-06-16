using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Surging.Core.CPlatform.DependencyResolution;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Surging.Core.CPlatform
{
   public class AppConfig
    {
        public AppConfig()
        {
            RegisterLocalInstance();
        }

        internal static IConfigurationRoot Configuration { get; set; }

        internal static AppConfig DefaultInstance
        {
            get
            {
                ServiceResolver.Current.Register(null, Activator.CreateInstance(typeof(AppConfig), new object[] { }));
                return ServiceResolver.Current.GetService<AppConfig>();
            }
        }

        public T GetContextInstance<T>() where T : class
        {
            var context = ServiceResolver.Current.GetService<T>(typeof(T));
            return context;
        }

        public T GetContextInstance<T>(string name) where T : class
        {
            DebugCheck.NotEmpty(name);
            var context = ServiceResolver.Current.GetService<T>(name);
            return context;
        }

        private void RegisterLocalInstance()
        {
            var types = this.GetType().GetTypeInfo().Assembly.GetTypes().Where(p => p.GetTypeInfo().IsClass==true);
            foreach (var t in types)
            {
                ServiceResolver.Current.Register(null, Activator.CreateInstance(t, new object[] { }));
            }
        }

        private void RegisterLocalInstance(string typeName)
        {
            var types = this.GetType().GetTypeInfo().Assembly.GetTypes().Where(p => p.GetTypeInfo().GetInterface(typeName) != null);
            foreach (var t in types)
            {
                var attribute = t.GetTypeInfo().GetCustomAttribute<IdentifyAttribute>();
                ServiceResolver.Current.Register(attribute.Name.ToString(),
                    Activator.CreateInstance(t));
            }
        }
    }
}
