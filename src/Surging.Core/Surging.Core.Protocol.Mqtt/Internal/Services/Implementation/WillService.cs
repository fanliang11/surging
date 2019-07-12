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
    /// <summary>
    /// Defines the <see cref="WillService" />
    /// </summary>
    public class WillService : IWillService
    {
        #region 字段

        /// <summary>
        /// Defines the _logger
        /// </summary>
        private readonly ILogger<WillService> _logger;

        /// <summary>
        /// Defines the _serviceProvider
        /// </summary>
        private readonly CPlatformContainer _serviceProvider;

        /// <summary>
        /// Defines the willMeaasges
        /// </summary>
        private ConcurrentDictionary<String, MqttWillMessage> willMeaasges = new ConcurrentDictionary<String, MqttWillMessage>();

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="WillService"/> class.
        /// </summary>
        /// <param name="logger">The logger<see cref="ILogger{WillService}"/></param>
        /// <param name="serviceProvider">The serviceProvider<see cref="CPlatformContainer"/></param>
        public WillService(ILogger<WillService> logger, CPlatformContainer serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The Add
        /// </summary>
        /// <param name="deviceid">The deviceid<see cref="string"/></param>
        /// <param name="willMessage">The willMessage<see cref="MqttWillMessage"/></param>
        public void Add(string deviceid, MqttWillMessage willMessage)
        {
            willMeaasges.AddOrUpdate(deviceid, willMessage, (id, message) => willMessage);
        }

        /// <summary>
        /// The Remove
        /// </summary>
        /// <param name="deviceid">The deviceid<see cref="string"/></param>
        public void Remove(string deviceid)
        {
            willMeaasges.TryRemove(deviceid, out MqttWillMessage message);
        }

        /// <summary>
        /// The SendWillMessage
        /// </summary>
        /// <param name="deviceId">The deviceId<see cref="string"/></param>
        /// <returns>The <see cref="Task"/></returns>
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

        #endregion 方法
    }
}