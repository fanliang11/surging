using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Tools.Cli.Internal
{
    public interface IHttpClientProvider
    {
        Task<T> UploadFileAsync<T>(string requestUri, Dictionary<string, string> formData, IDictionary<string, string> headers);
        Task<T> PostJsonMessageAsync<T>(string requestUri, string message, IDictionary<string, string> headers);

        Task<T> GetJsonMessageAsync<T>(string requestUri, IDictionary<string, string> headers);

        Task<T> PutJsonMessageAsync<T>(string requestUri, string message, IDictionary<string, string> headers);

        Task<T> DeleteJsonMessageAsync<T>(string requestUri, string message, IDictionary<string, string> headers);
    }
}
