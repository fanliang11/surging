using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Surging.Core.CPlatform.Transport.Implementation
{
    /// <summary>
    /// Defines the <see cref="RpcContext" />
    /// </summary>
    public class RpcContext
    {
        #region 字段

        /// <summary>
        /// Defines the rpcContextThreadLocal
        /// </summary>
        private static ThreadLocal<RpcContext> rpcContextThreadLocal = new ThreadLocal<RpcContext>(() =>
        {
            RpcContext context = new RpcContext();
            context.SetContextParameters(new ConcurrentDictionary<string, object>());
            return context;
        });

        /// <summary>
        /// Defines the contextParameters
        /// </summary>
        private ConcurrentDictionary<String, Object> contextParameters;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Prevents a default instance of the <see cref="RpcContext"/> class from being created.
        /// </summary>
        private RpcContext()
        {
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The GetContext
        /// </summary>
        /// <returns>The <see cref="RpcContext"/></returns>
        public static RpcContext GetContext()
        {
            return rpcContextThreadLocal.Value;
        }

        /// <summary>
        /// The RemoveContext
        /// </summary>
        public static void RemoveContext()
        {
            rpcContextThreadLocal.Dispose();
        }

        /// <summary>
        /// The GetAttachment
        /// </summary>
        /// <param name="key">The key<see cref="string"/></param>
        /// <returns>The <see cref="object"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object GetAttachment(string key)
        {
            contextParameters.TryGetValue(key, out object result);
            return result;
        }

        /// <summary>
        /// The GetContextParameters
        /// </summary>
        /// <returns>The <see cref="ConcurrentDictionary{String, Object}"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ConcurrentDictionary<String, Object> GetContextParameters()
        {
            return contextParameters;
        }

        /// <summary>
        /// The SetAttachment
        /// </summary>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="value">The value<see cref="object"/></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAttachment(string key, object value)
        {
            contextParameters.AddOrUpdate(key, value, (k, v) => value);
        }

        /// <summary>
        /// The SetContextParameters
        /// </summary>
        /// <param name="contextParameters">The contextParameters<see cref="ConcurrentDictionary{String, Object}"/></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetContextParameters(ConcurrentDictionary<String, Object> contextParameters)
        {
            this.contextParameters = contextParameters;
        }

        #endregion 方法
    }
}