The DI framework used for surging is IServiceCollection and autofac, what to do if you need to inject other components in the project. Here are three ways to inject third-party components.

1.How to injection
=============

Use the RegisterServices method of the ServiceHostBuilder class
```c#
//If do not add UseProtoBufferCodec or UseMessagePackCodec, the default json.net
 var host = new ServiceHostBuilder()
	.RegisterServices(builder =>
	{
	    builder.RegisterGeneric(typeof(MongoRepository<>)).As(typeof(IMongoRepository<>)).SingleInstance();
	})；
``` 

2.How to injection
=============
Through ServiceHostBuilder configuration
```c#
.UseStartup<Startup>
``` 

Startup：
```c#
  public IContainer ConfigureServices(ContainerBuilder builder)
  {
    var services = new ServiceCollection();
    ConfigureLogging(services);
    builder.Populate(services);
    builder.RegisterGeneric(typeof(MongoRepository<>)).As(typeof(IMongoRepository<>)).SingleInstance();
    ServiceLocator.Current = builder.Build();
    return ServiceLocator.Current;
  }
``` 
3.How to injection（Recommended Use）
=============
Create a class that inherits SystemModule or BusinessModule:
```c#
    public class MongoModule : SystemModule
    {
    	/// <summary>
        ///  Function module initialization,trigger when the module starts loading
        /// </summary>
        public override void Initialize() //Trigger when the module starts loading
        {
            base.Initialize();
        }
	
    	/// <summary>
        /// Inject dependent third-party components
        /// </summary>
        /// <param name="builder"></param>
        protected override void RegisterBuilder(ContainerBuilderWrapper builder)
        {
            base.RegisterBuilder(builder);
            builder.RegisterGeneric(typeof(MongoRepository<>)).As(typeof(IMongoRepository<>)).SingleInstance();
        }
    }
```


                
