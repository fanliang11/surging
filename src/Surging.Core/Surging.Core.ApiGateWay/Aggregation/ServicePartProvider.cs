using Newtonsoft.Json.Linq;
using Surging.Core.ApiGateWay.Configurations;
using Surging.Core.ProxyGenerator;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.ApiGateWay.Aggregation
{
    /// <summary>
    /// 服务部件提供者
    /// </summary>
    public class ServicePartProvider : IServicePartProvider
    {
        #region 字段

        /// <summary>
        /// Defines the _servicePartTypes
        /// </summary>
        private readonly ConcurrentDictionary<string, ServicePartType> _servicePartTypes =
            new ConcurrentDictionary<string, ServicePartType>();

        /// <summary>
        /// Defines the _serviceProxyProvider
        /// </summary>
        private readonly IServiceProxyProvider _serviceProxyProvider;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ServicePartProvider"/> class.
        /// </summary>
        /// <param name="serviceProxyProvider">The serviceProxyProvider<see cref="IServiceProxyProvider"/></param>
        public ServicePartProvider(IServiceProxyProvider serviceProxyProvider)
        {
            _serviceProxyProvider = serviceProxyProvider;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The IsPart
        /// </summary>
        /// <param name="routhPath">The routhPath<see cref="string"/></param>
        /// <returns>The <see cref="bool"/></returns>
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

        /// <summary>
        /// The Merge
        /// </summary>
        /// <param name="routhPath">The routhPath<see cref="string"/></param>
        /// <param name="param">The param<see cref="Dictionary{string, object}"/></param>
        /// <returns>The <see cref="Task{object}"/></returns>
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

        #endregion 方法
    }
}