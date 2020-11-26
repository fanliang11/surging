using Autofac;
using Autofac.Extensions.DependencyInjection;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Surging.Tools.Cli.Commands;
using Surging.Tools.Cli.Internal;
using Surging.Tools.Cli.Internal.Http;
using Surging.Tools.Cli.Internal.Implementation;
using Surging.Tools.Cli.Internal.MessagePack;
using Surging.Tools.Cli.Internal.Netty;
using Surging.Tools.Cli.Internal.Thrift;
using Surging.Tools.Cli.Utilities;
using System;

namespace Surging.Tools.Cli
{
    [HelpOption(Inherited = true)]
    [Command(Description = "command line terminal engine network request and configuration tool")]
    class Program
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CommandLineApplication _curlCommand;

        public Program()
        {
            _curlCommand = new CommandLineApplication<CurlCommand>();
            _serviceProvider = ConfigureServices();
           
        }

        static int Main(string[] args)
        {

            return new Program().Execute(args);
        }

  
        private int Execute(string[] args)
        {
            var app = new CommandLineApplication<Program>(); 
            app.AddSubcommand(_curlCommand);
            app.Conventions
                .UseDefaultConventions()
                .UseConstructorInjection(_serviceProvider);

            var console = (IConsole)_serviceProvider.GetService(typeof(IConsole));
            app.VersionOptionFromAssemblyAttributes("--version", typeof(Program).Assembly);
           
            try
            {
                return app.Execute(args);
            }
            catch (UnrecognizedCommandParsingException ex)
            {
                console.WriteLine(ex.Message);
                return -1;
            }
        }

        private  IServiceProvider ConfigureServices()
        {
            var serviceCollection = new ServiceCollection();
             serviceCollection.AddHttpClient();
            serviceCollection.AddSingleton<IConsole>(PhysicalConsole.Singleton);
            var builder = new ContainerBuilder();
            builder.Populate(serviceCollection);
            builder.RegisterType<HttpTransportClientFactory>().Named<ITransportClientFactory>("http").SingleInstance();
            builder.RegisterType<NettyTransportClientFactory>().Named<ITransportClientFactory>("netty").SingleInstance();
            builder.RegisterType<ThriftTransportClientFactory>().Named<ITransportClientFactory>("thrift").SingleInstance();
            builder.RegisterType<HttpClientProvider>().As<IHttpClientProvider>().SingleInstance();
            builder.RegisterType<MessagePackTransportMessageEncoder>().As<ITransportMessageEncoder>().SingleInstance();
            builder.RegisterType<MessagePackTransportMessageDecoder>().As<ITransportMessageDecoder>().SingleInstance();
            builder.RegisterType<MessagePackTransportMessageCodecFactory>().As<ITransportMessageCodecFactory>().SingleInstance();
            builder.RegisterType<DotNettyMessageClientSender>().Named<IMessageSender>("netty").SingleInstance();
            builder.RegisterType<ThriftMessageClientSender>().Named<IMessageSender>("thrift").SingleInstance();
            builder.RegisterType<MessageListener>().As<IMessageListener>().SingleInstance();
            builder.Register(provider=> _curlCommand).As<CommandLineApplication>().SingleInstance();
            ServiceLocator.Current = builder.Build();
            return serviceCollection.BuildServiceProvider();
        }

        private int OnExecute(CommandLineApplication app, IConsole console)
        {
            console.WriteLine("Please specify a command.");
            app.ShowHelp();
            return 1;
        }
    }
}
