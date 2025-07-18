using Microsoft.CodeAnalysis;
using Surging.Core.CPlatform.Codecs.Message;
using Surging.Core.DeviceGateway.Runtime.Core.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Device.Implementation.Http
{
    public class HttpRequestMessage: EncodedMessage
    {
 
        public  string Path { get;  set; }
         
        public string Url { get;   set; }


       public HttpMethod Method { get;   set; }

         
       public string ContentType { get;   set; }

         
        public  List<Header> Headers { get; set; }

       public  Dictionary<string,string>  QueryParameters { get;  set; }

        public Dictionary<string, string> GetRequestParam()
        {
            if (MediaType.ToString(MediaType.ApplicatioFormUrlencoded) == ContentType.ToString())
            {
                return PayloadAsString().ToUrlQuryDictionary();
            }
            return new Dictionary<string, string>();
        }

        public  object ParseBody()
        {
            if (MediaType.ToString(MediaType.ApplicationJson)== ContentType)
            {
                return PayloadAsJson().ToDictionary(p=>p.Key,p=>p.Value?.ToJsonString());
            }

            if (MediaType.ToString(MediaType.ApplicatioFormUrlencoded) == ContentType.ToString())
            {
                return PayloadAsString().ToUrlQuryDictionary();
            }

            return PayloadAsString();
        }

         public Header? GetHeader(string name)
        {
            return Headers.Where(header=>header.Name.Equals(name))
                    .FirstOrDefault();
        }

        public string? GetQueryParameter(String name)
        {
             QueryParameters.TryGetValue(name, out string? result);
            return result;
        } 
    }
}
