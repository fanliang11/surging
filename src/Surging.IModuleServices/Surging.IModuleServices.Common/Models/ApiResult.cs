

using System.Runtime.Serialization;

namespace Surging.IModuleServices.Common.Models
{
    [DataContract]
    public class ApiResult<T>
    {
        [DataMember]
        public int StatusCode { get; set; }

        [DataMember]
        public T Value { get; set; }
    }
}
