﻿using Autofac;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.EventExecutor;
using Surging.Core.CPlatform.Module;
using Surging.Core.CPlatform.Network;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.Protocol.Tcp.Runtime;
using Surging.Core.Protocol.Tcp.Runtime.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Tcp
{
    public class TcpProtocolModule : EnginePartModule
    {
        public override void Initialize(AppModuleContext serviceProvider)
        {
            base.Initialize(serviceProvider);
          //  var config =new Dictionary<string, object>();
          //  config.Add("script", @"parser.Fixed(4).Handler(
          //        function(buffer){
          //          var buf = BytesUtils.Slice(buffer,1,4);
          //          parser.Fixed(buffer.ReadableBytes).Result(buf);
          //   }).Handler(
          //          function(buffer){parser.Fixed(8).Result(buffer);}
          //  ).Handler(function(buffer){
          //          parser.Result('处理完成','gb2312').Complete();
          //       }
          //   )");
          //var network=  serviceProvider.ServiceProvoider.GetInstances<INetworkProvider<NetworkProperties>>(NetworkType.Tcp.ToString()).CreateNetwork(new NetworkProperties
          //{
          //    Id="233r",
          //     ParserType = PayloadParserType.Script,
          //     PayloadType = PayloadType.String,
          //     Host = "127.0.0.1",
          //     Port = 322,
          //     ParserConfiguration = config
          // });
          //  network.StartAsync();

          
          //  var network1 = serviceProvider.ServiceProvoider.GetInstances<INetworkProvider<NetworkProperties>>(NetworkType.Tcp.ToString()).CreateNetwork(new NetworkProperties
          //  {
          //      Id = "233rt",
          //      ParserType = PayloadParserType.Script,
          //      PayloadType = PayloadType.String,
          //      Host = "127.0.0.1",
          //      Port = 321,
          //      ParserConfiguration = config
          //  });
          //  network1.StartAsync();
        }

        /// <summary>
        /// Inject dependent third-party components
        /// </summary>
        /// <param name="builder"></param>
        protected override void RegisterBuilder(ContainerBuilderWrapper builder)
        {
            base.RegisterBuilder(builder);
            builder.Register(provider =>
            {
                return new DefaultTcpServiceEntryProvider(
                       provider.Resolve<IServiceEntryProvider>(),
                    provider.Resolve<ILogger<DefaultTcpServiceEntryProvider>>(),
                      provider.Resolve<CPlatformContainer>()
                      );
            }).As(typeof(ITcpServiceEntryProvider)).SingleInstance(); 
            builder.RegisterType(typeof(DefaultDeviceProvider)).As(typeof(IDeviceProvider)).SingleInstance();
            builder.RegisterType(typeof(DefaultTcpServiceEntryProvider)).As(typeof(ITcpServiceEntryProvider)).SingleInstance();
            builder.RegisterType(typeof(TcpNetworkProvider)).Named(NetworkType.Tcp.ToString(), typeof(INetworkProvider<NetworkProperties>)).SingleInstance();
            builder.RegisterType(typeof(TcpClientNetworkProvider)).Named(NetworkType.TcpClient.ToString(), typeof(INetworkProvider<NetworkProperties>)).SingleInstance();
            if (CPlatform.AppConfig.ServerOptions.Protocol == CommunicationProtocol.Tcp)
            {
                RegisterDefaultProtocol(builder);
            }
            else if (CPlatform.AppConfig.ServerOptions.Protocol == CommunicationProtocol.None)
            {
                RegisterTcpProtocol(builder);
            }
        }

        private static void RegisterDefaultProtocol(ContainerBuilderWrapper builder)
        {
            builder.Register(provider =>
            {
                return new DotNettyTcpServerMessageListener(provider.Resolve<ILogger<DotNettyTcpServerMessageListener>>(),
                     "default", provider.Resolve<IEventExecutorProvider>(), provider.Resolve<ITcpServiceEntryProvider>(), new NetworkProperties()
                      );
            }).SingleInstance();
            builder.Register(provider =>
            {
                var messageListener = provider.Resolve<DotNettyTcpServerMessageListener>();
                return new TcpServiceHost(async endPoint =>
                {
                    await messageListener.StartAsync(endPoint);
                    return messageListener;
                });

            }).As<IServiceHost>();
        }

        private static void RegisterTcpProtocol(ContainerBuilderWrapper builder)
        {

            builder.Register(provider =>
            {
                return new DotNettyTcpServerMessageListener(provider.Resolve<ILogger<DotNettyTcpServerMessageListener>>(),
                    
                    "default", provider.Resolve<IEventExecutorProvider>(), provider.Resolve<ITcpServiceEntryProvider>(), new NetworkProperties()
                      );
            }).SingleInstance();
            builder.Register(provider =>
            {
                var messageListener = provider.Resolve<DotNettyTcpServerMessageListener>();
                return new TcpServiceHost(async endPoint =>
                {
                    await messageListener.StartAsync(endPoint);
                    return messageListener;
                });

            }).As<IServiceHost>();
        }
    }
}