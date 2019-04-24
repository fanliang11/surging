# surging 　　　　　　　　　　　　　　　　　　　　[中文](https://github.com/dotnetcore/surging/blob/master/README.md)
[![Member project of .NET Core Community](https://img.shields.io/badge/member%20project%20of-NCC-9e20c9.svg)](https://github.com/dotnetcore)
[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://mit-license.org/)
# surging is a distributed micro service framework that provides high-performance RPC remote service calls, using Zookeeper, Consul as the registration center for surging services, integrating hash, random, polling as a load balancing algorithm, RPC integration using the netty framework, Using asynchronous transmission.
<br />

Start configuration：

 <br/>
 
 ```c#
var host = new ServiceHostBuilder()
                .RegisterServices(builder =>
                {
                    builder.AddMicroService(option =>
                    {
                        option.AddServiceRuntime();//
                        // option.UseZooKeeperManager(new ConfigInfo("127.0.0.1:2181")); //Using a Zookeeper management
                        option.UseConsulManager(new ConfigInfo("127.0.0.1:8500"));//Use the Consul management
                        option.UseDotNettyTransport();//Use Netty transmission
                        option.UseRabbitMQTransport();//Use the rabbitmq transmission
                        option.AddRabbitMQAdapt();//Based on the consumption of the rabbitmq service adaptation
                        builder.Register(p => new CPlatformContainer(ServiceLocator.Current));//Initializes the injection container
                    });
                })
                .SubscribeAt()     //News Feeds
                .UseServer("127.0.0.1", 98)
              //.UseServer("127.0.0.1", 98，“true”) //Automatically generate Token
              //.UseServer("127.0.0.1", 98，“123456789”) //Fixed password Token
                .UseLog4net("Configs/log4net.config") //Use log4net to generate the log
                .UseLog4net()  //Use log4net to generate the log
                .UseStartup<Startup>()
                .Build();
                
            using (host.Run())
            {
                Console.WriteLine($"The server startup success，{DateTime.Now}。");
            }
 ```    
                
<br/>

Subscription function：
<br/>

```c#
 ServiceLocator.GetService< ISubscriptionAdapt >().SubscribeAt();
 ```    
 
 <br/>
Increase service fault-tolerant, fault-tolerant forcibly demote demotion and service


* Increase the fault tolerance strategy Injection, the local module injection：

<br/>

```c#
[Command(Strategy= StrategyType.Injection ,Injection = @"return null;")]
```    

 <br/>
 
```C#  
[Command(Strategy= StrategyType.Injection ,Injection = @"return 
Task.FromResult(new Surging.IModuleServices.Common.Models.UserModel
         {
            Name=""fanly"",
            Age=18
         });",InjectionNamespaces =new string[] { "Surging.IModuleServices.Common"})] 
```


* Increase Injection fault-tolerant strategy, local Injection module：   

<br/>

```C#  
[Command(Strategy= StrategyType.Injection ,Injection = @"return true;")] 
```

<br/>

Increase the cache relegation, how to use?
<br/>
Add the following features in the business interface methods
<br/>

```C#  
   [Command(Strategy= StrategyType.Failover,FailoverCluster =3,RequestCacheEnabled =true)]  //RequestCacheEnabled =true Is to enable the cache
```

<br/>
How to intercept access to cache？
 <br/>
Add the following features in the business interface methods
 <br/>
 
```C#  
 [InterceptMethod(CachingMethod.Get, Key = "GetUser_id_{0}", Mode = CacheTargetType.Redis, Time = 480)]
```
    
<br/>
How to intercept the delete cache？
 <br/>
Add the following features in the business interface methods
 <br/>
 
```C#  
  [InterceptMethod(CachingMethod.Remove, "GetUser_id_{0}", "GetUserName_name_{0}", Mode = CacheTargetType.Redis)]
```
      
<br/>
How to add the cache KEY
   <br/>
On the business model attribute to add, the following features, can support multiple
   <br/>
   
```C# 
[CacheKey(1)]
```
        
<br/>
Configuring Interceptors
<br/>
   
```C# 
 .AddClientIntercepted(typeof(CacheProviderInterceptor))
```

IDE:Visual Studio 2017 15.3 Preview ,vscode
<br/>
The framework：.NET core 2.0
<br/>
如有任何问题可以加入QQ群：542283494 Gitter:not room
<br/>
[Blog]:https://www.cnblogs.com/fanliang11
