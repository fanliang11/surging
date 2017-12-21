using Consul;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Surging.Core.Consul.Utilitys
{
    public static class ConsulClientExtensions
    {
        public static async Task<string[]> GetChildrenAsync(this ConsulClient client, string path)
        {
            try
            {
                var queryResult = await client.KV.List(path);
                return queryResult.Response?.Select(p => Encoding.UTF8.GetString(p.Value)).ToArray();
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        public static async Task<byte[]> GetDataAsync(this ConsulClient client, string path)
        {
            try
            {
                var queryResult = await client.KV.Get(path);
                return queryResult.Response?.Value;
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }
    }
}

