using Consul;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Consul.Utilitys
{
    /// <summary>
    /// Defines the <see cref="ConsulClientExtensions" />
    /// </summary>
    public static class ConsulClientExtensions
    {
        #region 方法

        /// <summary>
        /// The GetChildrenAsync
        /// </summary>
        /// <param name="client">The client<see cref="ConsulClient"/></param>
        /// <param name="path">The path<see cref="string"/></param>
        /// <returns>The <see cref="Task{string[]}"/></returns>
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

        /// <summary>
        /// The GetDataAsync
        /// </summary>
        /// <param name="client">The client<see cref="ConsulClient"/></param>
        /// <param name="path">The path<see cref="string"/></param>
        /// <returns>The <see cref="Task{byte[]}"/></returns>
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

        #endregion 方法
    }
}