using Surging.Core.Caching.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Surging.Core.Caching.NetCache
{
    /// <summary>
    /// Defines the <see cref="GCThreadProvider" />
    /// </summary>
    public class GCThreadProvider
    {
        #region 字段

        /// <summary>
        /// 本地缓存垃圾回收线程
        /// </summary>
        private static readonly ConcurrentStack<ParameterizedThreadStart> _globalThread = new ConcurrentStack<ParameterizedThreadStart>();

        #endregion 字段

        #region 方法

        /// <summary>
        /// 添加垃圾线程方法
        /// </summary>
        /// <param name="paramThreadstart">表示在 System.Threading.Thread 上执行的方法。</param>
        /// <param name="para">  包含该线程过程的数据的对象。</param>
        public static void AddThread(ParameterizedThreadStart paramThreadstart, object para)
        {
            ParameterizedThreadStart threadstart;
            try
            {
                if (!_globalThread.TryPop(out threadstart))
                {
                    _globalThread.Push(paramThreadstart);
                    Thread thread = new Thread(paramThreadstart);
                    thread.IsBackground = true;
                    thread.Start(para ?? thread.ManagedThreadId);
                }
            }
            catch (Exception err)
            {
                throw new CacheException(err.Message, err);
            }
        }

        /// <summary>
        /// 添加垃圾线程方法
        /// </summary>
        /// <param name="start"> 表示在 System.Threading.Thread 上执行的方法。</param>
        public static void AddThread(ParameterizedThreadStart start)
        {
            AddThread(start, null);
        }

        #endregion 方法
    }
}