using Jint.Native;
using Newtonsoft.Json.Linq;
using Surging.Core.DeviceGateway.Runtime.Core.Implementation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Core
{
    public interface IConfigurable
    {
       
        IObservable<ValueObj> GetConfig(string key);
         
        IObservable<bool> SetConfig(string key, object value);

        IObservable<bool> SetConfig(KeyValuePair<string, object> keyValue);

        IObservable<bool> SetConfigs(Dictionary<string, object> configs);

        IObservable<ValueObjs> GetConfigs(params string[] keies);

       IObservable<bool> TryRemoveConfig(string key,out ValueObj valueObj);

        IObservable<bool> RemoveConfig(string key);

        IObservable<bool> RemoveConfigs(string[] keies);

        IObservable<bool> TryRemoveConfigs(string[] keies,out ValueObjs valueObjs);
    }
}
