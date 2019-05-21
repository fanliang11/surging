using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Surging.Core.Common
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
