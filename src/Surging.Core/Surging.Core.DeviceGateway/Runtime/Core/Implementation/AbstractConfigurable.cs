using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Core.Implementation
{
    public abstract class AbstractConfigurable : IConfigurable
    {
        private readonly ConcurrentDictionary<string, object> _configurables=new ConcurrentDictionary<string, object>();

        public IObservable<ValueObj> GetConfig(string key)
        { 
            return Observable.Return(
              new ValueObj( _configurables.GetValueOrDefault(key,default(ValueObj))));
        }

        public IObservable<ValueObjs> GetConfigs(params string[] keies)
        {
            var result = new ValueObjs();
            foreach(var key in keies)
            {
                result.Add(key, _configurables.GetValueOrDefault(key, default(ValueObj)));
            }
            return Observable.Return(result);
        }

        public IObservable<bool> RemoveConfig(string key)
        {
            return Observable.Return(_configurables.Remove(key, out object obj));
        }

        public IObservable<bool> RemoveConfigs(string[] keies)
        {
            var result = false;
            foreach (var key in keies)
            {
                result= _configurables.Remove(key, out object obj); 
            }
            return Observable.Return(result);
        }

        public IObservable<bool> SetConfig(string key, object value)
        {
            _configurables.AddOrUpdate(key, value, (key, value) => value);
            return Observable.Return(true);
        }

        public IObservable<bool> SetConfig(KeyValuePair<string, object> keyValue)
        {
            _configurables.AddOrUpdate(keyValue.Key, keyValue.Value, (key, value) => keyValue.Value);
            return Observable.Return(true);
        }

        public IObservable<bool> SetConfigs(Dictionary<string, object> configs)
        {
            foreach (var config in configs)
            {
                _configurables.AddOrUpdate(config.Key, config.Value, (key, value) => config.Value);
            }
            return Observable.Return(true);
        }

        public IObservable<bool> TryRemoveConfig(string key, out ValueObj valueObj)
        {
            var result = false;
            valueObj = default;
            if (_configurables.TryRemove(key, out object obj))
            {
                result = true;
                valueObj = new  ValueObj(obj);
            }
            return Observable.Return(result);
        }

        public IObservable<bool> TryRemoveConfigs(string[] keies, out ValueObjs valueObjs)
        {
            valueObjs= new ValueObjs();
            var result = false;
            foreach (var key in keies)
            {
                if (_configurables.TryRemove(key, out object obj))
                {
                    result = true;
                    valueObjs.Add(key,new ValueObj(obj));
                }
            }
            return Observable.Return(result);
        }
    }
}
