using Surging.Core.ProxyGenerator;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Concurrent;
using Newtonsoft.Json.Linq;
using Surging.Core.ApiGateWay.Configurations;

namespace Surging.Core.ApiGateWay.Aggregation
{
    public class ServicePartProvider : IServicePartProvider
    {
        private readonly IServiceProxyProvider _serviceProxyProvider;
        private readonly ConcurrentDictionary<string, ServicePartType> _servicePartTypes =
            new ConcurrentDictionary<string, ServicePartType>();
        public ServicePartProvider(IServiceProxyProvider serviceProxyProvider)
        {
            _serviceProxyProvider = serviceProxyProvider;
        }

        public bool IsPart(string routhPath)
        {
            var servicePart = AppConfig.ServicePart;
            var parts = servicePart.Services;
            if (!_servicePartTypes.TryGetValue(routhPath, out ServicePartType partType))
            {
                if (servicePart.MainPath.Equals(routhPath, StringComparison.OrdinalIgnoreCase))
                {
                    partType = _servicePartTypes.GetOrAdd(routhPath, ServicePartType.Main);
                }
                else if (parts.Any(p => p.UrlMapping.Equals(routhPath, StringComparison.OrdinalIgnoreCase)))
                {
                    partType = _servicePartTypes.GetOrAdd(routhPath, ServicePartType.Section);
                }
            }
            return partType != ServicePartType.None;


        }

        public async Task<object> Merge(string routhPath, Dictionary<string, object> param)
        {
            var partType = _servicePartTypes.GetValueOrDefault(routhPath);
            JObject jObject = new JObject();
            if (partType == ServicePartType.Main)
            {
                param.TryGetValue("ServiceAggregation", out object model);
                var parts = model as JArray;
                foreach (var part in parts)
                {
                    var routeParam = part["Params"].ToObject<Dictionary<string, object>>();
                    var path = part.Value<string>("RoutePath");
                    var serviceKey = part.Value<string>("ServiceKey");
                    var result = await _serviceProxyProvider.Invoke<object>(routeParam, path, serviceKey);
                    jObject.Add(part.Value<string>("Key"), JToken.FromObject(result));
                }
            }
            else
            {
                var service = AppConfig.ServicePart.Services.Where(p => p.UrlMapping.Equals(routhPath, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                foreach (var part in service.serviceAggregation)
                {
                    var result = await _serviceProxyProvider.Invoke<object>(param, part.RoutePath, part.ServiceKey);
                    jObject.Add(part.Key, JToken.FromObject(result));
                };
            }
            return jObject;
        }
    }
}
