using Microsoft.Extensions.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.KestrelHttpServer.Internal
{
    public class HttpFormCollection : IEnumerable<KeyValuePair<string, StringValues>>, IEnumerable
    {
        public static readonly HttpFormCollection Empty = new HttpFormCollection();
        private static readonly string[] EmptyKeys = Array.Empty<string>();
        private static readonly StringValues[] EmptyValues = Array.Empty<StringValues>();
        private static readonly Enumerator EmptyEnumerator = new Enumerator();
        private static readonly IEnumerator<KeyValuePair<string, StringValues>> EmptyIEnumeratorType = EmptyEnumerator;
        private static readonly IEnumerator EmptyIEnumerator = EmptyEnumerator;

        private static HttpFormFileCollection EmptyFiles = new HttpFormFileCollection();

        private HttpFormFileCollection _files;

        private HttpFormCollection()
        {
        }

        public HttpFormCollection(Dictionary<string, StringValues> fields, HttpFormFileCollection files = null)
        {
            Store = fields;
            _files = files;
        }

        public HttpFormFileCollection Files
        {
            get
            {
                return _files ?? EmptyFiles;
            }
            private set { _files = value; }
        }

        private Dictionary<string, StringValues> Store { get; set; }

      
        public StringValues this[string key]
        {
            get
            {
                if (Store == null)
                {
                    return StringValues.Empty;
                }

                StringValues value;
                if (TryGetValue(key, out value))
                {
                    return value;
                }
                return StringValues.Empty;
            }
        }

        
        public int Count
        {
            get
            {
                return Store?.Count ?? 0;
            }
        }

        public ICollection<string> Keys
        {
            get
            {
                if (Store == null)
                {
                    return EmptyKeys;
                }
                return Store.Keys;
            }
        }

      
        public bool ContainsKey(string key)
        {
            if (Store == null)
            {
                return false;
            }
            return Store.ContainsKey(key);
        }
        
        public bool TryGetValue(string key, out StringValues value)
        {
            if (Store == null)
            {
                value = default(StringValues);
                return false;
            }
            return Store.TryGetValue(key, out value);
        }

        public Enumerator GetEnumerator()
        {
            if (Store == null || Store.Count == 0)
            {
                return EmptyEnumerator;
            }
            return new Enumerator(Store.GetEnumerator());
        }
        
        IEnumerator<KeyValuePair<string, StringValues>> IEnumerable<KeyValuePair<string, StringValues>>.GetEnumerator()
        {
            if (Store == null || Store.Count == 0)
            {
                return EmptyIEnumeratorType;
            }
            return Store.GetEnumerator();
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            if (Store == null || Store.Count == 0)
            {
                return EmptyIEnumerator;
            }
            return Store.GetEnumerator();
        }
    }

    public struct Enumerator : IEnumerator<KeyValuePair<string, StringValues>>
    {
        private Dictionary<string, StringValues>.Enumerator _dictionaryEnumerator;
        private bool _notEmpty;

        internal Enumerator(Dictionary<string, StringValues>.Enumerator dictionaryEnumerator)
        {
            _dictionaryEnumerator = dictionaryEnumerator;
            _notEmpty = true;
        }

        public bool MoveNext()
        {
            if (_notEmpty)
            {
                return _dictionaryEnumerator.MoveNext();
            }
            return false;
        }

        public KeyValuePair<string, StringValues> Current
        {
            get
            {
                if (_notEmpty)
                {
                    return _dictionaryEnumerator.Current;
                }
                return default(KeyValuePair<string, StringValues>);
            }
        }

        public void Dispose()
        {
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        void IEnumerator.Reset()
        {
            if (_notEmpty)
            {
                ((IEnumerator)_dictionaryEnumerator).Reset();
            }
        }
    }
}
