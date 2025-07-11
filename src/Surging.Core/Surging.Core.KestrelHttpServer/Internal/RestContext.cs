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
            var htpContextAccessor = ServiceLocator.GetService<IHttpContextAccessor>();
            htpContextAccessor.HttpContext.Items.Add(key,value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object GetAttachment(string key)
        { 
            var htpContextAccessor = ServiceLocator.GetService<IHttpContextAccessor>();
            if (htpContextAccessor.HttpContext != null)
            {
                htpContextAccessor.HttpContext.Items.TryGetValue(key, out object value);
                return value;
            }
            return null;
        }

        public  void RemoveContextParameters(string key)
        { 
            var htpContextAccessor = ServiceLocator.GetService<IHttpContextAccessor>();
            if (htpContextAccessor.HttpContext.Items.ContainsKey(key))
                htpContextAccessor.HttpContext.Items.Remove(key);

        }

        public Dictionary<String, Object> GetContextParameters()
        { 
            var htpContextAccessor = ServiceLocator.GetService<IHttpContextAccessor>();
            return htpContextAccessor.HttpContext.Items.ToDictionary(p => p.Key.ToString(), m => m.Value);
            
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetContextParameters(IDictionary<object, object> contextParameters)
        { 
            var htpContextAccessor = ServiceLocator.GetService<IHttpContextAccessor>();
            htpContextAccessor.HttpContext.Items= contextParameters;
        }
         

        internal void Initialize(IServiceProvider provider)
        { 
        }

        public static RestContext GetContext()
        {
            return new RestContext();
        }

        public static void RemoveContext()
        { 
            var htpContextAccessor = ServiceLocator.GetService<IHttpContextAccessor>();
            htpContextAccessor.HttpContext.Items.Clear();
            htpContextAccessor.HttpContext = null;
            htpContextAccessor = null;
        } 
    }
}
