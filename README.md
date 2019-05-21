# surging 　　　　　　　　　　　　　　　　　　　　[English](https://github.com/dotnetcore/surging/blob/master/README.EN.md)
[![Member project of .NET Core Community](https://img.shields.io/badge/member%20project%20of-NCC-9e20c9.svg)](https://github.com/dotnetcore)
[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://mit-license.org/)
### surging 是一个分布式微服务框架,提供高性能RPC远程服务调用，采用Zookeeper、Consul作为surging服务的注册中心，集成了哈希，随机，轮询，压力最小优先作为负载均衡的算法，RPC集成采用的是netty框架，采用异步传输。

<br />

### 名字由来

英文名：surging

中文名：滔滔

中文名来自周星驰的经典台词

我对阁下的景仰犹如滔滔江水,连绵不绝,犹如黄河泛滥,一发而不可收拾，而取名英文的含义也希望此框架能流行起来，也能像《.net core surging》这句英文语句含义一样，.net core技术风起云涌,冲击整个软件生态系统。

### 微服务定义
微服务应该是可以自由组合拆分，对于每个业务都是独立的，针对于业务模块的 CRUD 可以注册为服务，而每个服务都是高度自治的，从开发，部署都是独立，而每个服务只做单一功能，利用领域驱动设计去更好的拆分成粒度更小的模块

### 能做什么
1.简化的服务调用，通过服务规则的指定，就可以做到服务之间的远程调用，无需其它方式的侵入

2.服务自动注册与发现，不需要配置服务提供方地址，注册中心基于ServiceId 或者RoutePath查询服务提供者的地址和元数据，并且能够平滑添加或删除服务提供者。

3.软负载均衡及容错机制，通过surging内部负载算法和容错规则的设定，从而达到内部调用的负载和容错

4.分布式缓存中间件：通过哈希一致性算法来实现负载，并且有健康检查能够平滑的把不健康的服务从列表中删除

5. 事件总线：通过对于事件总线的适配可以实现发布订阅交互模式

6.容器化持续集成与持续交付 ：通过构建一体化Devops平台,实现项目的自动化构建、部署、测试和发布，从而提高生产环境的可靠性、稳定性、弹性和安全性。

7. 业务模块化驱动引擎，通过加载指定业务模块，能够更加灵活、高效的部署不同版本的业务功能模块

### 引擎如何安装

docker hub : docker pull serviceengine/surging:版本号

nuget:Install-Package surging -Version  版本号

### surging模块功能

<img src="https://github.com/dotnetcore/surging/blob/master/docs/SurgingFunction.png" alt="surging模块功能" />

### 配置：

 ```c#
var host = new ServiceHostBuilder()
                .RegisterServices(builder =>
                {
                    builder.AddMicroService(option =>
                    {
                        option.AddServiceRuntime();//
                        option.AddRelateService();//添加支持服务代理远程调用
                         option.AddConfigurationWatch();//添加同步更新配置文件的监听处理
                        // option.UseZooKeeperManager(new ConfigInfo("127.0.0.1:2181")); //使用Zookeeper管理
                        option.UseConsulManager(new ConfigInfo("127.0.0.1:8500"));//使用Consul管理
                        option.UseDotNettyTransport();//使用Netty传输
                        option.UseRabbitMQTransport();//使用rabbitmq 传输
                        option.AddRabbitMQAdapt();//基于rabbitmq的消费的服务适配
                      //  option.UseProtoBufferCodec();//基于protobuf序列化
                        option.UseMessagePackCodec();//基于MessagePack序列化
                        builder.Register(p => new CPlatformContainer(ServiceLocator.Current));//初始化注入容器
                    });
                })
                .SubscribeAt()     //消息订阅
              //.UseServer("127.0.0.1", 98)
              //.UseServer("127.0.0.1", 98，“true”) //自动生成Token
              //.UseServer("127.0.0.1", 98，“123456789”) //固定密码Token
                .UseServer(options=> {
                    options.Ip = "127.0.0.1";
                    options.Port = 98;
                    //options.IpEndpoint = new IPEndPoint(IPAddress.Any, 98);
                    //options.Ip = "0.0.0.0";
                    options.ExecutionTimeoutInMilliseconds = 30000; //执行超时时间
                    options.Strategy=(int)StrategyType.Failover; //容错策略使用故障切换
                    options.RequestCacheEnabled=true; //开启缓存（只有通过接口代理远程调用，才能启用缓存）
                    options.Injection="return null"; //注入方式
                    options.InjectionNamespaces= new string[] { "Surging.IModuleServices.Common" }); //脚本注入使用的命名空间
                    options.BreakeErrorThresholdPercentage=50;  //错误率达到多少开启熔断保护
                    options.BreakeSleepWindowInMilliseconds=60000; //熔断多少毫秒后去尝试请求
                    options.BreakerForceClosed=false;   //是否强制关闭熔断
                    options.BreakerRequestVolumeThreshold = 20;//10秒钟内至少多少请求失败，熔断器才发挥起作用
                    options.MaxConcurrentRequests=100000;//支持最大并发
                    options.ShuntStrategy=AddressSelectorMode.Polling; //使用轮询负载分流策略
                    options.NotRelatedAssemblyFiles = "Centa.Agency.Application.DTO\\w*|StackExchange.Redis\\w*"; //排除无需依赖注册
                })
                //.UseLog4net("Configs/log4net.config") //使用log4net记录日志
                .UseNLog(LogLevel.Error, "Configs/NLog.config")// 使用NLog 记录日志
                //.UseLog4net(LogLevel.Error) //使用log4net记录日志
                //.UseLog4net()  //使用log4net记录日志
                .Configure(build =>
                build.AddEventBusFile("eventBusSettings.json", optional: false))//使用eventBusSettings.json文件进行配置
                .Configure(build =>
                 build.AddCacheFile("cacheSettings.json", optional: false))//使用cacheSettings.json文件进行配置
                .UseProxy() //使用Proxy
                .UseStartup<Startup>()
                .Build();
                
            using (host.Run())
            {
                Console.WriteLine($"服务端启动成功，{DateTime.Now}。");
            }
 ```    
<br/>

### 文件配置：

```c#
{
  "ConnectionString": "${Register_Conn}|127.0.0.1:8500", // ${环境变量名} |默认值,
  "SessionTimeout": "${Register_SessionTimeout}|50",
  "ReloadOnChange": true
}

```

### 非容器环境文件配置

```c#
 {
  "Ip": "${Server_IP}|127.0.0.1",
  "WatchInterval": 30,
  "Port": "${Server_port}",
  "Token": "true",
   "Protocol": "${Protocol}|Tcp", //支持Http,Tcp协议
  "RootPath": "${RootPath}",
  "RequestCacheEnabled": false
}

```


### 容器环境文件配置

```c#
 {
  "Ip": "${Server_IP}|0.0.0.0",//私有容器IP
  "WatchInterval": 30,
  "Port": "${Server_port}|98",//私有容器端口
   "MappingIp": "${Mapping_ip}",//公开主机IP
  "MappingPort": "${Mapping_Port}",//公开主机端口
   "Protocol": "${Protocol}|Tcp", //支持Http,Tcp协议
  "Token": "true",
  "RootPath": "${RootPath}",
  "RequestCacheEnabled": false
}

```


服务路由访问配置：
<br/>

```c#
[ServiceBundle("api/{Service}")]
 ```    
<br/>

JWT验证，接口方法添加以下特性：
<br/>

```c#
   [Authorization(AuthType = AuthorizationType.JWT)];
 ```    
<br/>

AppSecret验证，接口方法添加以下特性：
<br/>

```c#
 [Authorization(AuthType = AuthorizationType.AppSecret)];
 ```    
<br/>

订阅功能：
<br/>

```c#
 ServiceLocator.GetService< ISubscriptionAdapt >().SubscribeAt();
 ```    
 
 <br/>
增加服务容错、服务容错降级、服务强制降级


* 增加容错策略Injection，脚本注入：

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


* 增加容错策略Injection，本地模块注入：   

<br/>

```C#  
[Command(Strategy= StrategyType.Injection ,Injection = @"return true;")] 
```

<br/>

增加缓存降级，怎么使用？
<br/>
在业务接口方法上添加如下特性
<br/>

```C#  
   [Command(Strategy= StrategyType.Failover,FailoverCluster =3,RequestCacheEnabled =true)]  //RequestCacheEnabled =true 就是启用缓存
```

<br/>
怎么拦截获取缓存
 <br/>
在业务接口方法上添加如下特性
 <br/>
 
```C#  
 [InterceptMethod(CachingMethod.Get, Key = "GetUser_id_{0}", Mode = CacheTargetType.Redis, Time = 480)]
```
    
<br/>
怎么拦截删除缓存
 <br/>
在业务接口方法上添加如下特性
 <br/>
 
```C#  
  [InterceptMethod(CachingMethod.Remove, "GetUser_id_{0}", "GetUserName_name_{0}", Mode = CacheTargetType.Redis)]
```
      
<br/>
怎么添加缓存KEY
   <br/>
在业务模型属性上添加，如下特性，可以支持多个
   <br/>
   
```C# 
[CacheKey(1)]
```
        
<br/>
配置拦截器
<br/>
   
```C# 
 .AddClientIntercepted(typeof(CacheProviderInterceptor))
```
### 捐赠基金
如果觉得这个框架不错，可以支持surging开源，请fanly喝一杯咖啡或吃一顿午餐或者是更好的社区发展，扫描下方二维码进行捐赠，并在付款说明填写您的改进意见。

![](https://github.com/dotnetcore/surging/blob/master/%E6%8D%90%E8%B5%A0.png)


## 捐赠明细

surging 接受来自社区的捐赠，所有款项将通过 [捐赠明细表](Statement-of-Income-and-Expense.md) 进行公示，接受社区监督。

IDE:Visual Studio 2017 15.5,vscode
<br/>
框架：.NET core 2.1
<br/>
QQ群：615562965
* [文档](http://docs.dotnet-china.org/surging/)
* [简单示例](https://github.com/dotnetcore/surging/blob/master/docs/docs.en/INDEX.md)

## 谁在使用


