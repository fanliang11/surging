using Surging.Core.DeviceGateway.Runtime.session;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Session.Implementation
{
    internal class DefaultDeviceSessionManager : IDeviceSessionManager
    {
        public event ListenDelegate ListenEvent;
        private readonly ConcurrentDictionary<string, IDeviceSession> _deviceSessions=new ConcurrentDictionary<string, IDeviceSession>();

        public IObservable<IDeviceSession> AddorUpdate(string deviceId, Func<IDeviceSession, IDeviceSession> func)
        {
            IDeviceSession deviceSession = _deviceSessions.GetValueOrDefault(deviceId);
           
            var newDeviceSession = func.Invoke(deviceSession);
            var result = _deviceSessions.AddOrUpdate(deviceId,
                newDeviceSession, (key, value) => newDeviceSession);
            OnListen(new DeviceSessionEventArg(DeviceSessionEventArg.DeviceSessionEventArgType.Register,
                result));

            return Observable.Return(result);
       
        }

        public IObservable<IDeviceSession> AddorUpdate(string deviceId, IDeviceSession creator, Func<IDeviceSession, IDeviceSession> updater)
        { 
               var result=_deviceSessions.AddOrUpdate(deviceId,
                   creator, (key, value) => updater.Invoke(value));
            OnListen(new DeviceSessionEventArg(DeviceSessionEventArg.DeviceSessionEventArgType.Register,
          result)); 
            return Observable.Return(result);
        }

        public IObservable<bool> CheckAlive(string deviceId, bool onlyLocal)
        {
             var deviceSession= _deviceSessions.GetValueOrDefault(deviceId);
            return Observable.Return( deviceSession.IsAlive());
        }

        public string GetCurrentServerId()
        {
            return "";
        }

        public ISubject<DeviceSessionInfo> GetLocalSessionInfo()
        {
            var result=new AsyncSubject<DeviceSessionInfo>();
            _deviceSessions.Values.ToList().ForEach(p =>
            {
                result.OnNext(new DeviceSessionInfo(p.GetId(),p));
            });
            result.OnCompleted();
            return result;
        }

        public IObservable<IDeviceSession> GetSession(string deviceId)
        {
            var deviceSession = _deviceSessions.GetValueOrDefault(deviceId);
            return Observable.Return(deviceSession);
        }

        public IObservable<IDeviceSession> GetSession(string deviceId, bool unregisterWhenNotAlive)
        {
            var deviceSession = _deviceSessions.GetValueOrDefault(deviceId);
            if (deviceSession != null && deviceSession.IsAlive() && unregisterWhenNotAlive)
            {
                _deviceSessions.TryRemove(deviceId, out deviceSession);
                OnListen(new DeviceSessionEventArg(
                    DeviceSessionEventArg.DeviceSessionEventArgType.Unregister,
                        deviceSession));
            }
            return Observable.Return(deviceSession);
        }

        public ISubject<DeviceSessionInfo> GetSessionInfo()
        {
            return GetLocalSessionInfo();
        }

        public ISubject<DeviceSessionInfo> GetSessionInfo(string serverId)
        {
            return GetLocalSessionInfo();
        }

        public ISubject<IDeviceSession> GetSessions()
        {
            var result = new AsyncSubject<IDeviceSession>();
            _deviceSessions.Values.ToList().ForEach(deviceSession =>
            {
                result.OnNext(deviceSession);
            });
            result.OnCompleted();
            return result;
        }

        public IObservable<bool> IsAlive(string deviceId)
        {
            var devSesssion = _deviceSessions.GetValueOrDefault(deviceId);
            return Observable.Return(devSesssion.IsAlive());
        }

        public IObservable<bool> IsAlive(string deviceId, bool onlyLocal)
        {
            var devSesssion = _deviceSessions.GetValueOrDefault(deviceId);
            return Observable.Return(devSesssion.IsAlive());
        }

        public IObservable<bool> Remove(string deviceId, bool onlyLocal)
        {
           var result= _deviceSessions.TryRemove(deviceId, out IDeviceSession deviceSession);
            OnListen(new DeviceSessionEventArg(
                    DeviceSessionEventArg.DeviceSessionEventArgType.Unregister,
                        deviceSession));
            return Observable.Return(result);
        }

        public IObservable<long> TotalSessions(bool onlyLocal)
        {
            return Observable.Return((long)_deviceSessions.Count());
        }

        public async void OnListen(DeviceSessionEventArg arg)
        {
            if (ListenEvent == null)
                return;
            await ListenEvent(arg);
        }
    }
}
