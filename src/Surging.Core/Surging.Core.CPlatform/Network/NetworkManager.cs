using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace Surging.Core.CPlatform.Network
{
    public class NetworkManager:INetworkManager
    {
        private readonly ConcurrentDictionary<string, NetworkProperties> _networkProperties = new ConcurrentDictionary<string, NetworkProperties>();
        private readonly  ConcurrentDictionary<string, INetwork> _network = new ConcurrentDictionary<string, INetwork>();

        private readonly ConcurrentDictionary<string, INetworkProvider<NetworkProperties>> _networkProvider = new ConcurrentDictionary<string, INetworkProvider<NetworkProperties>>();
        public void Destroy(NetworkType type, string id)
        {
            _networkProvider.TryRemove(type.ToString(), out INetworkProvider<NetworkProperties> provider);
            provider.Shutdown(id);
            
        }

        public IObservable<INetwork> GetNetwork(NetworkType type, string id)
        {
            if (_network.TryGetValue(id.ToString(), out INetwork network))
                return Observable.Return(network);

            return Observable.Empty<INetwork>();
        }

        public ReplaySubject<List<INetwork>> GetNetworks()
        {
            var result = new ReplaySubject<List<INetwork>>();
            result.OnNext(_network.Values.ToList());
            return result;
        }

        public INetworkProvider<NetworkProperties> GetProvider(string type)
        {
            if (_networkProvider.TryGetValue(type.ToString(), out INetworkProvider<NetworkProperties> provider))
              return provider;
            return default;
        }

        public List<INetworkProvider<NetworkProperties>> GetProviders()
        {
            return _networkProvider.Values.ToList();
        }

        public IObservable<INetwork> CreateOrUpdate(INetworkProvider<NetworkProperties> provider, NetworkProperties properties, ISubject<NetworkLogMessage> subject)
        {
            _network.TryGetValue(properties.Id, out INetwork network);
            _networkProvider.AddOrUpdate(provider.GetNetworkType().ToString(), key => provider, (key, value) => provider);
            _networkProperties.AddOrUpdate(properties.Id, key => properties, (key, value) => properties);
            return Observable.Return(_network.AddOrUpdate(properties.Id, key =>
            {
                 var result =provider.CreateNetwork(properties,subject);
                result.StartAsync();
                return result;
            }, (key, value) =>
            {
                INetwork network = null;
                if (value == null)
                {
                    network = provider.CreateNetwork(properties, subject);
                    network.StartAsync();
                    return network;
                }
                provider.ReloadAsync(properties); 
                return value;
            }));
        }

        public void Reload(NetworkType type, string id)
        {
            if (_networkProvider.TryGetValue(type.ToString(), out INetworkProvider<NetworkProperties> provider))
            {
                _networkProperties.TryGetValue(id, out NetworkProperties value);
                provider.ReloadAsync(value);
            }
        }

        public void Shutdown(NetworkType type, string id)
        {
            if (_networkProvider.TryGetValue(type.ToString(), out INetworkProvider<NetworkProperties> provider))
            {
                provider.Shutdown(id);
                _network.TryRemove(id.ToString(),out _);
            } 
        } 
    }
}
