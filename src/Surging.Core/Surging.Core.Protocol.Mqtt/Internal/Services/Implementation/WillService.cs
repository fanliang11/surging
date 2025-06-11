using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform;
using Surging.Core.Protocol.Mqtt.Internal.Messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Mqtt.Internal.Services.Implementation
{
   public class WillService: IWillService
    {
        private  ConcurrentDictionary<String, MqttWillMessage> willMeaasges = new ConcurrentDictionary<String, MqttWillMessage>();
        private readonly ILogger<WillService> _logger;
        private readonly CPlatformContainer _serviceProvider;

        public WillService(ILogger<WillService> logger,  CPlatformContainer serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public void Add(string deviceid, MqttWillMessage willMessage)
        {
            willMeaasges.AddOrUpdate(deviceid, willMessage,(id,message)=>willMessage); 
        }
        
        public async Task SendWillMessage(string deviceId)
        {
            if (!string.IsNullOrEmpty(deviceId))
            {
                willMeaasges.TryGetValue(deviceId, out MqttWillMessage willMessage);
                if (willMeaasges != null)
                {
                   await _serviceProvider.GetInstances<IChannelService>().SendWillMsg(willMessage);
                    if (!willMessage.WillRetain)
                    {
                        Remove(deviceId);
                        if (_logger.IsEnabled(LogLevel.Information))
                            _logger.LogInformation($"deviceId:{deviceId} 的遗嘱消息[" + willMessage.WillMessage + "] 已经被删除");

                    }
                }
            }
        }
        
        public void Remove(string deviceid) {
            willMeaasges.TryRemove(deviceid,out MqttWillMessage message);
        }
    }
}
