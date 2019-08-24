using Surging.Core.CPlatform.Diagnostics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.Mqtt.Diagnostics
{
    public class MqttTransportCarrierHeaderCollection : ICarrierHeaderCollection
    {
        private readonly TracingHeaders _tracingHeaders;

        public MqttTransportCarrierHeaderCollection(TracingHeaders tracingHeaders)
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
