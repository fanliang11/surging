using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Surging.Core.DeviceGateway.Runtime.Device.Implementation.Http
{
    public static class UriQueryExtensions
    {
        public static Dictionary<string, string> ToUrlQuryDictionary(this string query)
        {
            var result = new Dictionary<string, string>();
            var nameValueCollection = HttpUtility.ParseQueryString(query);

            foreach (var key in nameValueCollection.AllKeys)
            {
                result[key] = nameValueCollection[key];
            }

            return result;
        }
    }
}
