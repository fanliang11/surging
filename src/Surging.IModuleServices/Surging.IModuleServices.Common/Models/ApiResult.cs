

using System.Runtime.Serialization;

namespace Surging.IModuleServices.Common.Models
{
    [DataContract]
    public class ApiResult<T>
    {
        
        public int StatusCode { get; set; }

        public T Value { get; set; }
    }
}
