using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.Protocol.Mqtt.Internal.Services;
using System.Linq;

namespace Surging.Core.Protocol.Mqtt.Internal.Runtime.Implementation
{
    /// <summary>
    /// Defines the <see cref="DefaultMqttBehaviorProvider" />
    /// </summary>
    public class DefaultMqttBehaviorProvider : IMqttBehaviorProvider
    {
        #region 字段

        /// <summary>
        /// Defines the _serviceEntryProvider
        /// </summary>
        private readonly IServiceEntryProvider _serviceEntryProvider;

        /// <summary>
        /// Defines the _serviceProvider
        /// </summary>
        private readonly CPlatformContainer _serviceProvider;

        /// <summary>
        /// Defines the _mqttBehavior
        /// </summary>
        private MqttBehavior _mqttBehavior;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultMqttBehaviorProvider"/> class.
        /// </summary>
        /// <param name="serviceEntryProvider">The serviceEntryProvider<see cref="IServiceEntryProvider"/></param>
        /// <param name="serviceProvider">The serviceProvider<see cref="CPlatformContainer"/></param>
        public DefaultMqttBehaviorProvider(IServiceEntryProvider serviceEntryProvider, CPlatformContainer serviceProvider)
        {
            _serviceEntryProvider = serviceEntryProvider;
            _serviceProvider = serviceProvider;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The GetMqttBehavior
        /// </summary>
        /// <returns>The <see cref="MqttBehavior"/></returns>
        public MqttBehavior GetMqttBehavior()
        {
            if (_mqttBehavior == null)
            {
                _mqttBehavior = _serviceEntryProvider.GetTypes()
                   .Select(type => _serviceProvider.GetInstances(type) as MqttBehavior).Where(p => p != null).FirstOrDefault();
            }
            return _mqttBehavior;
        }

        #endregion 方法
    }
}