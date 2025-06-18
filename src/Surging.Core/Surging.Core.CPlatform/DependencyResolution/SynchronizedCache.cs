using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.DependencyResolution
{
    public class SynchronizedCache<TKey,TValue>
    {
        private ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim();
        private Dictionary<TKey, TValue> innerCache = new Dictionary<TKey, TValue>(300);

        public TValue Read(TKey key)
        {
            cacheLock.EnterReadLock();
            try
            {
                if (innerCache.ContainsKey(key))
                return innerCache[key];
                else
                    return default(TValue);
            }
            finally
            {
                cacheLock.ExitReadLock();
            }
        }

        public void Add(TKey key, TValue value)
        {
            cacheLock.EnterWriteLock();
            try
            {
                innerCache[key] = value;
            }
            finally
            {
                cacheLock.ExitWriteLock();
            }
        }
    }
}
