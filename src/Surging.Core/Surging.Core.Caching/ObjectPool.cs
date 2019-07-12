using Surging.Core.Caching.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Surging.Core.Caching
{
    /// <summary>
    /// Defines the <see cref="ObjectPool{T}" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectPool<T>
    {
        #region 字段

        /// <summary>
        /// Defines the maxSize
        /// </summary>
        private readonly int maxSize = 50;

        /// <summary>
        /// Defines the minSize
        /// </summary>
        private readonly int minSize = 1;

        /// <summary>
        /// Defines the currentResource
        /// </summary>
        private int currentResource = 0;

        /// <summary>
        /// Defines the func
        /// </summary>
        private Func<T> func = null;

        /// <summary>
        /// Defines the isTaked
        /// </summary>
        private int isTaked = 0;

        /// <summary>
        /// Defines the queue
        /// </summary>
        private Queue<T> queue = new Queue<T>();

        /// <summary>
        /// Defines the tryNewObject
        /// </summary>
        private int tryNewObject = 0;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectPool{T}"/> class.
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

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// 从对象池中取一个对象出来, 执行完成以后会自动将对象放回池中
        /// </summary>
        /// <returns>The <see cref="T"/></returns>
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

        /// <summary>
        /// The Enter
        /// </summary>
        private void Enter()
        {
            while (Interlocked.Exchange(ref isTaked, 1) != 0)
            {
            }
        }

        /// <summary>
        /// The Leave
        /// </summary>
        private void Leave()
        {
            Interlocked.Exchange(ref isTaked, 0);
        }

        #endregion 方法
    }
}