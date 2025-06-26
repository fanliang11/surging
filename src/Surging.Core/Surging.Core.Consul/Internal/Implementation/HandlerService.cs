using Newtonsoft.Json.Linq;
using Surging.Core.Consul.Configurations;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Cache;
using Surging.Core.CPlatform.Ioc;
using Surging.Core.CPlatform.Mqtt;
using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Serialization;
using Surging.Core.CPlatform.Support;
using Surging.Core.CPlatform.Transport.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Consul.Internal.Implementation
{
    public class HandlerService : ServiceBase, IHandlerService
    {
        private readonly ConfigInfo _configInfo;
        private readonly ISerializer<string> _stringSerializer;
        private readonly ConsulServiceRouteManager _consulServiceRouteManager;
        private readonly ConsulServiceCommandManager _consulServiceCommandManager;
        private readonly ConsulMqttServiceRouteManager _consulMqttServiceRouteManager;
        private readonly ConsulServiceCacheManager _consulServiceCacheManager;
        public HandlerService(ConfigInfo configInfo, ISerializer<string> stringSerializer, CPlatformContainer serviceProvider)
        {
            _configInfo = configInfo;
            _stringSerializer = stringSerializer;
            _consulServiceRouteManager = serviceProvider.GetInstances<IServiceRouteManager>() as ConsulServiceRouteManager;
            _consulServiceCommandManager = serviceProvider.GetInstances<IServiceCommandManager>() as ConsulServiceCommandManager;
            _consulMqttServiceRouteManager= serviceProvider.GetInstances<IMqttServiceRouteManager>() as ConsulMqttServiceRouteManager;
            _consulServiceCacheManager = serviceProvider.GetInstances<IServiceCacheManager>() as ConsulServiceCacheManager;
        }
        public async Task<bool> KeyPrefixWatch()
        {

           var body= RpcContext.GetContext().GetAttachment("requset.body") as JArray;  
            #region route
            var routes= body.Where(p =>p["Key"].ToString().Contains(_configInfo.RoutePath,StringComparison.OrdinalIgnoreCase)).ToList();
            var values = routes.Select(p => new { key = p["Key"].ToString(),value = Base64Decode(p["Value"].ToString()) }).ToDictionary(p=>p.key,p=>p.value);
            await _consulServiceRouteManager.ChildrenChange(values);
            #endregion 
            #region serviceCommand
            var commands = body.Where(p => p["Key"].ToString().Contains(_configInfo.CommandPath,StringComparison.OrdinalIgnoreCase)).ToList();
            var commandValues = commands.Select(p => new { key = p["Key"].ToString(), value = Base64Decode(p["Value"].ToString()) }).ToDictionary(p => p.key, p => p.value);
            _consulServiceCommandManager.ChildrenChange(commandValues);
            #endregion
 
            #region mqttRoute
            var mqttRoutes = body.Where(p => p["Key"].ToString().Contains(_configInfo.MqttRoutePath, StringComparison.OrdinalIgnoreCase)).ToList();
            var mqttRouteValues = mqttRoutes.Select(p => new { key = p["Key"].ToString(), value = Base64Decode(p["Value"].ToString()) }).ToDictionary(p => p.key, p => p.value);
            await _consulMqttServiceRouteManager.ChildrenChange(mqttRouteValues);
            #endregion 

            #region ServiceCache
            var serviceCaches = body.Where(p => p["Key"].ToString().Contains(_configInfo.CachePath, StringComparison.OrdinalIgnoreCase)).ToList();
            var serviceCacheValues = serviceCaches.Select(p => new { key = p["Key"].ToString(), value = Base64Decode(p["Value"].ToString()) }).ToDictionary(p => p.key, p => p.value); ;
            await _consulServiceCacheManager.ChildrenChange(serviceCacheValues);
            #endregion
            return true;
        }

        public byte[] Base64Decode(string Message)
        {
            byte[] bytes = Convert.FromBase64String(Message);
            return bytes;
        }

        public async Task<bool> KeyWatch(string Key, string Value)
        { 
            #region route
            if (Key.Contains(_configInfo.RoutePath, StringComparison.OrdinalIgnoreCase))
            {
                var value = Base64Decode(Value);
                await _consulServiceRouteManager.NodeChange(value);
            }

            #endregion
            #region serviceCommand
            if (Key.Contains(_configInfo.CommandPath, StringComparison.OrdinalIgnoreCase))
            {
                var commandValue = Base64Decode(Value);
                _consulServiceCommandManager.NodeChange(commandValue);

            }
            #endregion

            #region mqttRoute
            if (Key.ToString().Contains(_configInfo.MqttRoutePath, StringComparison.OrdinalIgnoreCase))
            {
                var mqttRouteValue = Base64Decode(Value);
                await _consulMqttServiceRouteManager.NodeChange(mqttRouteValue);
            }
            #endregion

            #region ServiceCache
            if (Key.ToString().Contains(_configInfo.CachePath, StringComparison.OrdinalIgnoreCase))
            {
                var serviceCacheValue = Base64Decode(Value);
                await _consulServiceCacheManager.NodeChange(serviceCacheValue);
            }
            #endregion
            return true;
        }
    }
}
