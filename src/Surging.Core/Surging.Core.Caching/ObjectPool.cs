using Surging.Core.Caching.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Surging.Core.Caching
{
    public class ObjectPool<T>
    {
        #region 
        private int isTaked = 0;
        private Queue<T> queue = new Queue<T>();
        private Func<T> func = null;
        private int currentResource = 0;
        private int tryNewObject = 0;
        private readonly int minSize = 1;
        private readonly int maxSize = 50;
        #endregion

        #region private methods
        private void Enter()
        {
            while (Interlocked.Exchange(ref isTaked, 1) != 0)
            {
            }
        }
        private void Leave()
        {
            Interlocked.Exchange(ref isTaked, 0);
        }
        #endregion

        /// <summary>
        /// 构造一个对象池
        /// </summary>
        /// <param name="func">用来初始化对象的函数</param>
        /// <param name="minSize">对象池下限</param>
        /// <param name="maxSize">对象池上限</param>
        public ObjectPool(Func<T> func, int minSize = 100, int maxSize = 100)
        {
            Check.CheckCondition(() => func == null, "func");
            Check.CheckCondition(() => minSize < 0, "minSize");
            Check.CheckCondition(() => maxSize < 0, "maxSize");
            if (minSize > 0)
                this.minSize = minSize;
            if (maxSize > 0)
                this.maxSize = maxSize;
            for (var i = 0; i < this.minSize; i++)
            {
                this.queue.Enqueue(func());
            }
            this.currentResource = this.minSize;
            this.tryNewObject = this.minSize;
            this.func = func;
        }

        /// <summary>
        /// 从对象池中取一个对象出来, 执行完成以后会自动将对象放回池中
        /// </summary>
        public T GetObject()
        {
            var t = default(T);
            try
            {
                if (this.tryNewObject < this.maxSize)
                {
                    Interlocked.Increment(ref this.tryNewObject);
                    t = func();
                    // Interlocked.Increment(ref this.currentResource);
                }
                else
                {
                    this.Enter();
                    t = this.queue.Dequeue();
                    this.Leave();
                    Interlocked.Decrement(ref this.currentResource);
                }
                return t;
            }
            finally
            {
                this.Enter();
                this.queue.Enqueue(t);
                this.Leave();
                Interlocked.Increment(ref currentResource);
            }
        }

    }
}
