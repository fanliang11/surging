﻿using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Exceptions;
using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Transport;
using Surging.Core.CPlatform.Utilities;
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
    /// <summary>
    /// 默认健康检查服务(每10秒会检查一次服务状态，在构造函数中添加服务管理事件) 
    /// </summary>
    public class DefaultHealthCheckService : IHealthCheckService, IDisposable
    {
        private readonly Func<EndPoint, Task<bool>> _isService;
        private readonly ConcurrentDictionary<ValueTuple<string, int>, MonitorEntry> _dictionary =
            new ConcurrentDictionary<ValueTuple<string, int>, MonitorEntry>();
        private readonly IServiceRouteManager _serviceRouteManager;
        private readonly int _timeout = 30000;
        private readonly Timer _timer;
        private EventHandler<HealthCheckEventArgs> _removed;

        private EventHandler<HealthCheckEventArgs> _changed;

        public event EventHandler<HealthCheckEventArgs> Removed
        {
            add { _removed += value; }
            remove { _removed -= value; }
        }

        public event EventHandler<HealthCheckEventArgs> Changed
        {
            add { _changed += value; }
            remove { _changed -= value; }
        }

        /// <summary>
        /// 默认心跳检查服务(每10秒会检查一次服务状态，在构造函数中添加服务管理事件) 
        /// </summary>
        /// <param name="serviceRouteManager"></param>
        public DefaultHealthCheckService(IServiceRouteManager serviceRouteManager)
        {
            var timeSpan = TimeSpan.FromSeconds(10);
            _isService = async address => (await _serviceRouteManager.GetAddressAsync(address.ToString())).Count() > 0;
            _serviceRouteManager = serviceRouteManager;
            //建立计时器
            _timer = new Timer(async s =>
            {

                //检查服务是否可用
                await Check(_dictionary.ToArray().Select(i => i.Value), _timeout, CheckService, _isService);
                //移除不可用的服务地址
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
                var keys = e.Route.Address.Select(address =>
                {
                    var ipAddress = address as IpAddressModel;
                    return new ValueTuple<string, int>(ipAddress.Ip, ipAddress.Port);
                });
                await Check(_dictionary.Where(i => keys.Contains(i.Key)).Select(i => i.Value), _timeout, CheckService, _isService);
            };
            //重新监控。
            serviceRouteManager.Changed += async (s, e) =>
            {
                var keys = e.Route.Address.Select(address =>
                {
                    var ipAddress = address as IpAddressModel;
                    return new ValueTuple<string, int>(ipAddress.Ip, ipAddress.Port);
                });
                await Check(_dictionary.Where(i => keys.Contains(i.Key)).Select(i => i.Value), _timeout, CheckService, _isService);
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
            var ipAddress = address as IpAddressModel;
            _dictionary.GetOrAdd(new ValueTuple<string, int>(ipAddress.Ip, ipAddress.Port), k => new MonitorEntry(address));
        }

        /// <summary>
        /// 判断一个地址是否健康。
        /// </summary>
        /// <param name="address">地址模型。</param>
        /// <returns>健康返回true，否则返回false。</returns>
        public async ValueTask<bool> IsHealth(AddressModel address)
        {
            var ipAddress = address as IpAddressModel;
            MonitorEntry entry;
            var isHealth = !_dictionary.TryGetValue(new ValueTuple<string, int>(ipAddress.Ip, ipAddress.Port), out entry) ? (await _isService(address.CreateEndPoint()) ? await CheckService(address.CreateEndPoint(), _timeout) : await Check(address.CreateEndPoint(), _timeout)) : entry.Health;
            OnChanged(new HealthCheckEventArgs(address, isHealth));
            return isHealth;
        }

        public async ValueTask<bool> MonitorHealth(AddressModel address)
        {
            var ipAddress = address as IpAddressModel;
            MonitorEntry entry;
            var getResult = !_dictionary.TryGetValue(new ValueTuple<string, int>(ipAddress.Ip, ipAddress.Port), out entry);
            var isHealth = getResult ? (await _isService(address.CreateEndPoint()) ? await CheckService(address.CreateEndPoint(), _timeout) : await Check(address.CreateEndPoint(), _timeout)) : entry.Health;
            if (getResult)
                Monitor(address);
            OnChanged(new HealthCheckEventArgs(address, isHealth));
            return isHealth;
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
                var ipAddress = address as IpAddressModel;
                var entry = _dictionary.GetOrAdd(new ValueTuple<string, int>(ipAddress.Ip, ipAddress.Port), k => new MonitorEntry(address, false));
                entry.Health = false;
            });
        }

        protected void OnRemoved(params HealthCheckEventArgs[] args)
        {
            if (_removed == null)
                return;

            foreach (var arg in args)
                _removed(this, arg);
        }

        protected void OnChanged(params HealthCheckEventArgs[] args)
        {
            if (_changed == null)
                return;

            foreach (var arg in args)
                _changed(this, arg);
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
                var ipAddress = addressModel as IpAddressModel;
                _dictionary.TryRemove(new ValueTuple<string, int>(ipAddress.Ip, ipAddress.Port), out value);
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
                addresses.ForEach(p =>
                {
                    var ipAddress = p as IpAddressModel;
                    _dictionary.TryRemove(new ValueTuple<string, int>(ipAddress.Ip, ipAddress.Port), out MonitorEntry value);
                });
                OnRemoved(addresses.Select(p => new HealthCheckEventArgs(p)).ToArray());
            }
        }

        private async Task<bool> CheckService(EndPoint address, int timeout)
        {
            var transportClientFactory = ServiceLocator.GetService<ITransportClientFactory>();
            bool isHealth = false;
            var client = await transportClientFactory.CreateClientAsync(address);
            try
            {
                using (var cts = new CancellationTokenSource())
                {
                    await client.SendAsync(new Messages.RemoteInvokeMessage()
                    {
                        DecodeJOject = false,
                        ServiceId = "client.checkService",
                        Attachments = new Dictionary<string, object>()

                    }, cts.Token).WithCancellation(cts, timeout);
                    isHealth = true;
                }
            }
            catch (CommunicationException ex)
            {
                isHealth = false;
            }
            catch (Exception exception)
            {
                isHealth = false;
            }
            return isHealth;
        }

        private static async Task<bool> Check(EndPoint address, int timeout)
        {
            bool isHealth = false;
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) { SendTimeout = timeout })
            {
                try
                {
                    await socket.ConnectAsync(address);
                    isHealth = true;
                }
                catch
                {

                }
                return isHealth;
            }
        }

        private static async Task Check(IEnumerable<MonitorEntry> entrys, int timeout, Func<EndPoint, int, Task<bool>> checkService, Func<EndPoint, Task<bool>> isService)
        {
            foreach (var entry in entrys)
            {
                var endPoint = entry.EndPoint;
                if (await isService(endPoint))
                {
                    entry.Health = await checkService(endPoint, timeout);

                }
                else
                {
                    entry.Health = await Check(entry.EndPoint, timeout);
                }
                if (entry.Health)
                    entry.UnhealthyTimes = 0;
                else
                    entry.UnhealthyTimes++;
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