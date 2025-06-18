using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Mqtt.Interceptors
{
    public interface IMqttInterceptor
    {
        Task<bool> Intercept(IMqttInvocation invocation);
    }
}
