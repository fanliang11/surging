using Surging.Core.CPlatform.Diagnostics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.ProxyGenerator.Diagnostics
{
   public class RpcRequestCarrierHeaderCollection : ICarrierHeaderCollection
    {
        private readonly TracingHeaders _tracingHeaders;

        public RpcRequestCarrierHeaderCollection(TracingHeaders tracingHeaders)
        {
            _tracingHeaders = tracingHeaders;
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _tracingHeaders.GetEnumerator();
        }

        public void Add(string key, string value)
        {
            _tracingHeaders.Add(key, value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _tracingHeaders.GetEnumerator();
        }
    }
}