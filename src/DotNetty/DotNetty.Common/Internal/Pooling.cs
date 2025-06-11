// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace DotNetty.Common.Internal
{
    internal interface IPooledObjectPolicy<T>
    {
        T Create();

        T PreGetting(T obj);

        bool Return(T obj);
    }

    internal class StringBuilderPooledObjectPolicy : IPooledObjectPolicy<StringBuilder>
    {
        public int InitialCapacity { get; set; } = 100;

        public int MaximumRetainedCapacity { get; set; } = 4 * 1024;

        public StringBuilder Create() => new StringBuilder(InitialCapacity);

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public StringBuilder PreGetting(StringBuilder sb) => sb;

        public bool Return(StringBuilder sb)
        {
            if (null == sb) { return false; }
            _ = sb.Clear();
            if (sb.Capacity > MaximumRetainedCapacity)
            {
                sb.Capacity = MaximumRetainedCapacity;
            }
            return true;
        }
    }

    internal abstract class ObjectPool<T> where T : class
    {
        /// <summary></summary>
        /// <returns></returns>
        public abstract T Get();

        /// <summary></summary>
        /// <returns></returns>
        public abstract T Take();

        /// <summary></summary>
        /// <param name="obj"></param>
        public abstract void Return(T obj);

        /// <summary></summary>
        public abstract void Clear();
    }

    internal class SynchronizedObjectPool<TPoolItem> : ObjectPool<TPoolItem>
        where TPoolItem : class
    {
        private readonly SynchronizedPool<TPoolItem> _innerPool;
        private readonly IPooledObjectPolicy<TPoolItem> _policy;

        public SynchronizedObjectPool(IPooledObjectPolicy<TPoolItem> policy)
          : this(policy, int.MaxValue)
        {
        }

        public SynchronizedObjectPool(IPooledObjectPolicy<TPoolItem> policy, int maximumRetained)
        {
            _policy = policy ?? throw new ArgumentNullException(nameof(policy));
            _innerPool = new SynchronizedPool<TPoolItem>(maximumRetained);
        }

        public override TPoolItem Get()
        {
            var item = _innerPool.Take();
            if (null == item) { item = _policy.Create(); }

            return _policy.PreGetting(item);
        }

        public override TPoolItem Take()
        {
            var item = _innerPool.Take();
            if (null == item) { item = _policy.Create(); }

            return item;
        }

        public override void Return(TPoolItem item)
        {
            if (_policy.Return(item)) { _ = _innerPool.Return(item); }
        }

        public override void Clear() => _innerPool.Clear();
    }

    internal abstract class ObjectPoolProvider
    {
        public abstract ObjectPool<T> Create<T>(IPooledObjectPolicy<T> policy) where T : class;

        public abstract ObjectPool<T> Create<T>(IPooledObjectPolicy<T> policy, int maximumRetained) where T : class;
    }

    internal class SynchronizedObjectPoolProvider : ObjectPoolProvider
    {
        public static readonly SynchronizedObjectPoolProvider Default = new SynchronizedObjectPoolProvider();

        public int MaximumRetained { get; set; } = int.MaxValue;

        public override ObjectPool<T> Create<T>(IPooledObjectPolicy<T> policy)
        {
            return new SynchronizedObjectPool<T>(policy, MaximumRetained);
        }

        public override ObjectPool<T> Create<T>(IPooledObjectPolicy<T> policy, int maximumRetained)
        {
            return new SynchronizedObjectPool<T>(policy, maximumRetained);
        }
    }

    public sealed class StringBuilderManager
    {
        private static StringBuilderPooledObjectPolicy _defaultPolicy = new StringBuilderPooledObjectPolicy();
        internal static StringBuilderPooledObjectPolicy DefaultPolicy { get => _defaultPolicy; set => _defaultPolicy = value; }

        private static ObjectPool<StringBuilder> _innerPool;
        internal static ObjectPool<StringBuilder> InnerPool
        {
            [MethodImpl(InlineMethod.AggressiveInlining)]
            get
            {
                var pool = Volatile.Read(ref _innerPool);
                if (pool is null)
                {
                    pool = SynchronizedObjectPoolProvider.Default.Create(DefaultPolicy);
                    var current = Interlocked.CompareExchange(ref _innerPool, pool, null);
                    if (current != null) { return current; }
                }
                return pool;
            }
        }

        public static StringBuilder Allocate() => InnerPool.Take();
        internal static StringBuilder Take() => InnerPool.Take();

        public static StringBuilder Allocate(int capacity)
        {
            if ((uint)(capacity - 1) > SharedConstants.TooBigOrNegative) // <= 0
            {
                capacity = _defaultPolicy.InitialCapacity;
            }

            var sb = InnerPool.Take();
            if (sb.Capacity < capacity) { sb.Capacity = capacity; }

            return sb;
        }

        public static string ReturnAndFree(StringBuilder sb)
        {
            var ret = sb.ToString();
            InnerPool.Return(sb);
            return ret;
        }

        public static void Free(StringBuilder sb) => InnerPool.Return(sb);

        public static void Clear() => InnerPool.Clear();
    }
}
