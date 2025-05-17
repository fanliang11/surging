namespace DotNetty.Common.Internal
{
    using System;

    public interface IBlockingQueue<T> : IQueue<T>
    {
        T Take();

        bool TryTake(out T item);

        bool TryTake(out T item, TimeSpan timeout);

        bool TryTake(out T item, int millisecondsTimeout);
    }
}