using Surging.Core.CPlatform.Diagnostics;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Serialization;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using SurgingEvents = Surging.Core.CPlatform.Diagnostics.DiagnosticListenerExtensions;

namespace Surging.Core.Protocol.Mqtt.Diagnostics
{
     public class MqttTransportDiagnosticProcessor: ITracingDiagnosticProcessor
    {
        private Func<TransportEventData, string> _transportOperationNameResolver;
        public string ListenerName => SurgingEvents.DiagnosticListenerName;


        private readonly ISerializer<string> _serializer;
        private readonly ITracingContext _tracingContext;
        private readonly IEntrySegmentContextAccessor _segmentContextAccessor;

        public Func<TransportEventData, string> TransportOperationNameResolver
        {
            get
            {
                return _transportOperationNameResolver ??
                       (_transportOperationNameResolver = (data) => "Mqtt-Transport:: " + data.Message.MessageName);
            }
            set => _transportOperationNameResolver =
                value ?? throw new ArgumentNullException(nameof(TransportOperationNameResolver));
        }

        public MqttTransportDiagnosticProcessor(ITracingContext tracingContext, ISerializer<string> serializer, IEntrySegmentContextAccessor contextAccessor)
        {
            _tracingContext = tracingContext;
            _serializer = serializer;
            _segmentContextAccessor = contextAccessor;
        }

        [DiagnosticName(SurgingEvents.SurgingBeforeTransport, TransportType.Mqtt)]
        public void TransportBefore([Object] TransportEventData eventData)
        {
            var message = eventData.Message.GetContent<RemoteInvokeMessage>();
            var operationName = TransportOperationNameResolver(eventData);
            var context = _tracingContext.CreateEntrySegmentContext(operationName, new MqttTransportCarrierHeaderCollection(eventData.Headers));
            if (!string.IsNullOrEmpty(eventData.TraceId))
                context.TraceId = ConvertUniqueId(eventData);
            context.Span.AddLog(LogEvent.Message($"Worker running at: {DateTime.Now}"));
            context.Span.SpanLayer = SpanLayer.RPC_FRAMEWORK;
            context.Span.AddTag(Tags.MQTT_CLIENT_ID, eventData.TraceId.ToString());
            context.Span.AddTag(Tags.MQTT_METHOD, eventData.Method.ToString());
            context.Span.AddTag(Tags.REST_PARAMETERS, _serializer.Serialize(message.Parameters));
            context.Span.AddTag(Tags.MQTT_BROKER_ADDRESS, NetUtils.GetHostAddress().ToString());
        }

        [DiagnosticName(SurgingEvents.SurgingAfterTransport, TransportType.Mqtt)]
        public void TransportAfter([Object] ReceiveEventData eventData)
        {
            var context = _segmentContextAccessor.Context;
            if (context != null)
            {
                _tracingContext.Release(context);
            }
        }

        [DiagnosticName(SurgingEvents.SurgingErrorTransport, TransportType.Mqtt)]
        public void TransportError([Object] TransportErrorEventData eventData)
        {
            var context = _segmentContextAccessor.Context;
            if (context != null)
            {
                context.Span.ErrorOccurred(eventData.Exception);
                _tracingContext.Release(context);
            }
        }

        public UniqueId ConvertUniqueId(TransportEventData eventData)
        {
            long part1 = 0, part2 = 0, part3 = 0;
            UniqueId uniqueId = new UniqueId();
            var bytes = Encoding.Default.GetBytes($"{eventData.TraceId}-{nameof(MqttTransportDiagnosticProcessor)}");
            part1 = BitConverter.ToInt64(bytes, 0);
            if (eventData.TraceId.Length > 8)
                part2 = BitConverter.ToInt64(bytes, 8);
            if (eventData.TraceId.Length > 16)
                part3 = BitConverter.ToInt64(bytes, 16);
            if (!string.IsNullOrEmpty(eventData.TraceId))
                uniqueId = new UniqueId(part1, part2, part3);
            return uniqueId;
        }
    }
}