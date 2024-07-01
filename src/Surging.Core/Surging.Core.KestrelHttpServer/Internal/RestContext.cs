using Microsoft.AspNetCore.Http; 
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Surging.Core.CPlatform.Utilities;
using System.Linq;

namespace Surging.Core.KestrelHttpServer.Internal
{
   public class RestContext
    {
     
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAttachment(string key, object value)
        {
            Check.NotNull(serviceProvider, "serviceProvider");
            var htpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
            htpContextAccessor.HttpContext.Items.Add(key,value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object GetAttachment(string key)
        {
            Check.NotNull(serviceProvider, "serviceProvider");
            var htpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
            htpContextAccessor.HttpContext.Items.TryGetValue(key, out object value);
            return value;
        }

        public  void RemoveContextParameters(string key)
        {
            Check.NotNull(serviceProvider, "serviceProvider");
            var htpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
            if (htpContextAccessor.HttpContext.Items.ContainsKey(key))
                htpContextAccessor.HttpContext.Items.Remove(key);

        }

        public Dictionary<String, Object> GetContextParameters()
        {
            Check.NotNull(serviceProvider, "serviceProvider");
            var htpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
            return htpContextAccessor.HttpContext.Items.ToDictionary(p => p.Key.ToString(), m => m.Value);
            
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetContextParameters(IDictionary<object, object> contextParameters)
        {
            Check.NotNull(serviceProvider, "serviceProvider");
            var htpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
            htpContextAccessor.HttpContext.Items= contextParameters;
        }

        private static IServiceProvider serviceProvider;

        internal void Initialize(IServiceProvider provider)
        {
            serviceProvider= provider;
        }

        public static RestContext GetContext()
        {

            return new RestContext();
        }

        public static void RemoveContext()
        {
            Check.NotNull(serviceProvider, "serviceProvider");
            var htpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
            htpContextAccessor.HttpContext.Items.Clear();
            
        }

   
    }
}
