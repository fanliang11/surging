using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Routing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Runtime.Client.HealthChecks.Implementation
{
    public class DefaultHealthCheckService : IHealthCheckService, IDisposable
    {
        private readonly ConcurrentDictionary<string, MonitorEntry> _dictionary =
            new ConcurrentDictionary<string, MonitorEntry>();
        private readonly IServiceRouteManager _serviceRouteManager;
        private readonly int _timeout = 30000;
        private readonly Timer _timer;

        public DefaultHealthCheckService(IServiceRouteManager serviceRouteManager)
        {
            var timeSpan = TimeSpan.FromSeconds(10);
            
            _serviceRouteManager = serviceRouteManager;
            _timer = new Timer(async s =>
            {
                await Check(_dictionary.ToArray().Select(i => i.Value), _timeout);
                RemoveUnhealthyAddress(_dictionary.ToArray().Select(i => i.Value).Where(m => m.UnhealthyTimes >= 6));
            }, null, timeSpan, timeSpan);

            //去除监控。
            serviceRouteManager.Removed += (s, e) =>
            {
                Remove(e.Route.Address);
            };
            //重新监控。
            serviceRouteManager.Created += async (s, e) =>
            {
                var keys = e.Route.Address.Select(i => i.ToString());
                await  Check(_dictionary.Where(i => keys.Contains(i.Key)).Select(i => i.Value), _timeout);
            };
            //重新监控。
            serviceRouteManager.Changed +=async (s, e) =>
            {
                var keys = e.Route.Address.Select(i => i.ToString());
                await  Check(_dictionary.Where(i => keys.Contains(i.Key)).Select(i => i.Value), _timeout);
            };
        }


        #region Implementation of IHealthCheckService

        /// <summary>
        /// 监控一个地址。
        /// </summary>
        /// <param name="address">地址模型。</param>
        /// <returns>一个任务。</returns>
        public void Monitor(AddressModel address)
        {
            _dictionary.GetOrAdd(address.ToString(), k => new MonitorEntry(address));
        }

        /// <summary>
        /// 判断一个地址是否健康。
        /// </summary>
        /// <param name="address">地址模型。</param>
        /// <returns>健康返回true，否则返回false。</returns>
        public ValueTask<bool> IsHealth(AddressModel address)
        {
            var key = address.ToString();
            MonitorEntry entry;
            return !_dictionary.TryGetValue(key, out entry) ? new ValueTask<bool>(Check(address,_timeout)) : new ValueTask<bool>(entry.Health);
        }

        /// <summary>
        /// 标记一个地址为失败的。
        /// </summary>
        /// <param name="address">地址模型。</param>
        /// <returns>一个任务。</returns>
        public Task MarkFailure(AddressModel address)
        {
            return Task.Run(() =>
            {
                var key = address.ToString();
                var entry = _dictionary.GetOrAdd(key, k => new MonitorEntry(address, false));
                entry.Health = false;
            });
        }

        #endregion Implementation of IHealthCheckService

        #region Implementation of IDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            _timer.Dispose();
        }

        #endregion Implementation of IDisposable

        #region Private Method

        private void Remove(IEnumerable<AddressModel> addressModels)
        {
            foreach (var addressModel in addressModels)
            {
                MonitorEntry value;
                _dictionary.TryRemove(addressModel.ToString(), out value);
            }
        }

        private void RemoveUnhealthyAddress(IEnumerable<MonitorEntry> monitorEntry)
        {
            if (monitorEntry.Any())
            {
                var addresses = monitorEntry.Select(p =>
                {
                    var ipEndPoint = p.EndPoint as IPEndPoint;
                    return new IpAddressModel(ipEndPoint.Address.ToString(), ipEndPoint.Port);
                }).ToList();
                _serviceRouteManager.RemveAddressAsync(addresses).Wait();
                addresses.ForEach(p => _dictionary.TryRemove(p.ToString(), out MonitorEntry value));
              
            }
        }

        private static async Task<bool> Check(AddressModel address,int timeout)
        {
            bool isHealth = false;
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) { SendTimeout= timeout })
            {
                try
                {
                    await  socket.ConnectAsync(address.CreateEndPoint());
                    isHealth = true;
                }
                catch
                {

                }
                return isHealth;
            }
        }

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

        #endregion Private Method

        #region Help Class

        protected class MonitorEntry
        {
            public MonitorEntry(AddressModel addressModel, bool health = true)
            {
                EndPoint = addressModel.CreateEndPoint();
                Health = health;

            }

            public int UnhealthyTimes { get; set; }

            public EndPoint EndPoint { get; set; }
            public bool Health { get; set; }
        }

        #endregion Help Class
    }
}