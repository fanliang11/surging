namespace DotNetty.Common.Internal
{
    using System.Collections.Concurrent;
    using System.Runtime.CompilerServices;

    public class CompatibleBlockingQueue<T> : BlockingCollection<T>, IBlockingQueue<T>
    {
        private readonly ConcurrentQueue<T> _innerQueue;

        public CompatibleBlockingQueue()
            : this(new ConcurrentQueue<T>())
        {
        }

        public CompatibleBlockingQueue(int boundedCapacity)
            : this(new ConcurrentQueue<T>(), boundedCapacity)
        {
        }

        public CompatibleBlockingQueue(ConcurrentQueue<T> queue)
            : base(queue)
        {
            _innerQueue = queue;
        }

        public CompatibleBlockingQueue(ConcurrentQueue<T> queue, int boundedCapacity)
            : base(queue, boundedCapacity)
        {
            _innerQueue = queue;
        }

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public bool TryPeek(out T item) => _innerQueue.TryPeek(out item);

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public bool TryDequeue(out T item) => TryTake(out item, 0);

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public bool TryEnqueue(T element) => TryAdd(element);

        void IQueue<T>.Clear()
        {
            while (TryDequeue(out _)) { }
        }

        public bool NonEmpty => (uint)Count > 0u;

        public bool IsEmpty => 0u >= (uint)Count;
    }
}