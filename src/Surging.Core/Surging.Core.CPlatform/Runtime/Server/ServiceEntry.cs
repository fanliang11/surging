using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Runtime.Server
{
    /// <summary>
    /// 服务条目。
    /// </summary>
    public class ServiceEntry
    {
        /// <summary>
        /// 执行委托。
        /// </summary>
        public  Func<string, IDictionary<string, object>, Task<object>> Func { get; set; }
       
        public string RoutePath { get; set; }
        public Type Type { get; set; }
        public string MethodName { get; set; }
        public List<Attribute> Attributes { get; set; }
        /// <summary>
        /// 服务描述符。
        /// </summary>
        public ServiceDescriptor Descriptor { get; set; }
    }
}