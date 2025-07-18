using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DotNetty.Common.Internal
{
    internal static class CollectionHelpers
    {
        internal static IReadOnlyCollection<T> ReifyCollection<T>(IEnumerable<T> source)
        {
            if (source is IReadOnlyCollection<T> result) { return result; }
            if (source is ICollection<T> collection) { return new CollectionWrapper<T>(collection); }
            if (source is ICollection nongenericCollection) { return new NongenericCollectionWrapper<T>(nongenericCollection); }

            var builder = new LargeArrayBuilder<T>(initialize: true);
            builder.AddRange(source);
            return builder.ToArray();
        }

        private sealed class NongenericCollectionWrapper<T> : IReadOnlyCollection<T>
        {
            private readonly ICollection _collection;

            internal NongenericCollectionWrapper(ICollection collection)
            {
                if (collection == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.collection);
                _collection = collection;
            }

            public int Count
            {
                get { return _collection.Count; }
            }

            public IEnumerator<T> GetEnumerator()
            {
                foreach (T item in _collection)
                {
                    yield return item;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _collection.GetEnumerator();
            }
        }

        private sealed class CollectionWrapper<T> : IReadOnlyCollection<T>
        {
            private readonly ICollection<T> _collection;

            internal CollectionWrapper(ICollection<T> collection)
            {
                if (collection == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.collection);
                _collection = collection;
            }

            public int Count
            {
                get { return _collection.Count; }
            }

            public IEnumerator<T> GetEnumerator()
            {
                return _collection.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _collection.GetEnumerator();
            }
        }
    }
}