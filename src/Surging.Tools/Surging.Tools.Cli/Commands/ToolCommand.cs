using McMaster.Extensions.CommandLineUtils;
using Surging.Tools.Cli.Internal;
using Surging.Tools.Cli.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Tools.Cli.Commands
{
    [Command("tool", Description = "Command line engine tool")]
    public class ToolCommand
    {
        [Option("--gc", "gc module", CommandOptionType.SingleValue)]
        public GCModuleType GCModuleName { get; set; }

        private async Task OnExecute(CommandLineApplication app, IConsole console)
        {
            try
            {
                GCModule(app, console); 

                console.ForegroundColor = ConsoleColor.White;
            }
            catch (Exception ex)
            {
                console.ForegroundColor = ConsoleColor.Red;
                console.WriteLine(ex.Message);
                console.WriteLine(ex.StackTrace);
                console.ForegroundColor = ConsoleColor.White;
            }
        }

        private void GCModule(CommandLineApplication app, IConsole console)
        {
            var gcModuleName = GCModuleName.ToString().ToLower(); 
            if (!ServiceLocator.IsRegistered<IGCModuleProvider>(gcModuleName))
            {
                console.ForegroundColor = ConsoleColor.Red;
                console.WriteLine($"{GCModuleName.ToString()} not supported");
            }
            else
            {
                var gcModuleProvider = ServiceLocator.GetService<IGCModuleProvider>(gcModuleName);
                gcModuleProvider.Collect();
            }
        }
    }
}
