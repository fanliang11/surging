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

        private readonly Timer _timer;

        public DefaultHealthCheckService(IServiceRouteManager serviceRouteManager)
        {
            var timeSpan = TimeSpan.FromSeconds(10);
            _timer = new Timer(s =>
            {
                Check(_dictionary.ToArray().Select(i => i.Value));
            }, null, timeSpan, timeSpan);

            //去除监控。
            serviceRouteManager.Removed += (s, e) =>
            {
                Remove(e.Route.Address);
            };
            //重新监控。
            serviceRouteManager.Created += (s, e) =>
            {
                var keys = e.Route.Address.Select(i => i.ToString());
                Check(_dictionary.Where(i => keys.Contains(i.Key)).Select(i => i.Value));
            };
            //重新监控。
            serviceRouteManager.Changed += (s, e) =>
            {
                var keys = e.Route.Address.Select(i => i.ToString());
                Check(_dictionary.Where(i => keys.Contains(i.Key)).Select(i => i.Value));
            };
        }

        #region Implementation of IHealthCheckService

        /// <summary>
        /// 监控一个地址。
        /// </summary>
        /// <param name="address">地址模型。</param>
        /// <returns>一个任务。</returns>
        public Task Monitor(AddressModel address)
        {
            return Task.Run(() => { _dictionary.GetOrAdd(address.ToString(), k => new MonitorEntry(address)); });
        }

        /// <summary>
        /// 判断一个地址是否健康。
        /// </summary>
        /// <param name="address">地址模型。</param>
        /// <returns>健康返回true，否则返回false。</returns>
        public Task<bool> IsHealth(AddressModel address)
        {
            return Task.Run(() =>
            {
                var key = address.ToString();
                MonitorEntry entry;
                return !_dictionary.TryGetValue(key, out entry) ? Check(address): entry.Health;
            });
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

        private static bool Check(AddressModel address)
        {
            bool isHealth = false;
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                try
                {
                    socket.Connect(address.CreateEndPoint());
                    isHealth = true;
                }
                catch
                {
                }
                return isHealth;
            }
        }

        private static void Check(IEnumerable<MonitorEntry> entrys)
        {
            foreach (var entry in entrys)
            {
                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    try
                    {
                        socket.Connect(entry.EndPoint);
                        entry.Health = true;
                    }
                    catch
                    {
                        entry.Health = false;
                    }
                }
            }
            /*foreach (var entry in entrys)
            {
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                var socketAsyncEventArgs = new SocketAsyncEventArgs
                {
                    RemoteEndPoint = entry.EndPoint,
                    UserToken = new KeyValuePair<MonitorEntry, Socket>(entry, socket)
                };
                socketAsyncEventArgs.Completed += (sender, e) =>
                {
                    var isHealth = e.SocketError == SocketError.Success;

                    var token = (KeyValuePair<MonitorEntry, Socket>)e.UserToken;
                    token.Key.Health = isHealth;

                    e.Dispose();
                    token.Value.Dispose();
                };

                socket.ConnectAsync(socketAsyncEventArgs);
            }*/
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

            public EndPoint EndPoint { get; set; }
            public bool Health { get; set; }
        }

        #endregion Help Class
    }
}