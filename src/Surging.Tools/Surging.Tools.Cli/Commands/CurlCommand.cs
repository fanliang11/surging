using McMaster.Extensions.CommandLineUtils;
using Surging.Tools.Cli.Utilities;
using System;
using Autofac;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Surging.Tools.Cli.Internal;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;

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
        public string Address { get; }

        [Option("-X|--request", "http method", CommandOptionType.SingleValue)] 
        public string Method { get; } = "GET";


        [Option("-H|--header", "http request header", CommandOptionType.MultipleValue)]
        public string[] Header { get; }

        [Option("--serviceid", "remote service call parameter serviceid", CommandOptionType.SingleValue)]
        public string ServiceId { get; set; }

        [Option("--servicekey", "remote service call parameter servicekey", CommandOptionType.SingleValue)]
        public string ServiceKey { get; set; }

        [Option("--attachments", "remote service call invisible parameter transfer, CommandOptionType.SingleValue", CommandOptionType.SingleValue)]
        public string Attachments { get; set; }

        [Option("-F|--form", "http request form data", CommandOptionType.MultipleValue)]
        public string[] FormData { get; }

        [Option("-d|--data", "request content", CommandOptionType.SingleValue)]
        public string Data { get; }

        [Option("--mqtt-clientid", "mqtt clientid", CommandOptionType.SingleValue)]
        public string ClientID { get; }

        [Option("--mqtt-productid", "mqtt broker productid", CommandOptionType.SingleValue)]
        public string ProductId { get; }

        [Option("--mqtt-password", "mqtt broker password", CommandOptionType.SingleValue)]
        public string MqttPassword { get; }

        [Option("--mqtt-pub", "mqtt broker publish path", CommandOptionType.SingleValue)]
        public string MqttPublishPath { get; }

        private async Task OnExecute(CommandLineApplication app, IConsole console)
        {
            try
            {
                var uri = new Uri(Address);
                if (!ServiceLocator.IsRegistered<ITransportClientFactory>(uri.Scheme.ToLower()))
                {
                    console.ForegroundColor = ConsoleColor.Red;
                    console.WriteLine($"{uri.Scheme} not supported");
                }
                else
                {
                    var transportClientFactory = ServiceLocator.GetService<ITransportClientFactory>(uri.Scheme.ToLower());
                    var transportClient = await transportClientFactory.CreateClientAsync(new IPEndPoint(IPAddress.Parse(uri.Host), uri.Port));

                    var message = await transportClient.SendAsync(new System.Threading.CancellationToken());
                    if (message.StatusCode == 200)
                        console.WriteLine(JsonConvert.SerializeObject(message.Result));
                    else
                    {
                        console.ForegroundColor = ConsoleColor.Red;
                        console.WriteLine(JsonConvert.SerializeObject(message.Result));
                    }
                }
                console.ForegroundColor = ConsoleColor.White;
            }
            catch(Exception ex)
            {
                console.ForegroundColor = ConsoleColor.Red;
                console.WriteLine(ex.Message);
                console.WriteLine(ex.StackTrace);
                console.ForegroundColor = ConsoleColor.White; 
            }
        }
    }
}
