using Consul;
using Microsoft.Extensions.Logging;
using Surging.Core.Consul.Configurations;
using Surging.Core.Consul.Utilitys;
using Surging.Core.Consul.WatcherProvider;
using Surging.Core.CPlatform.Runtime.Client;
using Surging.Core.CPlatform.Runtime.Client.Implementation;
using Surging.Core.CPlatform.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Consul
{
  public  class ConsulServiceSubscribeManager: ServiceSubscribeManagerBase, IDisposable
{
        private readonly ConsulClient _consul;
        private readonly ConfigInfo _configInfo;
        private readonly ISerializer<byte[]> _serializer;
        private readonly IServiceSubscriberFactory _serviceSubscriberFactory;
        private readonly ILogger<ConsulServiceSubscribeManager> _logger;
        private readonly ISerializer<string> _stringSerializer;
        private readonly IClientWatchManager _manager;
        private ServiceSubscriber[] _subscribers;

        public ConsulServiceSubscribeManager(ConfigInfo configInfo, ISerializer<byte[]> serializer,
            ISerializer<string> stringSerializer, IClientWatchManager manager, IServiceSubscriberFactory serviceSubscriberFactory,
            ILogger<ConsulServiceSubscribeManager> logger):base(stringSerializer)
        {
            _configInfo = configInfo;
            _serializer = serializer;
            _stringSerializer = stringSerializer;
            _serviceSubscriberFactory = serviceSubscriberFactory;
            _logger = logger;
            _manager = manager;
            _consul = new ConsulClient(config =>
            {
                config.Address = new Uri($"http://{configInfo.Host}:{configInfo.Port}");
            },null, h => { h.UseProxy = false; h.Proxy = null; }); 
            EnterSubscribers().Wait();
        }

        /// <summary>
        /// 获取所有可用的服务订阅者信息。
        /// </summary>
        /// <returns>服务路由集合。</returns>
        public override async Task<IEnumerable<ServiceSubscriber>> GetSubscribersAsync()
        {
            await EnterSubscribers();
            return _subscribers;
        }

        public override async Task ClearAsync()
        {
            var queryResult = await _consul.KV.List(_configInfo.SubscriberPath);
            var response = queryResult.Response;
            if (response != null)
            {
                foreach (var result in response)
                {
                    await _consul.KV.DeleteCAS(result);
                }
            }
        }
        
        public void Dispose()
        {
            _consul.Dispose();
        }

        protected override async Task SetSubscribersAsync(IEnumerable<ServiceSubscriberDescriptor> subscribers)
        {
            subscribers = subscribers.ToArray();
            if (_subscribers != null)
            {
                var oldSubscriberIds = _subscribers.Select(i => i.ServiceDescriptor.Id).ToArray();
                var newSubscriberIds = subscribers.Select(i => i.ServiceDescriptor.Id).ToArray();
                var deletedSubscriberIds = oldSubscriberIds.Except(newSubscriberIds).ToArray();
                foreach (var deletedSubscriberId in deletedSubscriberIds)
                {
                    await _consul.KV.Delete($"{_configInfo.SubscriberPath}{deletedSubscriberId}");
                }
            }
            foreach (var serviceSubscriber in subscribers)
            {
                var nodeData = _serializer.Serialize(serviceSubscriber);
                var keyValuePair = new KVPair($"{_configInfo.SubscriberPath}{serviceSubscriber.ServiceDescriptor.Id}") { Value = nodeData };
                await _consul.KV.Put(keyValuePair);
            }
        }

        public override async Task SetSubscribersAsync(IEnumerable<ServiceSubscriber> subscribers)
        {
            var serviceSubscribers = await GetSubscribers(subscribers.Select(p =>$"{ _configInfo.SubscriberPath }{ p.ServiceDescriptor.Id}"));
            if (serviceSubscribers.Count() > 0)
            {
                foreach (var subscriber in subscribers)
                {
                    var serviceSubscriber = serviceSubscribers.Where(p => p.ServiceDescriptor.Id == subscriber.ServiceDescriptor.Id).FirstOrDefault();
                    if (serviceSubscriber != null)
                    {
                        subscriber.Address = subscriber.Address.Concat(
                            subscriber.Address.Except(serviceSubscriber.Address));
                    }
                }
            }
            await base.SetSubscribersAsync(subscribers);
        }

        private async Task<ServiceSubscriber> GetSubscriber(byte[] data)
        {
            if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
                _logger.LogDebug($"准备转换服务订阅者，配置内容：{Encoding.UTF8.GetString(data)}。");

            if (data == null)
                return null;

            var descriptor = _serializer.Deserialize<byte[], ServiceSubscriberDescriptor>(data);
            return (await _serviceSubscriberFactory.CreateServiceSubscribersAsync(new[] { descriptor })).First();
        }

        private async Task EnterSubscribers()
        { 
            if (_subscribers != null)
                return;
            if (_consul.KV.Keys(_configInfo.SubscriberPath).Result.Response?.Count() > 0)
            {
                var result = await _consul.GetChildrenAsync(_configInfo.SubscriberPath);
                var keys = await _consul.KV.Keys(_configInfo.SubscriberPath);
                var childrens = result;
                _subscribers = await GetSubscribers(keys.Response);
            }
            else
            {
                if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Warning))
                    _logger.LogWarning($"无法获取订阅者信息，因为节点：{_configInfo.SubscriberPath}，不存在。");
                _subscribers = new ServiceSubscriber[0];
            }
        }

        private async Task<ServiceSubscriber> GetSubscriber(string path)
        {
            ServiceSubscriber result = null;
            var queryResult = await _consul.KV.Keys(path);
            if (queryResult.Response != null)
            {
                var data = (await _consul.GetDataAsync(path));
                if (data != null)
                {
                    result = await GetSubscriber(data);
                }
            }
            return result;
        }

        private async Task<ServiceSubscriber[]> GetSubscribers(IEnumerable<string> childrens)
        {
            childrens = childrens.ToArray();
            var subscribers = new List<ServiceSubscriber>(childrens.Count());
            foreach (var children in childrens)
            {
                if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
                    _logger.LogDebug($"准备从节点：{children}中获取订阅者信息。");
                
                var subscriber = await GetSubscriber(children);
                if (subscriber != null)
                    subscribers.Add(subscriber);
            }
            return subscribers.ToArray();
        }
    }
}
