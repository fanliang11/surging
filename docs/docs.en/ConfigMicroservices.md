surging is a distributed micro service framework that provides high-performance RPC remote service calls, using Zookeeper, Consul as the registration center for surging services, integrating hash, random, polling as a load balancing algorithm, RPC integration using the netty framework, Using asynchronous transmission. Use json.net, protobuf, messagepack for serialization Codec

Server how to configure
=============

Add the following configuration to main program "main":
```c#
//If do not add UseProtoBufferCodec or UseMessagePackCodec, the default json.net
var host = new ServiceHostBuilder()
            .RegisterServices(builder =>
            {
                builder.AddMicroService(option =>
                {
                    option.AddServiceRuntime();//
                    //option.UseZooKeeperManager(new ConfigInfo("127.0.0.1:2181"));//Use Zookeeper Manage
                    option.UseConsulManager(new ConfigInfo("127.0.0.1:8500"));//Use Consul Manage
                    option.UseDotNettyTransport();//Use DotNetty Transport
                    option.UseRabbitMQTransport();//Use Rabbitmq Transport
		    option.AddRabbitMQAdapt();//Based on rabbitmq consumer service  adapter
                    //option.UseProtoBufferCodec();//Based on protobuf serialization codec
                    option.UseMessagePackCodec();//Based on messagepack serialization codec
                    builder.Register(p => new CPlatformContainer(ServiceLocator.Current));//Initialize the injection container
                });
            })
	    .SubscribeAt() //News subscription
            .UseServer("127.0.0.1", 98)
	  //.UseServer("127.0.0.1", 98，“true”) //Token automatically generated
	  //.UseServer("127.0.0.1", 98，“123456789”) //Fixed password token
	    .UseLog4net("Configs/log4net.config") //Use log4net to generate the log
            .UseLog4net()  //Use log4net to generate the log
            .UseStartup<Startup>()
            .Build();
               
 	    using (host.Run())
            {
              	Console.WriteLine($"服务端启动成功，{DateTime.Now}。");
            }

```
Client how to configure
=============
```c#
//If do not add UseProtoBufferCodec or UseMessagePackCodec, the default json.net
var host = new ServiceHostBuilder()
            .RegisterServices(builder =>
            {
                builder.AddMicroService(option =>
                {
                    option.AddClient();
                    option.AddClientIntercepted(typeof(CacheProviderInterceptor)); //Set the cache interceptor "CacheProviderInterceptor"
                    //option.UseZooKeeperManager(new ConfigInfo("127.0.0.1:2181"));//Use Zookeeper Manage
                    option.UseConsulManager(new ConfigInfo("127.0.0.1:8500"));//Use Consul Manage
                    option.UseDotNettyTransport();//Use DotNetty Transport
                    option.UseRabbitMQTransport();//Use Rabbitmq Transport
                    //option.UseProtoBufferCodec();//Based on protobuf serialization codec
                    option.UseMessagePackCodec();//Based on messagepack serialization codec
                    builder.Register(p => new CPlatformContainer(ServiceLocator.Current));//Initialize the injection container
                });
            })
            .UseClient()
	    .UseLog4net("Configs/log4net.config") //Use log4net to generate the log
            .UseLog4net()  //Use log4net to generate the log
            .UseStartup<Startup>()
            .Build();

            using (host.Run())
            {
              
            }
```


## Next steps

* [How to configure service routing address mapping]()
* [How to configure authentication]()
