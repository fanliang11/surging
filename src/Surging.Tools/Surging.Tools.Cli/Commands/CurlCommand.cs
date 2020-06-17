using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Surging.Tools.Cli.Commands
{
    [Command("curl", Description = "Command line terminal network request tool, including http, ws, mqtt, dotnetty, thrifit protocol requests, some of which can only rely on the surging engine")]
    public class CurlCommand
    {
        /// <summary>
        /// ws://127.0.0.1:96/api/chat?name=abc
        /// thrift://127.0.0.1:96
        /// netty://127.0.0.1:98
        /// mqtt://127.0.0.1:97
        /// http://127.0.0.1:280
        [Argument(0, Name = "address")]
        [Required(ErrorMessage = "The Address is required.")]
        public string Address { get; set; }

        private void OnExecute(IConsole console)
        { 
            console.WriteLine(Address);
        }
    }
}
