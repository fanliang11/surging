using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Surging.Tools.Cli.Commands;
using System;

namespace Surging.Tools.Cli
{
    [HelpOption(Inherited = true)]
    [Command(Description = "command line terminal engine network request and configuration tool"), Subcommand(typeof(CurlCommand))]
    class Program
    {
        private readonly IServiceProvider _serviceProvider;

        public Program()
        {
            _serviceProvider = ConfigureServices();
        }

        static int Main(string[] args)
        {

            return new Program().Execute(args);
        }

  


        private int Execute(string[] args)
        {
            var app = new CommandLineApplication<Program>();
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

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            return services.BuildServiceProvider();
        }

        private int OnExecute(CommandLineApplication app, IConsole console)
        {
            console.WriteLine("Please specify a command.");
            app.ShowHelp();
            return 1;
        }
    }
}
