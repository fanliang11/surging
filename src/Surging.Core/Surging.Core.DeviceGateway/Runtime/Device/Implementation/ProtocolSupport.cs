using DotNetty.Common.Utilities;
using Microsoft.CodeAnalysis;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Diagnostics;
using Surging.Core.CPlatform.Protocol;
using Surging.Core.CPlatform.Routing;
using Surging.Core.DeviceGateway.Runtime.Core;
using Surging.Core.DeviceGateway.Runtime.Core.Metadata;
using Surging.Core.DeviceGateway.Runtime.Device.MessageCodec;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Device.Implementation
{
    public abstract class ProtocolSupport: IProtocolSupport
    {
        private readonly ConcurrentDictionary<String, Func<IObservable<ConfigMetadata>>> _configMetadatas = new ConcurrentDictionary<string, Func<IObservable<ConfigMetadata>>>(); 
        private readonly ConcurrentDictionary<string, Func<string>> _docFiles = new ConcurrentDictionary<string, Func<string>>();
        private readonly ConcurrentDictionary<string, IAuthenticator> _authenticators=new ConcurrentDictionary<string, IAuthenticator>();
        private readonly ConcurrentDictionary<string, List<ServiceDescriptor>> _routes = new ConcurrentDictionary<string, List<ServiceDescriptor>>();

        public string? Script { get; set; }
        
        public abstract void AddMessageCodecSupport(MessageTransport transportType, Func<IObservable<DeviceMessageCodec>> messageCodec);

        public abstract IObservable<DeviceMessageCodec> GetMessageCodecSupport(string transportType);
         
        public  void AddDocument(MessageTransport transportType,string documentUrlOrFile)
        {
            _docFiles.AddOrUpdate(transportType.ToString(), () => documentUrlOrFile, (key, value) => () => documentUrlOrFile);
        }

        public string? GetDocument(MessageTransport transportType)
        {
            _docFiles.TryGetValue(transportType.ToString(), out Func<string>? document);
            if (document != null)
            {
                return document();
            }
            return default(string);
        }

        public void AddDocument(MessageTransport transportType, Func<string> document)
        {
            _docFiles.AddOrUpdate(transportType.ToString(), document, (key, value) => document);
        }

        public void AddConfigMetadata(MessageTransport transport, Func<IObservable<ConfigMetadata>> metadata)
        {
            _configMetadatas.AddOrUpdate(transport.ToString(), metadata, (key, value) => metadata);
        }

        public void AddAuthenticator(MessageTransport transport, IAuthenticator authenticator)
        {
            _authenticators.AddOrUpdate(transport.ToString(), authenticator, (key, value) => authenticator);
        }

        public IObservable<IAuthenticator> GetAuthenticator(MessageTransport transport)
        {
            _authenticators.TryGetValue(transport.ToString(), out IAuthenticator authenticator);
            if (authenticator != null)
            {
                return Observable.Return(authenticator);
            }
            return Observable.Return<IAuthenticator>(default);
        }

        public void AddRoutes(MessageTransport transport,List<ServiceDescriptor> routes)
        {
            _routes.AddOrUpdate(transport.ToString(), routes, (key, value) => routes);
        }

        public IObservable<List<ServiceDescriptor>> GetRoutes(MessageTransport transport)
        {
            _routes.TryGetValue(transport.ToString(), out List<ServiceDescriptor> routes);
            if (routes != null)
            {
                return Observable.Return(routes);
            }
            return Observable.Return<List<ServiceDescriptor>>(default);
        }

        public void AddConfigMetadata(MessageTransport transport, ConfigMetadata metadata)
        {
            _configMetadatas.AddOrUpdate(transport.ToString(), ()=> Observable.Return(metadata), (key, value) => () => Observable.Return(metadata));
        }

        public IObservable<ConfigMetadata> GetConfigMetadata(MessageTransport transportType)
        {
            _configMetadatas.TryGetValue(transportType.ToString(), out Func<IObservable<ConfigMetadata>>? configMetadata);
            if (configMetadata != null)
            {
                return configMetadata();
            }
            return Observable.Return<ConfigMetadata>(default);
        }
    }
}
