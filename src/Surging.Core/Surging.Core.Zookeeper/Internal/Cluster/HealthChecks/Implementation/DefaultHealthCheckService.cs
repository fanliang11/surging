using Surging.Core.CPlatform.Address;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.Zookeeper.Internal.Cluster.HealthChecks.Implementation
{
    /// <summary>
    /// Defines the <see cref="DefaultHealthCheckService" />
    /// </summary>
    public class DefaultHealthCheckService : IHealthCheckService
    {
        #region 字段

        /// <summary>
        /// Defines the _dictionary
        /// </summary>
        private readonly ConcurrentDictionary<ValueTuple<string, int>, MonitorEntry> _dictionary =
    new ConcurrentDictionary<ValueTuple<string, int>, MonitorEntry>();

        /// <summary>
        /// Defines the _timeout
        /// </summary>
        private readonly int _timeout = 30000;

        /// <summary>
        /// Defines the _timer
        /// </summary>
        private readonly Timer _timer;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultHealthCheckService"/> class.
        /// </summary>
        public DefaultHealthCheckService()
        {
            var timeSpan = TimeSpan.FromSeconds(60);

            _timer = new Timer(async s =>
            {
                await Check(_dictionary.ToArray().Select(i => i.Value), _timeout);
            }, null, timeSpan, timeSpan);
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The Dispose
        /// </summary>
        public void Dispose()
        {
            _timer.Dispose();
        }

        /// <summary>
        /// The IsHealth
        /// </summary>
        /// <param name="address">The address<see cref="AddressModel"/></param>
        /// <returns>The <see cref="ValueTask{bool}"/></returns>
        public async ValueTask<bool> IsHealth(AddressModel address)
        {
            var ipAddress = address as IpAddressModel;
            MonitorEntry entry;
            var isHealth = !_dictionary.TryGetValue(new ValueTuple<string, int>(ipAddress.Ip, ipAddress.Port), out entry) ? await Check(address, _timeout) : entry.Health;
            return isHealth;
        }

        /// <summary>
        /// The Monitor
        /// </summary>
        /// <param name="address">The address<see cref="AddressModel"/></param>
        public void Monitor(AddressModel address)
        {
            var ipAddress = address as IpAddressModel;
            _dictionary.GetOrAdd(new ValueTuple<string, int>(ipAddress.Ip, ipAddress.Port), k => new MonitorEntry(address));
        }

        /// <summary>
        /// The Check
        /// </summary>
        /// <param name="address">The address<see cref="AddressModel"/></param>
        /// <param name="timeout">The timeout<see cref="int"/></param>
        /// <returns>The <see cref="Task{bool}"/></returns>
        private static async Task<bool> Check(AddressModel address, int timeout)
        {
            bool isHealth = false;
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) { SendTimeout = timeout })
            {
                try
                {
                    await socket.ConnectAsync(address.CreateEndPoint());
                    isHealth = true;
                }
                catch
                {
                }
                return isHealth;
            }
        }

        /// <summary>
        /// The Check
        /// </summary>
        /// <param name="entrys">The entrys<see cref="IEnumerable{MonitorEntry}"/></param>
        /// <param name="timeout">The timeout<see cref="int"/></param>
        /// <returns>The <see cref="Task"/></returns>
        private static async Task Check(IEnumerable<MonitorEntry> entrys, int timeout)
        {
            foreach (var entry in entrys)
            {
                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) { SendTimeout = timeout })
                {
                    try
                    {
                        await socket.ConnectAsync(entry.EndPoint);
                        entry.UnhealthyTimes = 0;
                        entry.Health = true;
                    }
                    catch
                    {
                        entry.UnhealthyTimes++;
                        entry.Health = false;
                    }
                }
            }
        }

        #endregion 方法

        /// <summary>
        /// Defines the <see cref="MonitorEntry" />
        /// </summary>
        protected class MonitorEntry
        {
            #region 构造函数

            /// <summary>
            /// Initializes a new instance of the <see cref="MonitorEntry"/> class.
            /// </summary>
            /// <param name="addressModel">The addressModel<see cref="AddressModel"/></param>
            /// <param name="health">The health<see cref="bool"/></param>
            public MonitorEntry(AddressModel addressModel, bool health = true)
            {
                EndPoint = addressModel.CreateEndPoint();
                Health = health;
            }

            #endregion 构造函数

            #region 属性

            /// <summary>
            /// Gets or sets the EndPoint
            /// </summary>
            public EndPoint EndPoint { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether Health
            /// </summary>
            public bool Health { get; set; }

            /// <summary>
            /// Gets or sets the UnhealthyTimes
            /// </summary>
            public int UnhealthyTimes { get; set; }

            #endregion 属性
        }
    }
}