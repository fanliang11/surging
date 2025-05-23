using Autofac;
using Surging.Core.CPlatform.Serialization;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Transport.Implementation
{
    public class PayloadContext
    {
        private readonly ISerializer<string> _serializer;
        public PayloadContext(ISerializer<string> serializer)
        {
            _serializer = serializer;
        }

        public T Get<T>()
        {
            var payload = RpcContext.GetContext().GetAttachment("payload");
            var model = _serializer.Deserialize(payload?.ToString().Trim('"').Replace("\\", ""), typeof(T));
            return model == null ? default : (T)model;
        }

        public static T GetPayload<T>()
        {
            var payload = RpcContext.GetContext().GetAttachment("payload");
            var model = ServiceLocator.Current.Resolve<ISerializer<string>>()
                .Deserialize(payload?.ToString().Trim('"').Replace("\\", ""), typeof(T));
            return model == null ? default : (T)model;
        }
    }
}
