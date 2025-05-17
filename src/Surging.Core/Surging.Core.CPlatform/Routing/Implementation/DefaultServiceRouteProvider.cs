﻿using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Routing.Implementation;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Transport.Implementation;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Routing.Implementation
{
    public class DefaultServiceRouteProvider : IServiceRouteProvider
    {
        private readonly ConcurrentDictionary<string, ServiceRoute> _concurrent =
       new ConcurrentDictionary<string, ServiceRoute>();

        private readonly List<ServiceRoute> _localRoutes = new List<ServiceRoute>();

        private readonly ConcurrentDictionary<string, ServiceRoute> _serviceRoute =
       new ConcurrentDictionary<string, ServiceRoute>();

        private readonly IServiceEntryManager _serviceEntryManager;
        private readonly ILogger<DefaultServiceRouteProvider> _logger;
        private readonly IServiceRouteManager _serviceRouteManager;
        private readonly IServiceTokenGenerator _serviceTokenGenerator;
        public DefaultServiceRouteProvider(IServiceRouteManager serviceRouteManager, ILogger<DefaultServiceRouteProvider> logger,
            IServiceEntryManager serviceEntryManager, IServiceTokenGenerator serviceTokenGenerator)
        {
            _serviceRouteManager = serviceRouteManager;
            serviceRouteManager.Changed += ServiceRouteManager_Removed;
            serviceRouteManager.Removed += ServiceRouteManager_Removed;
            serviceRouteManager.Created += ServiceRouteManager_Add;
            _serviceEntryManager = serviceEntryManager;
            _serviceTokenGenerator = serviceTokenGenerator;
            _logger = logger;
        }

        public async Task<ServiceRoute> Locate(string serviceId)
        {
            _concurrent.TryGetValue(serviceId, out ServiceRoute route);
            if (route == null)
            {
                var routes = await _serviceRouteManager.GetRoutesAsync();
                route = routes.FirstOrDefault(i => i.ServiceDescriptor.Id == serviceId);
                if (route == null)
                {
                    if (_logger.IsEnabled(LogLevel.Warning))
                        _logger.LogWarning($"根据服务id：{serviceId}，找不到相关服务信息。");
                }
                else
                    _concurrent.GetOrAdd(serviceId, route);
            }
            return route;
        }

        public Task<ServiceRoute> GetLocalRouteByPathRegex(string path)
        {
            var addess = NetUtils.GetHostAddress();

            if (_localRoutes.Count == 0)
            {
                _localRoutes.AddRange(_serviceEntryManager.GetEntries().Select(i =>
                {
                    i.Descriptor.Token = _serviceTokenGenerator.GetToken();
                    return new ServiceRoute
                    {
                        Address = new[] { addess },
                        ServiceDescriptor = i.Descriptor
                    };
                }).ToList());
            }
            path = path.ToLower();
            _serviceRoute.TryGetValue(path, out ServiceRoute route);
            if (route == null)
            {
                return GetRouteByPathRegexAsync(_localRoutes, path);
            }
            else
            {
                return Task.FromResult(route);
            }
        }

        public Task<ServiceRoute> GetRouteByPath(string path)
        {
            _serviceRoute.TryGetValue(path.ToLower(), out ServiceRoute route);
            if (route == null)
            {
                return GetRouteByPathAsync(path);
            }
            else
            {
                return Task.FromResult(route);
            }
        }

        public async Task<ServiceRoute> GetRouteByPathRegex(string path)
        {
            path = path.ToLower();
            _serviceRoute.TryGetValue(path, out ServiceRoute route);
            if (route == null)
            {
                var routes = await _serviceRouteManager.GetRoutesAsync();
                return await GetRouteByPathRegexAsync(routes, path);
            }
            else
            {
                return route;
            }
        }

        public async Task<ServiceRoute> SearchRoute(string path)
        {
            return await SearchRouteAsync(path);
        }

        public async Task RegisterRoutes(decimal processorTime)
        {
            var addess = NetUtils.GetHostAddress();
            addess.ProcessorTime = processorTime;
            addess.Weight = AppConfig.ServerOptions.Weight;
            if (addess.Weight > 0)
                addess.Timestamp = DateTimeConverter.DateTimeToUnixTimestamp(DateTime.Now);
            RpcContext.GetContext().SetAttachment("Host", addess);
            var addressDescriptors = _serviceEntryManager.GetEntries().Select(i =>
            {
                i.Descriptor.Token = _serviceTokenGenerator.GetToken();
                return new ServiceRoute
                {
                    Address = new[] { addess },
                    ServiceDescriptor = i.Descriptor
                };
            }).ToList();
            await _serviceRouteManager.SetRoutesAsync(addressDescriptors);
        }

        #region 私有方法
        private static string GetCacheKey(ServiceDescriptor descriptor)
        {
            return descriptor.Id;
        }

        private void ServiceRouteManager_Removed(object sender, ServiceRouteEventArgs e)
        {
            var key = GetCacheKey(e.Route.ServiceDescriptor);
            ServiceRoute value;
            _concurrent.TryRemove(key, out value);
            _serviceRoute.TryRemove(e.Route.ServiceDescriptor.RoutePath, out value);
        }

        private void ServiceRouteManager_Add(object sender, ServiceRouteEventArgs e)
        {
            var key = GetCacheKey(e.Route.ServiceDescriptor);
            _concurrent.GetOrAdd(key, e.Route);
            _serviceRoute.GetOrAdd(e.Route.ServiceDescriptor.RoutePath, e.Route);
        }

        private async Task<ServiceRoute> SearchRouteAsync(string path)
        {
            var routes = await _serviceRouteManager.GetRoutesAsync();
            var route = routes.FirstOrDefault(i => String.Compare(i.ServiceDescriptor.RoutePath, path, true) == 0);
            if (route == null)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.LogWarning($"根据服务路由路径：{path}，找不到相关服务信息。");
            }
            else
                _serviceRoute.GetOrAdd(path, route);
            return route;
        }

        private async Task<ServiceRoute> GetRouteByPathAsync(string path)
        {
            var routes = await _serviceRouteManager.GetRoutesAsync();
            var route = routes.FirstOrDefault(i => String.Compare(i.ServiceDescriptor.RoutePath, path, true) == 0 && !i.ServiceDescriptor.GetMetadata<bool>("IsOverload"));
            if (route == null)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.LogWarning($"根据服务路由路径：{path}，找不到相关服务信息。");
            }
            else
                _serviceRoute.GetOrAdd(path, route);
            return route;
        }

        private async Task<ServiceRoute> GetRouteByPathRegexAsync(IEnumerable<ServiceRoute> routes, string path)
        {
            var pattern = "/{.*?}";

            var route = routes.FirstOrDefault(i =>
            {
                var routePath = Regex.Replace(i.ServiceDescriptor.RoutePath, pattern, "");
                var newPath = path.Replace(routePath, "");
                return (newPath.StartsWith("/") || newPath.Length == 0) && i.ServiceDescriptor.RoutePath.Split("/").Length == path.Split("/").Length && !i.ServiceDescriptor.GetMetadata<bool>("IsOverload");
            });


            if (route == null)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.LogWarning($"根据服务路由路径：{path}，找不到相关服务信息。");
            }
            else
              if (!Regex.IsMatch(route.ServiceDescriptor.RoutePath, pattern)) _serviceRoute.GetOrAdd(path, route);
            return await Task.FromResult(route);
        }

        public void ResetLocalRoute()
        {
            _localRoutes.Clear();
        }

        #endregion
    }
}