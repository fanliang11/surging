using Consul;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Text;

namespace Surging.Core.Consul.Utilitys
{
    public static class ConsulClientExtensions
    {
        public static async Task<string[]> GetChildrenAsync(this ConsulClient client, string path)
        {
            var host = client.Config.Address.Host;
            var port = client.Config.Address.Port;
            client.Config.Address = new Uri($"http://{host}:{port}");
            string clientPath = path;
            var queryResult = await client.KV.List(path);
            return  queryResult.Response?.Select(p=> Encoding.UTF8.GetString(p.Value)).ToArray();
        }

        public static async Task<byte[]> GetDataAsync(this ConsulClient client, string path)
        {
            var host = client.Config.Address.Host;
            var port = client.Config.Address.Port;
            var queryResult = await client.KV.Get(path);
            return queryResult.Response?.Value;
        }
    }
}

